// Common Libraries
using System.Collections.Generic;
// LINQ Libraries
using System.Linq;
// ASP.NET Entity Framework
using Microsoft.EntityFrameworkCore;

namespace beltexam.Models
{
    public class AuctionContext : DbContext
    {
        public AuctionContext(DbContextOptions<AuctionContext> options) : base(options) { }
        // These are the tables
        public DbSet<User> Users { get; set; }
        public DbSet<Auction> Auctions { get; set; }
        public DbSet<Bid> Bids { get; set; }
        // Override the SaveChanges to handle CreatedAt and UpdatedAt
        public override int SaveChanges()
        {
            // update entities that are tracked and inherit from BaseEntity (UpdatedAt Property)
            var trackables = ChangeTracker.Entries<BaseEntity>().Where(x => x.State == EntityState.Added || x.State == EntityState.Modified);
            if (trackables != null)
            {
                // added and modified changes only
                foreach (var item in trackables)
                {
                    item.Entity.UpdatedAt = System.DateTime.Now;
                }
            }
            // Return the base SaveChanges
            return base.SaveChanges();
        }
        // Full populate methods for DRY
        public Auction PopulateAuctionSingle(int auctionid)
        {
            return Auctions.Where(x => x.Id == auctionid).Include(x => x.User).Include(x => x.Bids).ThenInclude(x => x.User).SingleOrDefault();
        }
        public ICollection<Auction> PopulateAuctionAllOrderByRemaining()
        {
            return Auctions.Include(x => x.User).Include(x => x.Bids).ThenInclude(x => x.User).OrderBy(x => x.Remaining).ToList();
        }
        // This will process an auction that has expired and not been paid
        public void ProcessExpiredAuctions()
        {
            // Get the aution object
            ICollection<Auction> auctions = PopulateAuctionAllOrderByRemaining();
            // Handle the expired auctions before we View them (Candidate for async)
            foreach (var auction in auctions)
            {
                if (auction.TimeSpanRemaining() == null && !auction.Paid)
                {
                    // Grab the user that can pay
                    User payee = auction.UserWhoCanPay();
                    if (payee != null)
                    {
                        //Make sure we query the user so the changes get saved
                        User payee_record = Users.Where(x => x.Id == payee.Id).SingleOrDefault();
                        // Get the top bid for this auction
                        Bid topbid = auction.GetTopBid();
                        // Deduct the money from the payee
                        payee_record.Money -= topbid.UserBid;
                        // Set the auction to paid
                        auction.Paid = true;
                        // Save the table changes
                        SaveChanges();
                    }
                }
            }
        }
    }
}