using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpenseApproval.Domain.Enums;

namespace ExpenseApproval.Application.DTOs
{
    public sealed class ExpenseRequestQuery
    {
        public RequestStatus? Status { get; set; }
        public string? Category { get; set; }
        public DateOnly? DateFrom { get; set; }
        public DateOnly? DateTo { get; set; }
    }
}
