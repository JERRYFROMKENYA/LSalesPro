namespace SalesService.Application.DTOs
{
    public class ProductPricingDto
    {
        public string ProductId { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public decimal TaxRate { get; set; }
    }
}