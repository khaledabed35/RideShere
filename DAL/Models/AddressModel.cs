using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.Json.Serialization;

namespace DAL.Models
{
    public class AddressModel
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [JsonIgnore]
        public Guid UserId { get; set; }

        [MaxLength(20), MinLength(3), Required]
        public string AddressName { get; set; }
        [MaxLength(20), MinLength(3), Required]
        public string City { get; set; }
        [MaxLength(20), MinLength(3), Required]
        public string Address { get; set; }

        [ForeignKey("UserId")]
        [JsonIgnore]
        public App_User User { get; set; }
    }
}
