using SalesService.Domain.Entities;

namespace SalesService.Application.DTOs
{
    public class CalculateOrderTotalDto
    {
        public Guid? CustomerId { get; set; }
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
    }

    public class OrderCalculationResultDto
    {
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderItemDto> CalculatedItems { get; set; } = new List<OrderItemDto>();
    }
}