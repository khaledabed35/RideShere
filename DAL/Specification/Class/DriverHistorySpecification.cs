using BLL.Specification.Class;
using DAL.Models;

namespace DAL.Specification
{
    public class DriverHistorySpecification : Specification<Trip>
    {
        public DriverHistorySpecification(Guid driverId, string filterPeriod)
            : base(t => t.DriverId == driverId &&
                  (t.Status == TripStatus.Completed || t.Status == TripStatus.Cancelled))
        {
            // تطبيق فلترة الوقت
            var now = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(filterPeriod))
            {
                switch (filterPeriod.ToLower())
                {
                    case "today":
                        Criteria = t => t.DriverId == driverId &&
                                       (t.Status == TripStatus.Completed || t.Status == TripStatus.Cancelled) &&
                                       t.CreateAt.Date == now.Date;
                        break;
                    case "week":
                        var weekAgo = now.AddDays(-7);
                        Criteria = t => t.DriverId == driverId &&
                                       (t.Status == TripStatus.Completed || t.Status == TripStatus.Cancelled) &&
                                       t.CreateAt >= weekAgo;
                        break;
                    case "month":
                        var monthAgo = now.AddMonths(-1);
                        Criteria = t => t.DriverId == driverId &&
                                       (t.Status == TripStatus.Completed || t.Status == TripStatus.Cancelled) &&
                                       t.CreateAt >= monthAgo;
                        break;
                }
            }

            AddOrderByDescending(t => t.CreateAt);
        }
    }
}