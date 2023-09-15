using login.model;
using Microsoft.EntityFrameworkCore;
using RnkApi.Models;
using System;

namespace RnkApi.Context
{
    public class appDbContext : DbContext
    {
         
        public appDbContext(DbContextOptions<appDbContext> options) : base(options)
        {
            
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<employee> Employees { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>().ToTable("Customers");
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<employee>().ToTable("employees");
        }

        



    }
}
