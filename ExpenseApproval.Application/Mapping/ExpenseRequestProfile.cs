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

            CreateMap<ExpenseRequestCreateDto, ExpenseRequest>();
            CreateMap<ExpenseRequestUpdateDto, ExpenseRequest>();

            CreateMap<ExpenseRequest, ExpenseRequestReadDto>();
        }
    }
}
