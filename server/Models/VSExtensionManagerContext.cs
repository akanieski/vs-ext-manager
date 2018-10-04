using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace VSExtensionManager.Models
{
    public class VSExtensionManagerContext : DbContext
    {
        public VSExtensionManagerContext(DbContextOptions<VSExtensionManagerContext> options)
            : base(options)
        { 
            
        }
        public VSExtensionManagerContext() {}

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLoggerFactory(new LoggerFactory(new[] { new ConsoleLoggerProvider((_, __) => true, true) }));
            optionsBuilder.UseSqlServer(Environment.GetEnvironmentVariable("DB"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
        }

        private void PreSave() {
            foreach (var entry in ChangeTracker.Entries()) {
                if (entry is IPreSaveHooked) {
                    (entry as IPreSaveHooked).OnSave();
                }
            }
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess) {
            PreSave();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }
        public override int SaveChanges() {
            PreSave();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken)) {
            PreSave();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess);
        }
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            PreSave();
            return base.SaveChangesAsync();
        }

        public DbSet<Gallery> Galleries { get; set; }
        public DbSet<Package> Packages { get; set; }
        public DbSet<Blob> Blobs { get; set; }
    }
    public interface IPreSaveHooked {
        void OnSave();
    }
}