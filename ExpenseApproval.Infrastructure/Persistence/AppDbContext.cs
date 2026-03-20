using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpenseApproval.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpenseApproval.Infrastructure.Persistence
{
    public sealed class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<ExpenseRequest> ExpenseRequests => Set<ExpenseRequest>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ExpenseRequest>(b =>
            {
                b.ToTable("expense_requests");
                b.HasKey(x => x.Id);

                b.Property(x => x.Category).HasMaxLength(100).IsRequired();
                b.Property(x => x.Description).HasMaxLength(1000).IsRequired();
                b.Property(x => x.Value).HasColumnType("numeric(18,2)").IsRequired();
                b.Property(x => x.ExpenseDate).IsRequired();
                b.Property(x => x.RequestedBy).HasMaxLength(200).IsRequired();

                b.Property(x => x.Status).IsRequired();
                b.Property(x => x.CreatedAtUtc).IsRequired();

                b.Property(x => x.DecisionBy).HasMaxLength(200);
                b.Property(x => x.RejectionReason).HasMaxLength(500);

                b.HasIndex(x => x.Status);
                b.HasIndex(x => x.Category);
                b.HasIndex(x => x.ExpenseDate);
                b.HasIndex(x => x.CreatedAtUtc);
            });
        }
    }
}
