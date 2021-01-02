using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using My.DAO;
using My.Models;

namespace Orgref.PostgreSqlDao
{
    public class OrgrefContext : DbContext
    {
        private readonly string host;
        private readonly string database;
        private readonly string username;
        private readonly string password;

        public OrgrefContext(IOptions<DAOOptions> options)
        {
            host = options.Value.Host;
            database = options.Value.Database;
            username = options.Value.Username;
            password = options.Value.Password;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
            optionsBuilder.UseNpgsql($"Host={host};Database={database};Username={username};Password={password}");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entity>(entity => {
                entity.ToTable("entities");
                entity.Property(e => e.Id).HasColumnName("entity_id");
                entity.HasOne<Substance>(e => e.Sub).WithOne(e => e.Entity);
                entity.HasOne<Num9000>(e => e.Num).WithOne(e => e.Entity);
                entity.HasMany<Descriptor>(e => e.Descriptors).WithOne(e => e.Entity);
            });
            modelBuilder.Entity<Substance>().ToTable("substances");
            modelBuilder.Entity<Num9000>(entity => {
                entity.ToTable("nums");
                entity.Property(e => e.Num).HasColumnName("num_9000");
            });
            modelBuilder.Entity<Descriptor>(entity => {
                entity.ToTable("descriptors");
                entity.Property(e => e.Id).HasColumnName("descriptor_id");
                entity.Property(e => e.Desc).HasColumnName("descriptor");
            });
        }

        public DbSet<Entity> Entities { get; set; }

        public DbSet<Substance> Substances { get; set; }

        public DbSet<Descriptor> Descriptors { get; set; }

        public DbSet<Num9000> Num9000s { get; set; }
    }
}