using BLL.Specification.Class;
using DAL.Models;
using System;

namespace DAL.Specification.Class
{
    public class DriverSpecification : Specification<Driver>
    {
        // 1. استعلام عام مع ترقيم الصفحات (Paging)
        public DriverSpecification(UserQueryParameters queryParams) : base(d =>
                string.IsNullOrEmpty(queryParams.Search) ||
                d.Name.Contains(queryParams.Search) ||
                d.PhoneNumber.Contains(queryParams.Search)
            )
        {
            AddInclude(d => d.User);
            AddInclude(d => d.DriverDocument);
            ApplyPaging(queryParams.pagesize * (queryParams.pageidex - 1), queryParams.pagesize);
        }

        // 2. استعلام برعاية حالة السائق (Pending, Approved, Rejected)
        public DriverSpecification(UserQueryParameters queryParams, Driveracceptstatus status) : base(d =>
                d.DriverDocument != null && d.DriverDocument.Status == status &&
                (string.IsNullOrEmpty(queryParams.Search) ||
                d.Name.Contains(queryParams.Search) ||
                d.PhoneNumber.Contains(queryParams.Search))
            )
        {
            AddInclude(d => d.User);
            AddInclude(d => d.DriverDocument);
            ApplyPaging(queryParams.pagesize * (queryParams.pageidex - 1), queryParams.pagesize);
        }

        // 3. استعلام لحساب العدد (Count) بدون Paging
        public DriverSpecification(UserQueryParameters queryParams, Driveracceptstatus status, bool isCountCriteria) : base(d =>
                d.DriverDocument != null && d.DriverDocument.Status == status &&
                (string.IsNullOrEmpty(queryParams.Search) ||
                d.Name.Contains(queryParams.Search) ||
                d.PhoneNumber.Contains(queryParams.Search))
            )
        {
            AddInclude(d => d.User);
            AddInclude(d => d.DriverDocument);
        }

        // Methods للوصول السريع لطلبات السائقين المعلقة (Pending)
        public static DriverSpecification GetPendingDriversQueueSpec(UserQueryParameters queryParams)
        {
            var spec = new DriverSpecification(queryParams, Driveracceptstatus.Pending);
            spec.AddOrderByDescending(d => d.DriverDocument.CreatedAt);
            return spec;
        }

        public static DriverSpecification GetPendingDriversCountSpec(UserQueryParameters queryParams)
        {
            var spec = new DriverSpecification(queryParams, Driveracceptstatus.Pending, true);
            
            return spec;
        }
        public DriverSpecification(List<Guid> driverIds) : base(d => driverIds.Contains(d.Id))
        {
            // لا نحتاج Includes هنا غالباً، أو يمكنك إضافتها لو احتجت بيانات مرتبطة
        }
    }
}