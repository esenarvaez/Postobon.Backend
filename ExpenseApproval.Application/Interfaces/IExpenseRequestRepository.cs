using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpenseApproval.Application.DTOs;
using ExpenseApproval.Domain.Entities;

namespace ExpenseApproval.Application.Interfaces
{
    public interface IExpenseRequestRepository
    {
        Task<ExpenseRequest?> GetByIdAsync(Guid id, CancellationToken ct);
        Task<IReadOnlyList<ExpenseRequest>> GetAsync(ExpenseRequestQuery query, CancellationToken ct);
        Task AddAsync(ExpenseRequest entity, CancellationToken ct);
        Task UpdateAsync(ExpenseRequest entity, CancellationToken ct);
        Task<ExpenseRequestMetricsDto> GetMetricsAsync(CancellationToken ct);
    }
}
