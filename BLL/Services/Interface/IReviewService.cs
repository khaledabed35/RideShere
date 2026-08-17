using BLL.DTOs;

public interface IReviewService
{
    Task<DriverReviewDto> AddReviewAsync(
      Guid passengerId,
      AddReviewDto reviewDto);

    Task<DriverReviewDto> UpdateReviewAsync(
      Guid passengerId,
      Guid reviewId,
      UpdateReviewDto reviewDto);

    Task<bool> DeleteReviewAsync(
      Guid passengerId,
      Guid reviewId);

    Task<IEnumerable<DriverReviewDto>> GetReviewsByDriverIdAsync(
      Guid driverId);

    Task<DriverRatingDto> GetDriverRatingAsync(
      Guid driverId);
}