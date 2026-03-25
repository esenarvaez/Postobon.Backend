using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using ExpenseApproval.Application.Common.Exceptions;
using ExpenseApproval.Application.DTOs;
using ExpenseApproval.Application.Interfaces;
using ExpenseApproval.Application.Services;
using ExpenseApproval.Domain.Entities;
using ExpenseApproval.Domain.Enums;
using ExpenseApproval.Tests.TestHelpers;
using FluentValidation;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ExpenseApproval.Tests.Application
{
    public class ExpenseRequestServiceTests
    {
        private readonly Mock<IExpenseRequestRepository> _repo = new();
        private readonly IMapper _mapper = MapperFactory.Create();

        
        private readonly IValidator<ExpenseRequestCreateDto> _createValidator = new ExpenseApproval.Application.Validators.ExpenseRequestCreateValidator();
        private readonly IValidator<ExpenseRequestUpdateDto> _updateValidator = new ExpenseApproval.Application.Validators.ExpenseRequestUpdateValidator();
        private readonly IValidator<DecisionDto> _decisionValidator = new ExpenseApproval.Application.Validators.DecisionValidator();
        private readonly IValidator<RejectDto> _rejectValidator = new ExpenseApproval.Application.Validators.RejectValidator();

        private ExpenseRequestService CreateSut()
            => new ExpenseRequestService(
                _repo.Object,
                _mapper,
                NullLogger<ExpenseRequestService>.Instance,
                _createValidator,
                _updateValidator,
                _decisionValidator,
                _rejectValidator);

        [Fact]
        public async Task CreateAsync_WhenValid_CreatesPendingAndCallsRepoAdd()
        {
            // Arrange
            var sut = CreateSut();
            var dto = new ExpenseRequestCreateDto
            {
                Category = "Viajes",
                Description = "Taxi aeropuerto",
                Value = 100m,
                ExpenseDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                RequestedBy = "user@corp.com"
            };

            _repo.Setup(r => r.AddAsync(It.IsAny<ExpenseRequest>(), It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

            // Act
            var result = await sut.CreateAsync(dto, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal(RequestStatus.Pending, result.Status);
            Assert.Equal(dto.Category, result.Category);

            _repo.Verify(r => r.AddAsync(It.Is<ExpenseRequest>(e =>
                e.Status == RequestStatus.Pending &&
                e.Id != Guid.Empty &&
                e.CreatedAtUtc != default
            ), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WhenInvalid_ThrowsValidationException()
        {
            // Arrange
            var sut = CreateSut();
            var dto = new ExpenseRequestCreateDto
            {
                Category = "",              
                Description = "",           
                Value = 0,                  
                ExpenseDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(1)), 
                RequestedBy = ""            
            };

            // Act + Assert
            await Assert.ThrowsAsync<ValidationException>(() => sut.CreateAsync(dto, CancellationToken.None));

            
            _repo.Verify(r => r.AddAsync(It.IsAny<ExpenseRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ApproveAsync_WhenNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var sut = CreateSut();
            _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((ExpenseRequest?)null);

            // Act + Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                sut.ApproveAsync(Guid.NewGuid(), new DecisionDto { DecisionBy = "lnarvaez" }, CancellationToken.None));
        }

        [Fact]
        public async Task ApproveAsync_WhenNotPending_ThrowsBusinessRuleException()
        {
            // Arrange
            var sut = CreateSut();
            var entity = new ExpenseRequest
            {
                Id = Guid.NewGuid(),
                Category = "Viajes",
                Description = "Hotel",
                Value = 500,
                ExpenseDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                RequestedBy = "user",
                Status = RequestStatus.Approved 
            };

            _repo.Setup(r => r.GetByIdAsync(entity.Id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(entity);

            // Act + Assert
            await Assert.ThrowsAsync<BusinessRuleException>(() =>
                sut.ApproveAsync(entity.Id, new DecisionDto { DecisionBy = "lnarvaez" }, CancellationToken.None));

            _repo.Verify(r => r.UpdateAsync(It.IsAny<ExpenseRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ApproveAsync_WhenPending_UpdatesStatusAndDecisionFields()
        {
            // Arrange
            var sut = CreateSut();
            var entity = new ExpenseRequest
            {
                Id = Guid.NewGuid(),
                Category = "Viajes",
                Description = "Hotel",
                Value = 500,
                ExpenseDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                RequestedBy = "user",
                Status = RequestStatus.Pending,
                RejectionReason = "old reason" 
            };

            _repo.Setup(r => r.GetByIdAsync(entity.Id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(entity);

            _repo.Setup(r => r.UpdateAsync(entity, It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

            // Act
            var result = await sut.ApproveAsync(entity.Id, new DecisionDto { DecisionBy = "lnarvaez" }, CancellationToken.None);

            // Assert
            Assert.Equal(RequestStatus.Approved, result.Status);
            Assert.Equal("lnarvaez", result.DecisionBy);
            Assert.NotNull(result.DecisionAtUtc);
            Assert.Null(result.RejectionReason);

            _repo.Verify(r => r.UpdateAsync(It.Is<ExpenseRequest>(e =>
                e.Status == RequestStatus.Approved &&
                e.DecisionBy == "lnarvaez" &&
                e.DecisionAtUtc != null &&
                e.RejectionReason == null
            ), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RejectAsync_WhenPending_SetsRejectedAndReason()
        {
            var sut = CreateSut();
            var entity = new ExpenseRequest
            {
                Id = Guid.NewGuid(),
                Category = "Compras",
                Description = "Teclado",
                Value = 50,
                ExpenseDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                RequestedBy = "user",
                Status = RequestStatus.Pending
            };

            _repo.Setup(r => r.GetByIdAsync(entity.Id, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(entity);

            // Act
            var result = await sut.RejectAsync(entity.Id, new RejectDto { DecisionBy = "lnarvaez", Reason = "Fuera de política" }, CancellationToken.None);

            // Assert
            Assert.Equal(RequestStatus.Rejected, result.Status);
            Assert.Equal("Fuera de política", result.RejectionReason);
            Assert.NotNull(result.DecisionAtUtc);
            _repo.Verify(r => r.UpdateAsync(It.IsAny<ExpenseRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
