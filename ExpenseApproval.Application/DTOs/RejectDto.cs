using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseApproval.Application.DTOs
{
    public sealed class RejectDto
    {
        public string DecisionBy { get; set; } = default!;
        public string Reason { get; set; } = default!;
    }
}
