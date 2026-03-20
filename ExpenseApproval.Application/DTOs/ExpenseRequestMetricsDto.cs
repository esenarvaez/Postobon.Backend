using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseApproval.Application.DTOs
{
    public sealed class ExpenseRequestMetricsDto
    {
        public int Total { get; set; }
        public int Pending { get; set; }
        public int Approved { get; set; }
        public int Rejected { get; set; }

        public decimal TotalValue { get; set; }
        public decimal ApprovedValue { get; set; }
        public decimal PendingValue { get; set; }
        public decimal RejectedValue { get; set; }
    }
}
