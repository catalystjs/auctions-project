// Common Libraries
using System;
using System.Collections.Generic;
// Validation Annotations
using System.ComponentModel.DataAnnotations;
// LINQ Libraries
using System.Linq;
// Project Model Validations
using beltexam.ModelValidations;

namespace beltexam.Models
{
    public class Auction : BaseEntity
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MinLength(3)]
        public string Product { get; set; }
        [Required]
        [MinLength(10)]
        public string Description { get; set; }
        [Required]
        [Display(Name = "Start Price")]
        [DataType(DataType.Currency)]
        [Range(1, int.MaxValue, ErrorMessage = "Please enter a value bigger than {1}")]
        public float StartPrice { get; set; }
        [Required]
        [Display(Name = "End Date")]
        [CheckDateRangeAttribute]
        [DataType(DataType.Date)]
        public DateTime Remaining { get; set; }
        public bool Paid { get; set; }
        // Foreign Keys
        [Required]
        public int UserId { get; set; }
        public User User { get; set; }
        public ICollection<Bid> Bids { get; set;}
        public DateTime CreatedAt { set; get; }
        // This is the constructor
        public Auction()
        {
            CreatedAt = System.DateTime.Now;
            Bids = new List<Bid>();
            Paid = false;
        }
        // String override
        public override string ToString()
        {
            return $"Auction Data: ID: {this.Id}, Product: {this.Product}, User ID: {this.UserId}, Remaining: {this.Remaining}";
        }
        // Get the top bid for the auction
        public Bid GetTopBid()
        {
            Bid topbid = null;
            foreach (var bid in Bids)
            {
                if (topbid == null)
                {
                    topbid = bid;
                }
                else if (bid.UserBid > topbid.UserBid)
                {
                    topbid = bid;
                }
            }
            return topbid;
        }
        // Determine the time remaining in time span format
        public string TimeSpanRemaining()
        {
            // Do the logic to format the time according to TimeSpan information
            DateTime current = System.DateTime.Now;
            TimeSpan remaining = Remaining - current;
            // Format the Timespan and the output for the table data
            if (remaining.Days > 0) {return $"{remaining.Days.ToString()} days";}
            else if (remaining.Hours > 0) {return $"{remaining.Hours.ToString()} hours";}
            else if (remaining.Minutes > 0) {return $"{remaining.Minutes.ToString()} minutes";}
            else if (remaining.Seconds > 0) {return $"{remaining.Seconds.ToString()} seconds";}
            // Return null if expired
            else {return null;}
        }
        // This return the user that can pay for an auction
        public User UserWhoCanPay()
        {
            // Initialize payee
            User payee= null;
            // Make sure you can deduct the money from the Top bidder
            Bid topbid = GetTopBid();
            // Only process if there is a current top bid
            if (topbid != null)
                {
                float remainder = topbid.User.Money - topbid.UserBid;
                if (remainder >= 0)
                {
                    payee = topbid.User;
                }
                else
                {
                    // Loop through the bids until one can cover the cost
                    foreach (var bid in Bids.Reverse())
                    {
                        remainder = bid.User.Money - bid.UserBid;
                        if (remainder >= 0)
                        {
                            payee = topbid.User;
                            break;
                        }
                    }
                }
            }
            // Return the payee
            return payee;
        }
    }
}