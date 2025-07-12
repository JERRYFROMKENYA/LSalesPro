namespace SalesService.Application.DTOs;

public class CustomerInsightsDto
{
    public int TotalCustomers { get; set; }
    public int NewCustomersThisPeriod { get; set; }
    public int ReturningCustomers { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int ChurnedCustomers { get; set; }
    public string TopCustomerCategory { get; set; } = string.Empty;
    public int LoyalCustomers { get; set; }
}
