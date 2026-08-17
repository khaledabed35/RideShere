using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DAL.Models
{
 [Table("TripStatusLogs")]
        public class TripStatusLog
        {
            [Key]
            public Guid Id { get; set; } = Guid.NewGuid();

            [Required]
            public Guid TripId { get; set; } 

            [Required]
            [Column(TypeName = "varchar(20)")]
            public TripStatus Status { get; set; }

            [Required]
            public DateTime ChangedAt { get; set; } = DateTime.UtcNow; 


            [ForeignKey(nameof(TripId))]
            public virtual Trip Trip { get; set; } = null!;
        public string Notes { get; set; }
        public DateTime createdAt { get; set; }
    }
    }
