namespace SalesService.Application.DTOs;

public class SalesPerformanceDto
{
    public decimal TotalSales { get; set; }
    public int TotalOrders { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int SalesTarget { get; set; }
    public decimal SalesTargetProgress { get; set; } // percentage
    public int Period { get; set; } // e.g. month or week number
}
