// Common Libraries
using System;
// Validation Annotations
using System.ComponentModel.DataAnnotations;

namespace beltexam.Models
{
    public class Bid : BaseEntity
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [DisplayFormat(DataFormatString = "{$#.##}")]
        [Display(Name = "Your Bid")]
        [Range(1, int.MaxValue, ErrorMessage = "Please enter a value bigger than {1}")]
        public float UserBid { get; set; }
        [Required]
        public int UserId { get; set; }
        public User User { get; set; }
        // Foreign Keys
        [Required]
        public int AuctionId { get; set; }
        public Auction Auction { get; set; }
        public DateTime CreatedAt { set; get; }
        // This is the constructor
        public Bid()
        {
            CreatedAt = System.DateTime.Now;
        }
        // String override
        public override string ToString()
        {
            return $"Bid Data: ID: {this.Id}, User ID: {this.UserId}, Auction ID: {this.AuctionId}, Bid: {this.UserBid}";
        }

    }

}