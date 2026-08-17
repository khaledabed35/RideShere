using BLL.Specification.Class;
using DAL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Specification.Class
{
    public class PendingDriversSpecification : Specification<DriverDocument>
    {

        public PendingDriversSpecification(int pageNumber, int pageSize)
            : base(d => d.Status == Driveracceptstatus.Pending)
        {
            // جلب بيانات اليوزر المرتبطة بالسائق لعرضها للأدمن
            AddInclude(d => d.Driver);

            // تطبيق الـ Pagination (تأكد أن الـ BaseSpecification عندك تدعم ApplyPaging أو الخاصيتين Skip و Take)
            ApplyPaging(pageSize * (pageNumber - 1), pageSize);
        }
    }
}