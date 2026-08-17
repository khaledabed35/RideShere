using BLL.Specification.Class;
using DAL.Models;
using System;

namespace DAL.Specification.Class
{
    public class DriverDocumentSpecification : Specification<DriverDocument>
    {
        // 1. جلب وثائق السائق برقم المعرف الخاص به (بديل DriverDocumentByUserIdSpecification)
        public DriverDocumentSpecification(Guid userId)
            : base(d => d.DriverId == userId)
        {
        }

        // 2. جلب تفاصيل وثائق السائق للإدارة مع تضمين بيانات السائق الأساسية (بديل DriverDetailsForAdminSpecification)
        public DriverDocumentSpecification(Guid driverId, bool isAdminDetails)
            : base(d => d.DriverId == driverId)
        {
            AddInclude(d => d.Driver);
        }
    }
}