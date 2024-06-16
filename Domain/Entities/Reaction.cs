using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Reaction
    {
        [Key]
        public int ReactionId { get; set; }
       
        public Guid UserId { get; set; }

        public int NewsId { get; set; }
        public int ReactionType { get; set; }
        public IdentityUser User { get; set; }
        public News News { get; set; }


    }
}
