using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using ExpenseApproval.Application.DTOs;
using ExpenseApproval.Domain.Entities;

namespace ExpenseApproval.Application.Mapping
{
    public sealed class ExpenseRequestProfile : Profile
    {
        public ExpenseRequestProfile() {

            CreateMap<ExpenseRequestCreateDto, ExpenseRequest>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.Status, opt => opt.Ignore())
                .ForMember(d => d.CreatedAtUtc, opt => opt.Ignore())
                .ForMember(d => d.DecisionAtUtc, opt => opt.Ignore())
                .ForMember(d => d.DecisionBy, opt => opt.Ignore())
                .ForMember(d => d.RejectionReason, opt => opt.Ignore());

            CreateMap<ExpenseRequestUpdateDto, ExpenseRequest>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.RequestedBy, opt => opt.Ignore())
                .ForMember(d => d.Status, opt => opt.Ignore())
                .ForMember(d => d.CreatedAtUtc, opt => opt.Ignore())
                .ForMember(d => d.DecisionAtUtc, opt => opt.Ignore())
                .ForMember(d => d.DecisionBy, opt => opt.Ignore())
                .ForMember(d => d.RejectionReason, opt => opt.Ignore());

            CreateMap<ExpenseRequest, ExpenseRequestReadDto>();
        }
    }
}
