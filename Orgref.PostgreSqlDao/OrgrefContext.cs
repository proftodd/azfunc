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
            optionsBuilder
                .UseNpgsql($"Host={host};Database={database};Username={username};Password={password}")
                .UseLazyLoadingProxies();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Num9000>(entity => {
                entity.ToTable("nums");
                entity.HasKey(n => n.Num);
                entity.Property(n => n.Num).HasColumnName("num_9000");
                entity.Property(n => n.EntityId).HasColumnName("entity_id");
                entity.HasOne<Entity>(n => n.Entity).WithOne(e => e.Num).HasForeignKey<Num9000>(n => n.EntityId);
            });
            modelBuilder.Entity<Substance>(entity => {
                entity.ToTable("substances");
                entity.HasKey(e => e.EntityId);
                entity.Property(e => e.EntityId).HasColumnName("entity_id").IsRequired();
                entity.Property(e => e.Inchi).HasColumnName("inchi").IsRequired();
                entity.Property(e => e.InchiKey).HasColumnName("inchi_key").IsRequired();
                entity.HasOne<Entity>(s => s.Entity).WithOne(e => e.Sub).HasForeignKey<Substance>(s => s.EntityId);
            });
            modelBuilder.Entity<Descriptor>(entity => {
                entity.ToTable("descriptors");
                entity.HasKey(e => e.DescriptorId);
                entity.Property(e => e.DescriptorId).HasColumnName("descriptor_id").IsRequired();
                entity.Property(e => e.EntityId).HasColumnName("entity_id").IsRequired();
                entity.Property(e => e.Desc).HasColumnName("descriptor");
                entity.HasOne<Entity>(d => d.Entity).WithMany(e => e.Descriptors).HasForeignKey(d => d.EntityId);
            });
            modelBuilder.Entity<Entity>(entity => {
                entity.ToTable("entities");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("entity_id");
            });
        }

        public DbSet<Entity> Entities { get; set; }

        public DbSet<Substance> Substances { get; set; }

        public DbSet<Descriptor> Descriptors { get; set; }

        public DbSet<Num9000> Num9000s { get; set; }
    }
}