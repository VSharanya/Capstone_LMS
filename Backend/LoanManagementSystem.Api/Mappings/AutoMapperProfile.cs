using AutoMapper;
using LoanManagementSystem.Api.DTOs.Auth;
using LoanManagementSystem.Api.DTOs.Users;
using LoanManagementSystem.Api.DTOs.Loans;
using LoanManagementSystem.Api.DTOs.EMI;
using LoanManagementSystem.Api.DTOs.Reports;
using LoanManagementSystem.Api.Models;

namespace LoanManagementSystem.Api.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
           
            // USER MAPPINGS
           
            CreateMap<User, UserResponseDto>();

            
            // LOAN APPLICATION MAPPINGS
            
            CreateMap<LoanApplication, LoanResponseDto>()
                .ForMember(dest => dest.CustomerName,
                    opt => opt.MapFrom(src => src.Customer.FullName))
                .ForMember(dest => dest.LoanType,
                    opt => opt.MapFrom(src => src.LoanType.LoanTypeName))
                .ForMember(dest => dest.Remarks,
                    opt => opt.MapFrom(src => src.Remarks))
                .ForMember(dest => dest.AnnualIncome,
                    opt => opt.MapFrom(src => src.Customer.AnnualIncome))
                .ForMember(dest => dest.TotalPaid,
                    opt => opt.MapFrom(src => src.EMIs != null ? src.EMIs.Where(e => e.IsPaid).Sum(e => e.EMIAmount) : 0))
                .ForMember(dest => dest.OutstandingAmount,
                    opt => opt.MapFrom(src => src.EMIs != null ? src.EMIs.Where(e => !e.IsPaid).Sum(e => e.EMIAmount) : 0))
                .ForMember(dest => dest.HasDocuments,
                    opt => opt.MapFrom(src => src.LoanDocuments != null && src.LoanDocuments.Any()));

            CreateMap<LoanApplyDto, LoanApplication>()
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(_ => "Applied"))
                .ForMember(dest => dest.AppliedDate,
                    opt => opt.MapFrom(_ => DateOnly.FromDateTime(DateTime.UtcNow)));

            
            // EMI MAPPINGS
            
            CreateMap<EMI, EmiResponseDto>();

            
            // REPORT MAPPINGS
           
            CreateMap<LoanApplication, LoansByStatusDto>()
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.Count,
                    opt => opt.Ignore());
        }
    }
}
