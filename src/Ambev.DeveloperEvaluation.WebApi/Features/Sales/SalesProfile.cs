using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.ListSales;
using global::Ambev.DeveloperEvaluation.Application.Sales.ModifySale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales;

public class SalesProfile : Profile
{
    public SalesProfile()
    {
        CreateMap<ExternalIdentityRequest, Application.Sales.Common.ExternalIdentityModel>();
        CreateMap<SaleItemRequest, CreateSaleItemCommand>();
        CreateMap<SaleItemRequest, UpdateSaleItemCommand>();
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

        CreateMap<UpdateSaleRequest, UpdateSaleCommand>()
            .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.Customer.Id))
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.Name))
            .ForMember(dest => dest.BranchId, opt => opt.MapFrom(src => src.Branch.Id))
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch.Name))
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items.Select(i => new UpdateSaleItemCommand
            {
                ProductId = i.Product.Id,
                ProductName = i.Product.Name,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            })));

        CreateMap<ListSalesRequest, ListSalesQuery>()
            .ForMember(dest => dest.Page, opt => opt.MapFrom(src => src._page))
            .ForMember(dest => dest.Size, opt => opt.MapFrom(src => src._size))
            .ForMember(dest => dest.Order, opt => opt.MapFrom(src => src._order))
            .ForMember(dest => dest.MinSaleDate, opt => opt.MapFrom(src => src._minSaleDate))
            .ForMember(dest => dest.MaxSaleDate, opt => opt.MapFrom(src => src._maxSaleDate))
            .ForMember(dest => dest.MinTotalAmount, opt => opt.MapFrom(src => src._minTotalAmount))
            .ForMember(dest => dest.MaxTotalAmount, opt => opt.MapFrom(src => src._maxTotalAmount));

        CreateMap<Application.Sales.Common.ExternalIdentityModel, ExternalIdentityResponse>();
        CreateMap<Application.Sales.Common.SaleItemModel, SaleItemResponse>();
        CreateMap<Application.Sales.Common.SaleModel, SaleResponse>();
    }
}
