using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpenseApproval.Domain.Enums;

namespace ExpenseApproval.Domain.Entities
{
    public class ExpenseRequest
    {
        public Guid Id { get; set; }

        public string Category { get; set; } = default!;
        public string Description { get; set; } = default!;
        public decimal Value { get; set; }
        public DateOnly ExpenseDate { get; set; }

        public string RequestedBy { get; set; } = default!;

        public RequestStatus Status { get; set; } = RequestStatus.Pending;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public DateTime? DecisionAtUtc { get; set; }
        public string? DecisionBy { get; set; }
        public string? RejectionReason { get; set; }
    }
}
