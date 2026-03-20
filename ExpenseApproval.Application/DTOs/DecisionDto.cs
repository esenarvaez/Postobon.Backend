using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseApproval.Application.DTOs
{
    public sealed class DecisionDto
    {
        public string DecisionBy { get; set; } = default!;
    }
}
