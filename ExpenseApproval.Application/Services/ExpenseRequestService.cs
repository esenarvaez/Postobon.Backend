using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using ExpenseApproval.Application.Common.Exceptions;
using ExpenseApproval.Application.DTOs;
using ExpenseApproval.Application.Interfaces;
using ExpenseApproval.Domain.Entities;
using ExpenseApproval.Domain.Enums;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace ExpenseApproval.Application.Services
{
    public sealed class ExpenseRequestService : IExpenseRequestService
    {

        private readonly IExpenseRequestRepository _repo;
        private readonly IMapper _mapper;
        private readonly ILogger<ExpenseRequestService> _logger;

        private readonly IValidator<ExpenseRequestCreateDto> _createValidator;
        private readonly IValidator<ExpenseRequestUpdateDto> _updateValidator;
        private readonly IValidator<DecisionDto> _decisionValidator;
        private readonly IValidator<RejectDto> _rejectValidator;

        public ExpenseRequestService(
        IExpenseRequestRepository repo,
        IMapper mapper,
        ILogger<ExpenseRequestService> logger,
        IValidator<ExpenseRequestCreateDto> createValidator,
        IValidator<ExpenseRequestUpdateDto> updateValidator,
        IValidator<DecisionDto> decisionValidator,
        IValidator<RejectDto> rejectValidator)
        {
            _repo = repo;
            _mapper = mapper;
            _logger = logger;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _decisionValidator = decisionValidator;
            _rejectValidator = rejectValidator;
        }
        public async Task<ExpenseRequestReadDto> ApproveAsync(Guid id, DecisionDto dto, CancellationToken ct)
        {
            await ValidateOrThrowAsync(_decisionValidator, dto, ct);

            var entity = await _repo.GetByIdAsync(id, ct);
            if (entity is null) throw new NotFoundException($"Solicitud {id} no existe.");

            if (entity.Status != RequestStatus.Pending)
                throw new BusinessRuleException("Solo se puede aprobar una solicitud en estado Pendiente.");

            entity.Status = RequestStatus.Approved;
            entity.DecisionAtUtc = DateTime.UtcNow;
            entity.DecisionBy = dto.DecisionBy;

            
            entity.RejectionReason = null;

            await _repo.UpdateAsync(entity, ct);

            _logger.LogInformation("Approved expense request {Id} by {DecisionBy}", entity.Id, dto.DecisionBy);
            return _mapper.Map<ExpenseRequestReadDto>(entity);
        }

        public async Task<ExpenseRequestReadDto> CreateAsync(ExpenseRequestCreateDto dto, CancellationToken ct)
        {
            await ValidateOrThrowAsync(_createValidator, dto, ct);

            var entity = _mapper.Map<ExpenseRequest>(dto);
            entity.Id = Guid.NewGuid();
            entity.Status = RequestStatus.Pending;
            entity.CreatedAtUtc = DateTime.UtcNow;

            await _repo.AddAsync(entity, ct);

            _logger.LogInformation("Created expense request {Id} by {RequestedBy}", entity.Id, entity.RequestedBy);
            return _mapper.Map<ExpenseRequestReadDto>(entity);
        }

        public async Task<IReadOnlyList<ExpenseRequestReadDto>> GetAsync(ExpenseRequestQuery query, CancellationToken ct)
        {
            var entities = await _repo.GetAsync(query, ct);
            return entities.Select(_mapper.Map<ExpenseRequestReadDto>).ToList();
        }

        public async Task<ExpenseRequestReadDto> GetByIdAsync(Guid id, CancellationToken ct)
        {
            var entity = await _repo.GetByIdAsync(id, ct);
            if (entity is null) throw new NotFoundException($"Solicitud {id} no existe.");
            return _mapper.Map<ExpenseRequestReadDto>(entity);
        }

        public Task<ExpenseRequestMetricsDto> GetMetricsAsync(CancellationToken ct) =>
        _repo.GetMetricsAsync(ct);

        public async Task<ExpenseRequestReadDto> RejectAsync(Guid id, RejectDto dto, CancellationToken ct)
        {
            await ValidateOrThrowAsync(_rejectValidator, dto, ct);

            var entity = await _repo.GetByIdAsync(id, ct);
            if (entity is null) throw new NotFoundException($"Solicitud {id} no existe.");

            if (entity.Status != RequestStatus.Pending)
                throw new BusinessRuleException("Solo se puede rechazar una solicitud en estado Pendiente.");

            entity.Status = RequestStatus.Rejected;
            entity.DecisionAtUtc = DateTime.UtcNow;
            entity.DecisionBy = dto.DecisionBy;
            entity.RejectionReason = dto.Reason;

            await _repo.UpdateAsync(entity, ct);

            _logger.LogInformation("Rejected expense request {Id} by {DecisionBy}. Reason: {Reason}", entity.Id, dto.DecisionBy, dto.Reason);
            return _mapper.Map<ExpenseRequestReadDto>(entity);
        }

        public async Task<ExpenseRequestReadDto> UpdateAsync(Guid id, ExpenseRequestUpdateDto dto, CancellationToken ct)
        {
            await ValidateOrThrowAsync(_updateValidator, dto, ct);

            var entity = await _repo.GetByIdAsync(id, ct);
            if (entity is null) throw new NotFoundException($"Solicitud {id} no existe.");

            if (entity.Status != RequestStatus.Pending)
                throw new BusinessRuleException("Solo se puede editar una solicitud en estado Pendiente.");

            
            entity.Category = dto.Category;
            entity.Description = dto.Description;
            entity.Value = dto.Value;
            entity.ExpenseDate = dto.ExpenseDate;

            await _repo.UpdateAsync(entity, ct);

            _logger.LogInformation("Updated expense request {Id}", entity.Id);
            return _mapper.Map<ExpenseRequestReadDto>(entity);
        }

        private static async Task ValidateOrThrowAsync<T>(IValidator<T> validator, T model, CancellationToken ct)
        {
            ValidationResult result = await validator.ValidateAsync(model, ct);
            if (!result.IsValid)
                throw new ValidationException(result.Errors);
        }
    }
}
