using BLL.Specification.Class;
using DAL.Models;
using System;

namespace DAL.Specification.Class
{
    public class OfferSpecification : Specification<Offer>
    {
        public OfferSpecification(Guid offerId) : base(o => o.Id == offerId)
        {
            AddInclude(o => o.Trip);
        }

        public OfferSpecification(Guid tripId, bool isForAllTripOffers) : base(o => o.TripId == tripId)
        {
        }

        // 3. جلب عروض رحلة بحالة معينة (مثل Pending) مع تفاصيل السائق (بديل OffersWithDriverDetailsSpecification)
        public OfferSpecification(Guid tripId, OfferStatus status) : base(o => o.TripId == tripId && o.Status == status)
        {
            AddInclude(o => o.Driver);
            AddInclude(o => o.Driver.User);
            AddInclude(o => o.Driver.DriverDocument);
        }
    }
}