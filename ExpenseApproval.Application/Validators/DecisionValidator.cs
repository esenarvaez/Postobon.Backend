using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpenseApproval.Application.DTOs;
using FluentValidation;

namespace ExpenseApproval.Application.Validators
{
    public sealed class DecisionValidator
        : AbstractValidator<DecisionDto>
    {
        public DecisionValidator() {
            RuleFor(x => x.DecisionBy)
            .NotEmpty().WithMessage("DecisionBy es obligatorio.")
            .MaximumLength(200);
        }
    }
}
