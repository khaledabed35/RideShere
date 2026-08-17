using BLL.Error;
using BLL.Services.Interface;
using DAL.DTOs.Auth;
using DAL.Models;
using DAL.Reposetoriy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Services.Class
{
    public class UserService : IUserService
    {
        private readonly IUserRepo _userRepo;
        private readonly IGenaricRepo<AddressModel> _addressRepo;
        private readonly UserManager<App_User> _userManager;

        public UserService(
            IUserRepo userRepo,
            IGenaricRepo<AddressModel> addressRepo,
            UserManager<App_User> userManager)
        {
            _userRepo = userRepo;
            _addressRepo = addressRepo;
            _userManager = userManager;
        }

        public async Task DeleteAccountAsync(Guid userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("User not found.");
            }

            var allAddresses = await _addressRepo.GetAllAsync();
            var userAddresses = allAddresses.Where(a => a.UserId == userId).ToList();

            foreach (var address in userAddresses)
            {
                _addressRepo.Delete(address);
            }

            _userRepo.Delete(user);
            await _userRepo.SaveChangesAsync();
        }

        public async Task<UserprofileDto> GetUserProfileAsync(Guid userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("User not found.");
            }

            return new UserprofileDto
            {
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                ProfilePictureUrl = user.ImageUrl,
            };
        }

        public async Task UpdateUserProfileAsync(Guid userId, UpdateUserProfileDto model)
        {
            // استخدام UserManager للتعامل الآمن مع بيانات الـ Identity وتحديث الـ Normalized fields تلقائياً
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                throw new NotFoundException("User not found.");
            }

            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.ImageUrl = model.ProfilePictureUrl;

            // تحديث الـ UserName بالطريقة الصحيحة لضمان تحديث الـ NormalizedUserName معه
            if (!string.Equals(user.UserName, model.UserName, StringComparison.OrdinalIgnoreCase))
            {
                var setUserNameResult = await _userManager.SetUserNameAsync(user, model.UserName);
                if (!setUserNameResult.Succeeded)
                {
                    var errors = string.Join(", ", setUserNameResult.Errors.Select(e => e.Description));
                    throw new BadRequestException($"Failed to update username: {errors}");
                }
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new BadRequestException($"Failed to update user profile: {errors}");
            }
        }

        public async Task<string> UploadProfileImageAsync(Guid userId, IFormFile image)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("User not found.");
            }

            if (image == null || image.Length == 0)
                throw new BadRequestException("Image is invalid or null.");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(image.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
            {
                throw new BadRequestException("Invalid image extension.");
            }

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profile_images");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // 1. حذف الصورة القديمة لو موجودة
            if (!string.IsNullOrEmpty(user.ImageUrl))
            {
                var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ImageUrl.TrimStart('/'));

                if (File.Exists(oldImagePath))
                {
                    File.Delete(oldImagePath);
                }
            }

            // 2. حفظ الصورة الجديدة
            var uniqueFileName = Guid.NewGuid().ToString() + extension;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            // 3. تحديث المسار الجديد في كائن المستخدم
            user.ImageUrl = $"/uploads/profile_images/{uniqueFileName}";

            _userRepo.Update(user);
            await _userRepo.SaveChangesAsync();

            return "image uploaded successfully";
        }
    }
}