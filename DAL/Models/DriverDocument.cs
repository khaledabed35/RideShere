using System;

namespace DAL.Models
{
    public enum Driveracceptstatus
    {
        Pending,
        Approved,
        Rejected
    }

    public class DriverDocument
    {
        public int Id { get; set; }
        public Guid DriverId { get; set; } // الربط المباشر بجدول السواقين

        // بيانات الرخصة والسيارة
        public string LicenseNumber { get; set; } = string.Empty;
        public string CarModel { get; set; } = string.Empty;
        public string CarPlateNumber { get; set; } = string.Empty;
        public string CarCategory { get; set; } = string.Empty;

        // روابط المستندات والورق
        public string NationalIdImageUrl { get; set; } = string.Empty;
        public string DriverLicenseImageUrl { get; set; } = string.Empty;
        public string CarLicenseImageUrl { get; set; } = string.Empty;

        // الحالة وسبب الرفض (إن وجد)
        public Driveracceptstatus Status { get; set; } = Driveracceptstatus.Pending;
        public string? RejectionReason { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        // العلاقة مع الـ Driver بدل الـ User
        public Driver? Driver { get; set; }
    }
}