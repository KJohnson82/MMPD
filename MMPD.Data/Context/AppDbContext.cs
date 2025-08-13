#nullable disable

using Microsoft.EntityFrameworkCore;
using MMPD.Data.Models;

namespace MMPD.Data.Context
{
    public partial class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

        public virtual DbSet<Department> Departments { get; set; }
        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<Location> Locations { get; set; }
        public virtual DbSet<Loctype> Loctypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Department>(entity =>
            {
                entity.Property(e => e.Active).HasDefaultValue(true);
                entity.Property(e => e.RecordAdd).HasColumnType("DATETIME DEFAULT CURRENT_TIMESTAMP");
                entity.HasOne(d => d.DeptLocation).WithMany(p => p.Departments).HasForeignKey(d => d.Location);
            });

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.Property(e => e.Active).HasDefaultValue(true);
                entity.Property(e => e.RecordAdd).HasColumnType("DATETIME DEFAULT CURRENT_TIMESTAMP");
                entity.HasOne(d => d.EmpDepartment).WithMany(p => p.Employees).HasForeignKey(d => d.Department);
                entity.HasOne(d => d.EmpLocation).WithMany(p => p.Employees).HasForeignKey(d => d.Location);
            });

            modelBuilder.Entity<Location>(entity =>
            {
                entity.Property(e => e.Active).HasDefaultValue(true);
                entity.Property(e => e.RecordAdd).HasColumnType("DATETIME DEFAULT CURRENT_TIMESTAMP");
                entity.HasOne(d => d.LocationType).WithMany(p => p.Locations).HasForeignKey(d => d.Loctype);
            });
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}


//If Using Power Tools Again

//If you re-scaffold later:

//    Point EF Core Power Tools to your new MMPD.Data project.

//    Choose DbContext output folder as Context

//    Choose Model output folder as Models