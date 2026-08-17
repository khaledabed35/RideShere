using BLL.Specification.Class;
using DAL.Models;

public class DriverReviewsSpecification : Specification<Review>
{
    public DriverReviewsSpecification(Guid driverId)
        : base(r => r.DriverId == driverId)
    {
        AddInclude(r => r.Reviewer);
    }
}