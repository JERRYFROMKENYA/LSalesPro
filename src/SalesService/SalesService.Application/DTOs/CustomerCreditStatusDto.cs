namespace SalesService.Application.DTOs
{
    public class CustomerCreditStatusDto
    {
        public Guid CustomerId { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal AvailableCredit { get; set; }
        public bool HasCreditLimit { get; set; }
        public bool IsCreditAvailable { get; set; }
    }
}