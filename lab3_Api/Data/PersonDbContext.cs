using lab3_Api.Models;
using Microsoft.EntityFrameworkCore;

namespace lab3_Api.Data
{
    public class PersonDbContext(DbContextOptions<PersonDbContext> opts) : DbContext(opts)
    {

        public DbSet<Person> Persons { get; set; }
        public DbSet<Interest> Interests { get; set; }
        public DbSet<Link> Links { get; set; }
        public DbSet<InterestRelation> InterestRelation { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<InterestRelation>()
             .HasKey(ir => new { ir.PersonId, ir.InterestId });

            modelBuilder.Entity<Interest>()
                .HasKey(i => i.Id);

            modelBuilder.Entity<Link>()
                .HasKey(l => l.Id);

            modelBuilder.Entity<Person>()
                .ToTable("Persons")
                .HasKey(p => p.Id);


            base.OnModelCreating(modelBuilder);
        }

    }
}
