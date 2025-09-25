#nullable disable
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MMPD.Data.Data;
using MMPD.Data.Models;

namespace MMPD.Data.Context
{
    /// <summary>
    /// Represents the database session for the application.
    /// This class is the main entry point for querying and saving data using Entity Framework Core.
    /// It defines the entity sets (DbSet) that correspond to tables in the database.
    /// </summary>
    //public partial class AppDbContext : DbContext
    public partial class AppDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the AppDbContext class.
        /// This constructor allows for the database connection and other options to be configured
        /// and passed in via dependency injection.
        /// </summary>
        /// <param name="options">The options for this context.</param>
        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

        #region DbSet Properties

        /// <summary>
        /// Represents the 'Departments' table in the database.
        /// Used to query and manage Department entities.
        /// </summary>
        public virtual DbSet<Department> Departments { get; set; }

        /// <summary>
        /// Represents the 'Employees' table in the database.
        /// Used to query and manage Employee entities.
        /// </summary>
        public virtual DbSet<Employee> Employees { get; set; }

        /// <summary>
        /// Represents the 'Locations' table in the database.
        /// Used to query and manage Location entities.
        /// </summary>
        public virtual DbSet<Location> Locations { get; set; }

        /// <summary>
        /// Represents the 'Loctypes' (Location Types) table in the database.
        /// Used to query and manage location type classifications.
        /// </summary>
        public virtual DbSet<Loctype> Loctypes { get; set; }

        #endregion

        /// <summary>
        /// Represents the 'UserAccount' table in the database.
        /// Used to query and manage user authentication.
        /// </summary>
        public virtual DbSet<UserAccount> UserAccounts { get; set; }

        /// <summary>
        /// Represents the 'UserRoles' table in the database.
        /// Used to query and manage user roles.
        /// </summary>
        public virtual DbSet<UserRole> UserRoles { get; set; }

        /// <summary>
        /// Configures the database model using the Fluent API.
        /// This method is called by Entity Framework Core when the model for the context is being created.
        /// It is used to define table relationships, constraints, default values, and other data annotations.
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ===== DEPARTMENT ENTITY CONFIGURATION =====
            modelBuilder.Entity<Department>(entity =>
            {
                // Set the default value for the 'Active' property to true for new departments.
                entity.Property(e => e.Active).HasDefaultValue(true);

                // Configure the 'RecordAdd' column to automatically set the timestamp on creation.
                entity.Property(e => e.RecordAdd).HasColumnType("DATETIME DEFAULT CURRENT_TIMESTAMP");

                // Define the relationship: a Department belongs to one Location.
                // The foreign key is the 'Location' property in the Department entity.
                entity.HasOne(d => d.DeptLocation).WithMany(p => p.Departments).HasForeignKey(d => d.Location);
            });

            // ===== EMPLOYEE ENTITY CONFIGURATION =====
            modelBuilder.Entity<Employee>(entity =>
            {
                // Set the default value for the 'Active' property to true for new employees.
                entity.Property(e => e.Active).HasDefaultValue(true);

                // Configure the 'RecordAdd' column to automatically set the timestamp on creation.
                entity.Property(e => e.RecordAdd).HasColumnType("DATETIME DEFAULT CURRENT_TIMESTAMP");

                // Define the relationship: an Employee belongs to one Department.
                // The foreign key is the 'Department' property in the Employee entity.
                entity.HasOne(d => d.EmpDepartment).WithMany(p => p.Employees).HasForeignKey(d => d.Department);

                // Define the relationship: an Employee is assigned to one Location.
                // The foreign key is the 'Location' property in the Employee entity.
                entity.HasOne(d => d.EmpLocation).WithMany(p => p.Employees).HasForeignKey(d => d.Location);
            });

            // ===== LOCATION ENTITY CONFIGURATION =====
            modelBuilder.Entity<Location>(entity =>
            {
                // Set the default value for the 'Active' property to true for new locations.
                entity.Property(e => e.Active).HasDefaultValue(true);

                // Configure the 'RecordAdd' column to automatically set the timestamp on creation.
                entity.Property(e => e.RecordAdd).HasColumnType("DATETIME DEFAULT CURRENT_TIMESTAMP");

                // Define the relationship: a Location has one LocationType.
                // The foreign key is the 'Loctype' property in the Location entity.
                entity.HasOne(d => d.LocationType).WithMany(p => p.Locations).HasForeignKey(d => d.Loctype);
            });

            // ===== USER AUTHENTICATION CONFIGURATION =====

            // UserRoles configuration
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.ToTable("UserRoles");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(30);
                entity.HasIndex(e => e.Role).IsUnique();
            });

            // UserAccount configuration
            modelBuilder.Entity<UserAccount>(entity =>
            {
                entity.ToTable("UserAccount");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(30);
                entity.Property(e => e.RoleId).HasDefaultValue(3);
                entity.Property(e => e.Active).HasDefaultValue(false);

                // Foreign key relationship
                entity.HasOne(u => u.UserRole)
                    .WithMany(r => r.UserAccounts)
                    .HasForeignKey(u => u.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Call the partial method to allow for further model configuration in another file.
            OnModelCreatingPartial(modelBuilder);
        }

        /// <summary>
        /// Provides an extensibility point for model configuration.
        /// This partial method can be implemented in another partial class file to add
        /// custom model configurations without modifying this auto-generated file.
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}

/*
=================================================
NOTES FOR USING EF CORE POWER TOOLS
=================================================

If you need to re-scaffold the database context and models later:

1. Point EF Core Power Tools to your MMPD.Data project.
2. Choose the DbContext output folder as 'Context'.
3. Choose the Model output folder as 'Models'.

This will ensure the generated files are placed in the correct locations within the project structure.
*/
