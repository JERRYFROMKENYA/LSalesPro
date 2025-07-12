using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesService.Application.DTOs;
using SalesService.Application.Interfaces;

namespace SalesService.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class ReportsController : ControllerBase
{
    private readonly IReportsService _reportsService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(IReportsService reportsService, ILogger<ReportsController> logger)
    {
        _reportsService = reportsService;
        _logger = logger;
    }

    /// <summary>
    /// Get sales summary report for a date range
    /// </summary>
    /// <param name="startDate">Start date of the report</param>
    /// <param name="endDate">End date of the report</param>
    /// <returns>Sales summary report</returns>
    [HttpGet("sales/summary")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SalesSummaryReportDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SalesSummaryReportDto>> GetSalesSummaryReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            if (endDate < startDate)
            {
                return BadRequest("End date must be greater than or equal to start date");
            }

            var dateRange = new DateRangeDto { StartDate = startDate, EndDate = endDate };
            var report = await _reportsService.GetSalesSummaryReportAsync(dateRange);
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating sales summary report for period {StartDate} to {EndDate}", 
                startDate, endDate);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get daily sales report for a date range
    /// </summary>
    /// <param name="startDate">Start date of the report</param>
    /// <param name="endDate">End date of the report</param>
    /// <returns>List of daily sales</returns>
    [HttpGet("sales/daily")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<DailySalesReportDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<DailySalesReportDto>>> GetDailySalesReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            if (endDate < startDate)
            {
                return BadRequest("End date must be greater than or equal to start date");
            }

            var dateRange = new DateRangeDto { StartDate = startDate, EndDate = endDate };
            var report = await _reportsService.GetDailySalesReportAsync(dateRange);
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating daily sales report for period {StartDate} to {EndDate}", 
                startDate, endDate);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get monthly sales report for a year
    /// </summary>
    /// <param name="year">Year for the report</param>
    /// <returns>List of monthly sales</returns>
    [HttpGet("sales/monthly/{year}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<MonthlySalesReportDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<MonthlySalesReportDto>>> GetMonthlySalesReport(int year)
    {
        try
        {
            if (year < 2000 || year > 2100)
            {
                return BadRequest("Year must be between 2000 and 2100");
            }

            var report = await _reportsService.GetMonthlySalesReportAsync(year);
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating monthly sales report for year {Year}", year);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get top selling products for a date range
    /// </summary>
    /// <param name="startDate">Start date of the report</param>
    /// <param name="endDate">End date of the report</param>
    /// <param name="limit">Maximum number of products to return</param>
    /// <returns>List of top selling products</returns>
    [HttpGet("products/top-selling")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ProductSalesReportDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<ProductSalesReportDto>>> GetTopSellingProducts(
        [FromQuery] DateTime startDate, 
        [FromQuery] DateTime endDate, 
        [FromQuery] int limit = 10)
    {
        try
        {
            if (endDate < startDate)
            {
                return BadRequest("End date must be greater than or equal to start date");
            }

            if (limit <= 0)
            {
                return BadRequest("Limit must be greater than 0");
            }

            var dateRange = new DateRangeDto { StartDate = startDate, EndDate = endDate };
            var report = await _reportsService.GetTopSellingProductsAsync(dateRange, limit);
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating top selling products report for period {StartDate} to {EndDate}", 
                startDate, endDate);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get top customers for a date range
    /// </summary>
    /// <param name="startDate">Start date of the report</param>
    /// <param name="endDate">End date of the report</param>
    /// <param name="limit">Maximum number of customers to return</param>
    /// <returns>List of top customers</returns>
    [HttpGet("customers/top")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<CustomerSalesReportDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<CustomerSalesReportDto>>> GetTopCustomers(
        [FromQuery] DateTime startDate, 
        [FromQuery] DateTime endDate, 
        [FromQuery] int limit = 10)
    {
        try
        {
            if (endDate < startDate)
            {
                return BadRequest("End date must be greater than or equal to start date");
            }

            if (limit <= 0)
            {
                return BadRequest("Limit must be greater than 0");
            }

            var dateRange = new DateRangeDto { StartDate = startDate, EndDate = endDate };
            var report = await _reportsService.GetTopCustomersAsync(dateRange, limit);
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating top customers report for period {StartDate} to {EndDate}", 
                startDate, endDate);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get customer activity report for a specific customer
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="startDate">Start date of the report</param>
    /// <param name="endDate">End date of the report</param>
    /// <returns>Customer activity report</returns>
    [HttpGet("customers/{customerId}/activity")]
    [Authorize(Roles = "Admin,Manager,Sales")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CustomerActivityReportDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CustomerActivityReportDto>> GetCustomerActivityReport(
        Guid customerId, 
        [FromQuery] DateTime startDate, 
        [FromQuery] DateTime endDate)
    {
        try
        {
            if (endDate < startDate)
            {
                return BadRequest("End date must be greater than or equal to start date");
            }

            var dateRange = new DateRangeDto { StartDate = startDate, EndDate = endDate };
            var report = await _reportsService.GetCustomerActivityReportAsync(customerId, dateRange);
            
            if (report == null)
            {
                return NotFound($"Customer with ID {customerId} not found or has no activity in the specified period");
            }
            
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating customer activity report for customer {CustomerId}", customerId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get customer segment report
    /// </summary>
    /// <returns>List of customer segments with statistics</returns>
    [HttpGet("customers/segments")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<CustomerSegmentReportDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<CustomerSegmentReportDto>>> GetCustomerSegmentReport()
    {
        try
        {
            var report = await _reportsService.GetCustomerSegmentReportAsync();
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating customer segment report");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get sales performance report for a date range
    /// </summary>
    /// <param name="startDate">Start date of the report</param>
    /// <param name="endDate">End date of the report</param>
    /// <returns>Sales performance report</returns>
    [HttpGet("performance/sales")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SalesPerformanceReportDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SalesPerformanceReportDto>> GetSalesPerformanceReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            if (endDate < startDate)
            {
                return BadRequest("End date must be greater than or equal to start date");
            }

            var dateRange = new DateRangeDto { StartDate = startDate, EndDate = endDate };
            var report = await _reportsService.GetSalesPerformanceReportAsync(dateRange);
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating sales performance report for period {StartDate} to {EndDate}", 
                startDate, endDate);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get product performance report for a date range
    /// </summary>
    /// <param name="startDate">Start date of the report</param>
    /// <param name="endDate">End date of the report</param>
    /// <returns>Product performance report</returns>
    [HttpGet("performance/products")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<SalesPerformanceByProductDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<SalesPerformanceByProductDto>>> GetProductPerformanceReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            if (endDate < startDate)
            {
                return BadRequest("End date must be greater than or equal to start date");
            }

            var dateRange = new DateRangeDto { StartDate = startDate, EndDate = endDate };
            var report = await _reportsService.GetProductPerformanceReportAsync(dateRange);
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating product performance report for period {StartDate} to {EndDate}", 
                startDate, endDate);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get customer category performance report for a date range
    /// </summary>
    /// <param name="startDate">Start date of the report</param>
    /// <param name="endDate">End date of the report</param>
    /// <returns>Customer category performance report</returns>
    [HttpGet("performance/customer-categories")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<SalesPerformanceByCustomerCategoryDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<SalesPerformanceByCustomerCategoryDto>>> GetCustomerCategoryPerformanceReport(
        [FromQuery] DateTime startDate, 
        [FromQuery] DateTime endDate)
    {
        try
        {
            if (endDate < startDate)
            {
                return BadRequest("End date must be greater than or equal to start date");
            }

            var dateRange = new DateRangeDto { StartDate = startDate, EndDate = endDate };
            var report = await _reportsService.GetCustomerCategoryPerformanceReportAsync(dateRange);
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating customer category performance report for period {StartDate} to {EndDate}", 
                startDate, endDate);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get dashboard summary metrics
    /// </summary>
    /// <returns>Dashboard summary data</returns>
    [HttpGet("/api/v1/dashboard/summary")]
    [Authorize(Roles = "Admin,Manager,SalesManager,SalesRepresentative")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DashboardSummaryDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DashboardSummaryDto>> GetDashboardSummary()
    {
        try
        {
            var summary = await _reportsService.GetDashboardSummaryAsync();
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard summary");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get sales performance data
    /// </summary>
    /// <returns>Sales performance data</returns>
    [HttpGet("/api/v1/dashboard/sales-performance")]
    [Authorize(Roles = "Admin,Manager,SalesManager,SalesRepresentative")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SalesPerformanceDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SalesPerformanceDto>> GetSalesPerformance()
    {
        try
        {
            var performance = await _reportsService.GetSalesPerformanceAsync();
            return Ok(performance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sales performance");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get top products (best sellers)
    /// </summary>
    /// <returns>List of top products</returns>
    [HttpGet("/api/v1/dashboard/top-products")]
    [Authorize(Roles = "Admin,Manager,SalesManager,SalesRepresentative")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TopProductDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<TopProductDto>>> GetTopProducts()
    {
        try
        {
            var topProducts = await _reportsService.GetTopProductsAsync();
            return Ok(topProducts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top products");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get customer insights analytics
    /// </summary>
    /// <returns>Customer insights data</returns>
    [HttpGet("/api/v1/dashboard/customer-insights")]
    [Authorize(Roles = "Admin,Manager,SalesManager,SalesRepresentative")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CustomerInsightsDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CustomerInsightsDto>> GetCustomerInsights()
    {
        try
        {
            var insights = await _reportsService.GetCustomerInsightsAsync();
            return Ok(insights);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer insights");
            return StatusCode(500, "Internal server error");
        }
    }
}
