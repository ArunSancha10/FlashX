using Microsoft.EntityFrameworkCore;
using outofoffice.Models;
using System;
using System.Reflection.Metadata;

namespace outofoffice.App_code
{
    public class OOODbContext : DbContext
    {
        public OOODbContext(DbContextOptions<OOODbContext> options)
          : base(options)
        {
        }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<MessageAppList> MessageAppLists { get; set; }
        public DbSet<UserAppMessage> UserAppMessages { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Company Configuration
            modelBuilder.Entity<Company>()
                .HasKey(c => c.Company_ID);

            // Group Configuration
            modelBuilder.Entity<Group>()
                .HasKey(g => g.Group_ID);
            modelBuilder.Entity<Group>()
                .HasOne(g => g.Company)
                .WithMany(c => c.Groups)
                .HasForeignKey(g => g.Company_ID);

            // MessageAppList Configuration
            modelBuilder.Entity<MessageAppList>()
                .HasKey(ma => ma.MAID);

            modelBuilder.Entity<MessageAppList>()
                .HasOne(ma => ma.Group)
                .WithMany(g => g.MessageApps)
                .HasForeignKey(ma => ma.Group_ID);

            // UserAppMessage Configuration
            modelBuilder.Entity<UserAppMessage>()
                .HasKey(um => um.UAID);
            modelBuilder.Entity<UserAppMessage>()
                .HasOne(um => um.Group)
                .WithMany(g => g.UserMessages)
                .HasForeignKey(um => um.Group_ID);
            modelBuilder.Entity<UserAppMessage>()
                .Property(b => b.CreatedDate)
                .HasDefaultValueSql("getdate()");
            

        }
    }
}
