using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpenseApproval.Domain.Enums;

namespace ExpenseApproval.Application.DTOs
{
    public sealed class ExpenseRequestReadDto
    {
        public Guid Id { get; set; }
        public string Category { get; set; } = default!;
        public string Description { get; set; } = default!;
        public decimal Value { get; set; }
        public DateOnly ExpenseDate { get; set; }
        public string RequestedBy { get; set; } = default!;
        public RequestStatus Status { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? DecisionAtUtc { get; set; }
        public string? DecisionBy { get; set; }
        public string? RejectionReason { get; set; }
    }
}
