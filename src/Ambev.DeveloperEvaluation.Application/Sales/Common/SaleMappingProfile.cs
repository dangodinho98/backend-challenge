using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.Application.Sales.Common;

public class SaleMappingProfile : Profile
{
    public SaleMappingProfile()
    {
        CreateMap<ExternalIdentity, ExternalIdentityModel>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Description));

        CreateMap<SaleItem, SaleItemModel>();
        CreateMap<Sale, SaleModel>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
    }
}

public static class SaleMapper
{
    public static SaleModel ToModel(Sale sale, IMapper mapper) => mapper.Map<SaleModel>(sale);
}
