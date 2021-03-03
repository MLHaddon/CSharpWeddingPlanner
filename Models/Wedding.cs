using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WeddingPlanner.Validations;

namespace WeddingPlanner.Models
{
    public class Wedding
    {
        [Key]
        public int WeddingID {get; set;}

        public User Owner {get; set;}

        [Required]
        [Display(Name = "Wedder One")]
        public string WedderOne {get; set;}

        [Required]
        [Display(Name = "Wedder Two")]
        public string WedderTwo {get; set;}

        [Required]
        [ExpiredDate]
        public DateTime Date {get; set;}

        [Required]
        [MaxLength(100, ErrorMessage = "Address cannot be more than 100 characters")]
        public string Address {get; set;}
        public DateTime CreatedAt {get; set;} = DateTime.Now;
        public DateTime UpdatedAt {get; set;} = DateTime.Now;
        public List<ManyToMany> Guests {get; set;}
    }
}