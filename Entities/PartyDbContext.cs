using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace PartyRecordApp.Entities
{
    /// <summary>
    /// This class will inherit from the Entity Framework (EF) class
    /// called DbContext and is used by the code to interact with the DB
    /// </summary>
    public class PartyDbContext : DbContext
    {
        /// <summary>
        /// Define a constructor that simply passes the options argument
        /// up to the base class constuctor
        /// </summary>
        /// <param name="options"></param>
        /// 
        public PartyDbContext(DbContextOptions<PartyDbContext> options)
            : base(options) { }



        // Adding a property to access all Party
        public DbSet<Party> Party { get; set; }



        // Adding a property to access all Programs
        public DbSet<Invitation> Invitation { get; set; }



        // override the OnModelBuilding method as a place to do init'n
        // which for us will be to seed the DB w some data:
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            // Define primary keys
            modelBuilder.Entity<Party>().HasKey(p => p.PartyId);
            modelBuilder.Entity<Invitation>().HasKey(i => i.InvitationId);

            // Define relationships
            modelBuilder.Entity<Invitation>()
                .HasOne(i => i.Party)
                .WithMany(p => p.Invitations)
                .HasForeignKey(i => i.PartyId);

            // Seed some invitation
            modelBuilder.Entity<Invitation>()
                .Property(inv => inv.Status)
                .HasConversion<string>()
                .HasMaxLength(64);

            // Seed the DB with some data
            modelBuilder.Entity<Party>().HasData(
                new Party { PartyId = 1, EventDescription = "Party 1 Description", EventLocation = "Location 1", EventDate = "2024-03-02"},
                new Party { PartyId = 2, EventDescription = "Party 2 Description", EventLocation = "Location 2", EventDate = "2024-03-03"}
            );

            modelBuilder.Entity<Invitation>().HasData(
                new Invitation { InvitationId = 1, GuestName = "Guest 1", GuestEmail = "guest1@example.com", Status = InvitationStatus.InviteSent, PartyId = 1 },
                new Invitation { InvitationId = 2, GuestName = "Guest 2", GuestEmail = "guest2@example.com", Status = InvitationStatus.InviteSent, PartyId = 2 }
            );
        }
    }
}
