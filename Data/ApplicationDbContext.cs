using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using EjustLostAndFoundHub.Models;

namespace EjustLostAndFoundHub.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        // Constructor to initialize the DbContext with options
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // The Master Table
        public DbSet<ItemModel> Items { get; set; }

        // The Relational Category Tables
        public DbSet<IdItem> IdItems { get; set; }
        public DbSet<DeviceItem> DeviceItems { get; set; }
        public DbSet<WalletItem> WalletItems { get; set; }
        public DbSet<JewelryItem> JewelryItems { get; set; }
        public DbSet<NotebookItem> NotebookItems { get; set; }

        // Override the OnModelCreating method to configure the model
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the table names for each entity
            modelBuilder.Entity<ItemModel>().ToTable("Items");
            modelBuilder.Entity<IdItem>().ToTable("IdItems");
            modelBuilder.Entity<DeviceItem>().ToTable("DeviceItems");
            modelBuilder.Entity<WalletItem>().ToTable("WalletItems");
            modelBuilder.Entity<JewelryItem>().ToTable("JewelryItems");
            modelBuilder.Entity<NotebookItem>().ToTable("NotebookItems");
        }
    }
}