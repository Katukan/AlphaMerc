using Microsoft.EntityFrameworkCore;


namespace ConsoleProject.Models {

    public class DatabaseContext : DbContext {

        public DbSet<DoctorAccount> DoctorAccounts { get; set; }
        public DbSet<Milkman> Milkmen { get; set; }

        public DatabaseContext() {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            optionsBuilder.UseSqlite("Filename=database.db");
        }
    }
}