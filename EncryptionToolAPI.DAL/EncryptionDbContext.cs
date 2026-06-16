using EncryptionToolAPI.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace EncryptionToolAPI.DAL
{
    /// <summary>
    /// The Entity Framework Core database context for the Encryption Tool API.
    /// </summary>
    public class EncryptionDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptionDbContext"/> class.
        /// </summary>
        public EncryptionDbContext(DbContextOptions<EncryptionDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the DbSet for client applications.
        /// </summary>
        public DbSet<ClientApplication> ClientApplications { get; set; }

        /// <summary>
        /// Gets or sets the DbSet for audit logs.
        /// </summary>
        public DbSet<AuditLog> AuditLogs { get; set; }

        /// <inheritdoc/>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Fluent API Configuration for ClientApplication
            modelBuilder.Entity<ClientApplication>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ApiKeyHash).IsRequired();
                entity.Property(e => e.EncryptedDataKey).IsRequired();
            });

            // Fluent API Configuration for AuditLog
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Operation).IsRequired().HasMaxLength(50);

                entity.HasOne(a => a.ClientApplication)
                      .WithMany()
                      .HasForeignKey(a => a.ClientApplicationId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
