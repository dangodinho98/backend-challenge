using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales;

public class SalesProfile : Profile
{
    public SalesProfile()
    {
        CreateMap<ExternalIdentityRequest, Application.Sales.Common.ExternalIdentityModel>();
        CreateMap<SaleItemRequest, CreateSaleItemCommand>();
        CreateMap<CreateSaleRequest, CreateSaleCommand>()
            .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.Customer.Id))
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.Name))
            .ForMember(dest => dest.BranchId, opt => opt.MapFrom(src => src.Branch.Id))
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch.Name))
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items.Select(i => new CreateSaleItemCommand
            {
                ProductId = i.Product.Id,
                ProductName = i.Product.Name,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            })));

        CreateMap<Application.Sales.Common.ExternalIdentityModel, ExternalIdentityResponse>();
        CreateMap<Application.Sales.Common.SaleItemModel, SaleItemResponse>();
        CreateMap<Application.Sales.Common.SaleModel, SaleResponse>();
    }
}
