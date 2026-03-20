
using ExpenseApproval.Application.Common.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ExpenseApproval.Api.Middleware
{
    public sealed class GlobalExceptionHandlerMiddleware : IMiddleware
    {
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public GlobalExceptionHandlerMiddleware(ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception. TraceId: {TraceId}", context.TraceIdentifier);

                var problem = ex switch
                {
                    ValidationException vex => BuildValidationProblem(context, vex),
                    NotFoundException nf => BuildProblem(context, nf.Message, HttpStatusCode.NotFound),
                    BusinessRuleException br => BuildProblem(context, br.Message, HttpStatusCode.Conflict),
                    _ => BuildProblem(context, "Error interno del servidor.", HttpStatusCode.InternalServerError)
                };

                context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/problem+json";
                await context.Response.WriteAsJsonAsync(problem);
            }
        }

        private static ProblemDetails BuildProblem(HttpContext ctx, string message, HttpStatusCode code) =>
            new()
            {
                Title = "Error",
                Detail = message,
                Status = (int)code,
                Instance = ctx.Request.Path
            };

        private static ValidationProblemDetails BuildValidationProblem(HttpContext ctx, ValidationException ex)
        {
            var errors = ex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            return new ValidationProblemDetails(errors)
            {
                Title = "Validación fallida",
                Status = StatusCodes.Status400BadRequest,
                Instance = ctx.Request.Path
            };
        }
    }
}
