using BLL.Specification.Class;
using DAL.Models;
using System;

namespace DAL.Specification.Class
{
    public class PaymentByPassengerSpecification : Specification<Payment>
    {
        public PaymentByPassengerSpecification(Guid passengerId)
            : base(p => p.Trip != null && p.Trip.PassengerId == passengerId)
        {
            AddInclude(p => p.Trip);
            AddOrderByDescending(p => p.ProcessedAt);
        }
    }
}