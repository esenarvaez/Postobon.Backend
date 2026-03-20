using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpenseApproval.Application.DTOs;
using FluentValidation;

namespace ExpenseApproval.Application.Validators
{
    public sealed class ExpenseRequestCreateValidator
        : AbstractValidator<ExpenseRequestCreateDto>
    {
        public ExpenseRequestCreateValidator() {
            RuleFor(x => x.Category).NotEmpty().WithMessage("La categoria es obligatoria")
                .MaximumLength(100);
            RuleFor(x => x.Description).NotEmpty().WithMessage("La descripción es obligatoria")
                .MaximumLength(100);
            RuleFor(x => x.Value).GreaterThan(0).WithMessage("El valor debe ser mayor a 0");
            RuleFor(x => x.RequestedBy).NotEmpty().WithMessage("El usuario solicitante es obligatorio")
                .MaximumLength(200);
            RuleFor(x => x.ExpenseDate)
            .Must(d => d <= DateOnly.FromDateTime(DateTime.UtcNow.Date))
            .WithMessage("La fecha del gasto no puede ser futura");
        }
    }
}
