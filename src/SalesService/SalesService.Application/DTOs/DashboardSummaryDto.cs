namespace SalesService.Application.DTOs;

public class DashboardSummaryDto
{
    public int TotalOrders { get; set; }
    public int TotalCustomers { get; set; }
    public decimal TotalRevenue { get; set; }
    public int ActiveCustomers { get; set; }
    public int NewCustomers { get; set; }
    public int OrdersToday { get; set; }
    public decimal RevenueToday { get; set; }
}
