using AutoMapper;
using SalesService.Application.DTOs;
using SalesService.Domain.Entities;

namespace SalesService.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Customer mappings
        CreateMap<Customer, CustomerDto>()
            .ForMember(dest => dest.AvailableCredit, opt => opt.MapFrom(src => src.CreditLimit - src.CurrentBalance))
            .ForMember(dest => dest.OrderCount, opt => opt.MapFrom(src => src.Orders.Count))
            .ForMember(dest => dest.TotalPurchases, opt => opt.MapFrom(src => src.Orders.Sum(o => o.TotalAmount)));
            
        CreateMap<CreateCustomerDto, Customer>();
        CreateMap<UpdateCustomerDto, Customer>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            
        // Order mappings
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.Name))
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.OrderItems));
            
        CreateMap<CreateOrderDto, Order>()
            .ForMember(dest => dest.OrderItems, opt => opt.Ignore())
            .ForMember(dest => dest.OrderNumber, opt => opt.Ignore())
            .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
            .ForMember(dest => dest.SubTotal, opt => opt.Ignore())
            .ForMember(dest => dest.TaxAmount, opt => opt.Ignore())
            .ForMember(dest => dest.DiscountAmount, opt => opt.Ignore());
            
        CreateMap<UpdateOrderDto, Order>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            
        // OrderItem mappings
        CreateMap<OrderItem, OrderItemDto>();
        
        CreateMap<CreateOrderItemDto, OrderItem>()
            .ForMember(dest => dest.LineTotal, opt => opt.Ignore());
            
        CreateMap<UpdateOrderItemDto, OrderItem>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            
        CreateMap<AddOrderItemDto, OrderItem>()
            .ForMember(dest => dest.LineTotal, opt => opt.Ignore());
            
        // Notification mappings
        CreateMap<Notification, NotificationDto>();
        CreateMap<CreateNotificationDto, Notification>();
    }
}
