using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpenseApproval.Application.DTOs;
using FluentValidation;

namespace ExpenseApproval.Application.Validators
{
    public sealed class RejectValidator
        : AbstractValidator<RejectDto>
    {
        public RejectValidator() {

            RuleFor(x => x.DecisionBy)
            .NotEmpty().WithMessage("DecisionBy es obligatorio.")
            .MaximumLength(200);

            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("La razón de rechazo es obligatoria.")
                .MaximumLength(500);
        }
    }
}
