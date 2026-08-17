using BLL.Specification.Class;
using DAL.Models;
using System;

namespace DAL.Specification.Class
{
    public class UserSpecification : Specification<App_User>
    {
        public UserSpecification(UserQueryParameters queryParams) : base(u =>
                string.IsNullOrEmpty(queryParams.Search) ||
                u.UserName.Contains(queryParams.Search) ||
                u.Email.Contains(queryParams.Search)
            )
        {
            AddInclude(u => u.Addresses);

            ApplyPaging(queryParams.pagesize * (queryParams.pageidex - 1), queryParams.pagesize);
        }

        public UserSpecification(UserQueryParameters queryParams, bool isCountCriteria) : base(u =>
                string.IsNullOrEmpty(queryParams.Search) ||
                u.UserName.Contains(queryParams.Search) ||
                u.Email.Contains(queryParams.Search)
            )
        {
        }

        public static UserSpecification GetUsersQueueSpec(UserQueryParameters queryParams)
        {
            var spec = new UserSpecification(queryParams);
            spec.AddOrderByDescending(u => u.CreatedAt);
            return spec;
        }

        public static UserSpecification GetUsersCountSpec(UserQueryParameters queryParams)
        {
            var spec = new UserSpecification(queryParams, true);
            return spec;
        }
    }
}