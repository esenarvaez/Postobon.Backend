using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpenseApproval.Application.DTOs;
using ExpenseApproval.Application.Interfaces;
using ExpenseApproval.Application.Mapping;
using ExpenseApproval.Application.Services;
using ExpenseApproval.Application.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ExpenseApproval.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(ExpenseRequestProfile).Assembly);

            services.AddScoped<IExpenseRequestService, ExpenseRequestService>();

            services.AddScoped<IValidator<ExpenseRequestCreateDto>, ExpenseRequestCreateValidator>();
            services.AddScoped<IValidator<ExpenseRequestUpdateDto>, ExpenseRequestUpdateValidator>();
            services.AddScoped<IValidator<DecisionDto>, DecisionValidator>();
            services.AddScoped<IValidator<RejectDto>, RejectValidator>();

            return services;
        }
    }
}
