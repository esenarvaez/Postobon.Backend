using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpenseApproval.Application.DTOs;
using ExpenseApproval.Application.Interfaces;
using ExpenseApproval.Domain.Entities;
using ExpenseApproval.Domain.Enums;
using ExpenseApproval.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseApproval.Infrastructure.Repositories
{
    public sealed class ExpenseRequestRepository : IExpenseRequestRepository
    {
        private readonly AppDbContext _db;
        public ExpenseRequestRepository(AppDbContext db) => _db = db;

        public async Task AddAsync(ExpenseRequest entity, CancellationToken ct)
        {
            _db.ExpenseRequests.Add(entity);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<IReadOnlyList<ExpenseRequest>> GetAsync(ExpenseRequestQuery query, CancellationToken ct)
        {
            IQueryable<ExpenseRequest> q = _db.ExpenseRequests.AsNoTracking();

            if (query.Status.HasValue)
                q = q.Where(x => x.Status == query.Status.Value);

            if (!string.IsNullOrWhiteSpace(query.Category))
                q = q.Where(x => x.Category == query.Category);

            if (query.DateFrom.HasValue)
                q = q.Where(x => x.ExpenseDate >= query.DateFrom.Value);

            if (query.DateTo.HasValue)
                q = q.Where(x => x.ExpenseDate <= query.DateTo.Value);

            return await q
                .OrderByDescending(x => x.CreatedAtUtc)
                .ToListAsync(ct);
        }

        public Task<ExpenseRequest?> GetByIdAsync(Guid id, CancellationToken ct) =>
        _db.ExpenseRequests.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

        public async Task<ExpenseRequestMetricsDto> GetMetricsAsync(CancellationToken ct)
        {
            var baseQuery = _db.ExpenseRequests.AsNoTracking();

            var total = await baseQuery.CountAsync(ct);

            var pending = await baseQuery.CountAsync(x => x.Status == RequestStatus.Pending, ct);
            var approved = await baseQuery.CountAsync(x => x.Status == RequestStatus.Approved, ct);
            var rejected = await baseQuery.CountAsync(x => x.Status == RequestStatus.Rejected, ct);

            var totalValue = await baseQuery.SumAsync(x => (decimal?)x.Value, ct) ?? 0m;
            var approvedValue = await baseQuery.Where(x => x.Status == RequestStatus.Approved).SumAsync(x => (decimal?)x.Value, ct) ?? 0m;
            var pendingValue = await baseQuery.Where(x => x.Status == RequestStatus.Pending).SumAsync(x => (decimal?)x.Value, ct) ?? 0m;
            var rejectedValue = await baseQuery.Where(x => x.Status == RequestStatus.Rejected).SumAsync(x => (decimal?)x.Value, ct) ?? 0m;

            return new ExpenseRequestMetricsDto
            {
                Total = total,
                Pending = pending,
                Approved = approved,
                Rejected = rejected,
                TotalValue = totalValue,
                ApprovedValue = approvedValue,
                PendingValue = pendingValue,
                RejectedValue = rejectedValue
            };
        }

        public async Task UpdateAsync(ExpenseRequest entity, CancellationToken ct)
        {
            _db.ExpenseRequests.Update(entity);
            await _db.SaveChangesAsync(ct);
        }
    }
}
