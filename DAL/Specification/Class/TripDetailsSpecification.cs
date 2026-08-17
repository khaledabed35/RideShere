using BLL.Specification.Class;
using DAL.Models;
using DAL.Specification;
using System;

namespace DAL.Specification.Class
{
    public class TripSpecification : Specification<Trip>
    {
        // 1. رحلة نشطة للراكب مع السائق والسيارة (بديل TripWithDriverSpecification)
        public TripSpecification(Guid passengerId)
            : base(t => t.PassengerId == passengerId &&
                        t.Status != TripStatus.Completed &&
                        t.Status != TripStatus.Cancelled)
        {
            AddInclude(t => t.Driver);
            AddInclude(t => t.Car);
        }

        // 2. سجل رحلات المستخدم سواء كان راكب أو سائق (بديل TripHistorySpecification)
        public TripSpecification(Guid userId, string historyFlag)
            : base(t => t.PassengerId == userId || t.DriverId == userId)
        {
            AddInclude(t => t.Driver);
            AddInclude(t => t.Car);
            // AddOrderByDescending(t => t.CreateAt);
        }

        // 3. تفاصيل رحلة معينة مع السائق، السيارة، اللوجز، والعروض (بديل TripDetailsSpecification)
        public TripSpecification(Guid tripId, Guid userId)
            : base(t => t.Id == tripId && (t.PassengerId == userId || t.DriverId == userId))
        {
            AddInclude(t => t.Driver);
            AddInclude(t => t.Car);
            AddInclude(t => t.TripStatusLogs);
            AddInclude(t => t.Offers);
        }

        // 4. الرحلات المتاحة التي لم تنتهِ صلاحيتها (بديل AvailableTripsWithPassengerSpecification)
        public TripSpecification(DateTime thresholdTime)
            : base(t => t.Status == TripStatus.Requested && t.CreateAt >= thresholdTime)
        {
            AddInclude(t => t.Passenger);
        }

        // 5. الرحلة النشطة لمستخدم معين سواء راكب أو سائق مع العروض (بديل ActiveTripSpecification)
        public TripSpecification(Guid userId, bool isActiveTrip, int overloadProtector)
            : base(t => (t.PassengerId == userId || t.DriverId == userId) &&
                        t.Status != TripStatus.Completed &&
                        t.Status != TripStatus.Cancelled)
        {
            AddInclude(t => t.Driver);
            AddInclude(t => t.Offers);
        }
    }
}