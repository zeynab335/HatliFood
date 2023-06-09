﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace HatliFood.Models
{

    [Table("DeliveryGuy")]
    public partial class DeliveryGuy : IPerson
    {
        [ForeignKey(nameof(User)), Key]
        public string? UserId { get; set; }
        public IdentityUser? User { get; set; }

        [Required]
        public string Name { get; set; }

        public string PhoneNumber { get; set; }


        public virtual ICollection<Order>? DOrders { get; set; } = new HashSet<Order>();
    }

}
