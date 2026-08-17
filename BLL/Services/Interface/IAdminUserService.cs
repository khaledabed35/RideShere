using BLL.Specification.Class; // مساحة الكلاسات بتاعت الـ Query
using DAL.DTOs.Auth;
using DAL.DTOs.DriverDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Services.Interface
{
    public interface IAdminUserService
    {
        Task<IEnumerable<UserDto>> ListAllUsersAsync(UserQueryParameters queryParams);
        Task<UserDto> GetUserByIdAsync(Guid userId);
        Task SuspendUserAsync(Guid userId);
        Task UnBlockUserAsync(Guid userId);
        Task<IEnumerable<PendingDriverDto>> GetPendingDriversQueueAsync(int pageNumber = 1, int pageSize = 10);

        Task<DriverDetailsDto> GetDriverDetailsForAdminAsync(Guid driverId);
        Task<bool> ApproveDriverAsync(Guid driverId);
        Task RejectDriverAsync(Guid driverId, string rejectionReason);
    }
}