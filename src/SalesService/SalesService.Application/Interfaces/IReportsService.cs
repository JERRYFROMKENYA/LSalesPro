using SalesService.Application.DTOs;

namespace SalesService.Application.Interfaces;

public interface IReportsService
{
    // Sales Reports
    Task<SalesSummaryReportDto> GetSalesSummaryReportAsync(DateRangeDto dateRange);
    Task<List<DailySalesReportDto>> GetDailySalesReportAsync(DateRangeDto dateRange);
    Task<List<MonthlySalesReportDto>> GetMonthlySalesReportAsync(int year);
    Task<List<ProductSalesReportDto>> GetTopSellingProductsAsync(DateRangeDto dateRange, int limit = 10);
    
    // Customer Reports
    Task<List<CustomerSalesReportDto>> GetTopCustomersAsync(DateRangeDto dateRange, int limit = 10);
    Task<CustomerActivityReportDto> GetCustomerActivityReportAsync(Guid customerId, DateRangeDto dateRange);
    Task<List<CustomerSegmentReportDto>> GetCustomerSegmentReportAsync();
    Task<CustomerOrderSummaryDto> GetCustomerOrderSummaryAsync(Guid customerId, DateTime fromDate, DateTime toDate);
    
    // Performance Reports
    Task<SalesPerformanceReportDto> GetSalesPerformanceReportAsync(DateRangeDto dateRange);
    Task<List<SalesPerformanceByProductDto>> GetProductPerformanceReportAsync(DateRangeDto dateRange);
    Task<List<SalesPerformanceByCustomerCategoryDto>> GetCustomerCategoryPerformanceReportAsync(DateRangeDto dateRange);

    // Dashboard Analytics
    Task<DashboardSummaryDto> GetDashboardSummaryAsync();
    Task<SalesPerformanceDto> GetSalesPerformanceAsync();
    Task<IEnumerable<TopProductDto>> GetTopProductsAsync();
    Task<CustomerInsightsDto> GetCustomerInsightsAsync();
}
