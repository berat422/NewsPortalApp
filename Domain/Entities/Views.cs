using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Views
    {
        [Key]
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public string FingerPrintId { get; set; }
        public int NewsId { get; set; }
        public News News { get; set; }

        public IdentityUser User { get; set; }
        public DateTime ViewedOn { get; set; }


    }
}
