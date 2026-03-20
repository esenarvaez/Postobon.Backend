using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseApproval.Application.DTOs
{
    public sealed class ExpenseRequestCreateDto
    {
        public string Category { get; set; } = default!;
        public string Description { get; set; } = default!;
        public decimal Value { get; set; }
        public DateOnly ExpenseDate { get; set; }
        public string RequestedBy { get; set; } = default!;
    }
}
