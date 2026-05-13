using Microsoft.EntityFrameworkCore;
using SystemZglaszaniaUsterek.Models.Entities;

namespace SystemZglaszaniaUsterek.Models.Entities
{
    public class SystemZglaszaniaUsterekDbContext : DbContext
    {
        public SystemZglaszaniaUsterekDbContext(DbContextOptions<SystemZglaszaniaUsterekDbContext> options)
            : base(options)
        {
        }

        public DbSet<UserModel> Users { get; set; }
        public DbSet<TicketModel> Tickets { get; set; }
        public DbSet<CommentModel> Comments { get; set; }
        public DbSet<TicketHistoryModel> TicketHistories { get; set; }
        public DbSet<StatusModel> Statuses { get; set; }
        public DbSet<PriorityModel> Priorities { get; set; }
        public DbSet<CategoryModel> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TicketModel>()
                .HasOne(t => t.Reporter)
                .WithMany(u => u.ReportedTickets)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TicketModel>()
                .HasOne(t => t.Technician)
                .WithMany(u => u.AssignedTickets)
                .OnDelete(DeleteBehavior.Restrict);

            foreach (var foreignKey in modelBuilder.Model.GetEntityTypes()
                .SelectMany(e => e.GetForeignKeys()))
            {
                if (foreignKey.DeleteBehavior != DeleteBehavior.Restrict)
                {
                    foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
                }
            }
        }
    }
}
