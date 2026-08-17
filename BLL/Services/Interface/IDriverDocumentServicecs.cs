using BLL.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Services.Interface
{
    public interface IDriverDocumentService
    {
        Task<DriverDocumentDto> GetDocumentByIdAsync(int documentId);

        Task<DriverDocumentDto> GetDocumentsByDriverIdAsync(Guid driverId);

        Task<DriverDocumentDto> UploadDocumentAsync(Guid driverId, AddDriverDocumentDto documentDto);

        Task<DriverDocumentDto> UpdateDocumentStatusAsync(int documentId, UpdateDriverDocumentStatusDto statusDto);

        Task<bool> DeleteDocumentAsync(Guid driverId, int documentId);
    }
}