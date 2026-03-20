using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpenseApproval.Application.DTOs;

namespace ExpenseApproval.Application.Interfaces
{
    public interface IExpenseRequestService
    {
        Task<IReadOnlyList<ExpenseRequestReadDto>> GetAsync(ExpenseRequestQuery query, CancellationToken ct);
        Task<ExpenseRequestReadDto> GetByIdAsync(Guid id, CancellationToken ct);
        Task<ExpenseRequestReadDto> CreateAsync(ExpenseRequestCreateDto dto, CancellationToken ct);
        Task<ExpenseRequestReadDto> UpdateAsync(Guid id, ExpenseRequestUpdateDto dto, CancellationToken ct);
        Task<ExpenseRequestReadDto> ApproveAsync(Guid id, DecisionDto dto, CancellationToken ct);
        Task<ExpenseRequestReadDto> RejectAsync(Guid id, RejectDto dto, CancellationToken ct);
        Task<ExpenseRequestMetricsDto> GetMetricsAsync(CancellationToken ct);
    }
}
