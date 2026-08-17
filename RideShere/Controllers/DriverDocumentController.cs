using BLL.DTOs;
using BLL.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RideShere.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DriverDocumentController : ControllerBase
    {
        private readonly IDriverDocumentService _documentService;

        public DriverDocumentController(IDriverDocumentService documentService)
        {
            _documentService = documentService;
        }

        /// <span class="math-inline">ἰ) Driver Upload Documents
        [HttpPost("upload")]
        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> UploadDocument([FromBody] AddDriverDocumentDto documentDto)
        {
            var driverId = GetUserIdFromClaims();
            var result = await _documentService.UploadDocumentAsync(driverId, documentDto);

            return CreatedAtAction(
                nameof(GetMyDocument),
                new { id = result.Id },
                result);
        }

        /// </span>ἰi) Driver Get Own Documents (1-to-1)
        [HttpGet("me")]
        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> GetMyDocument()
        {
            var driverId = GetUserIdFromClaims();
            var document = await _documentService.GetDocumentsByDriverIdAsync(driverId);
            return Ok(document);
        }

        /// <span class="math-inline">iii\) Driver Delete Own Document \(if not approved\)
        [HttpDelete("{documentId}")]
        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> DeleteDocument(int documentId)
        {
            var driverId = GetUserIdFromClaims();
            await _documentService.DeleteDocumentAsync(driverId, documentId);
            return NoContent(); // 204 No Content for successful deletion
        }

        /// </span>iv) Admin Get Any Document by ID
        [HttpGet("{documentId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDocumentById(int documentId)
        {
            var document = await _documentService.GetDocumentByIdAsync(documentId);
            return Ok(document);
        }

        /// <span class="math-inline">v\) Admin Update Document Status \(Approve / Reject\)
        [HttpPatch("{documentId}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateDocumentStatus(int documentId, [FromBody] UpdateDriverDocumentStatusDto statusDto)
        {
            var result = await _documentService.UpdateDocumentStatusAsync(documentId, statusDto);
            return Ok(result);
        }

        // Helper Method to extract Driver ID securely from JWT Claims
        private Guid GetUserIdFromClaims()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? User.FindFirst("sub")?.Value
                            ?? User.FindFirst("uid")?.Value;

            if (!Guid.TryParse(userIdStr, out var userId))
            {
                throw new UnauthorizedAccessException("Invalid or missing user token.");
            }

            return userId;
        }
    }
}