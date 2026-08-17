using DAL.DTOs.Auth;
using DAL.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Services.Interface
{
    public interface IUserService
    {
        
        Task<UserprofileDto> GetUserProfileAsync(Guid userId);

        Task UpdateUserProfileAsync(Guid userId, UpdateUserProfileDto model);

        Task<string> UploadProfileImageAsync(Guid userId, IFormFile image);

        Task DeleteAccountAsync(Guid userId);


    }
}
