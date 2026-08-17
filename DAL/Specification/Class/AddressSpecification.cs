using BLL.Specification.Class;
using DAL.Models;
using System;

namespace DAL.Specification.Class
{
    public class AddressSpecification : Specification<AddressModel>
    {
        public AddressSpecification(Guid userId, string? search = null)
            : base(x => x.UserId == userId &&
                  (string.IsNullOrEmpty(search) ||
                   (!string.IsNullOrEmpty(x.AddressName) && x.AddressName.Contains(search)) ||
                   (!string.IsNullOrEmpty(x.City) && x.City.Contains(search)) ||
                   (!string.IsNullOrEmpty(x.Address) && x.Address.Contains(search))))
        {
        }

        public AddressSpecification(Guid userId, int id)
            : base(x => x.Id == id && x.UserId == userId)
        {
        }
    }
}