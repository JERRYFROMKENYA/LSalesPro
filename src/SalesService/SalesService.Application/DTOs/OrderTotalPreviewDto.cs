namespace SalesService.Application.DTOs
{
    public class OrderTotalPreviewDto
    {
        public string OrderId { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
    }
}

