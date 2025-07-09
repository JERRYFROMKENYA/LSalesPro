namespace SalesService.Application.DTOs;

public class CustomerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string CustomerType { get; set; } = string.Empty; // Garage, Dealership, etc.
    public string CustomerCategory { get; set; } = string.Empty; // A, A+, B, C
    public string ContactPerson { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? TaxId { get; set; }
    public int PaymentTerms { get; set; } // Days
    public decimal CreditLimit { get; set; }
    public decimal CurrentBalance { get; set; }
    public decimal AvailableCredit { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int OrderCount { get; set; }
    public decimal TotalPurchases { get; set; }
}

public class CreateCustomerDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? TaxId { get; set; }
    public int PaymentTerms { get; set; } = 30; // Default 30 days
    public decimal CreditLimit { get; set; } = 0;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateCustomerDto
{
    public string? Name { get; set; }
    public string? Type { get; set; }
    public string? Category { get; set; }
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? TaxId { get; set; }
    public int? PaymentTerms { get; set; }
    public decimal? CreditLimit { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Address { get; set; }
    public bool? IsActive { get; set; }
}

public class CustomerSearchDto
{
    public string? SearchTerm { get; set; }
    public string? Type { get; set; }
    public string? Category { get; set; }
    public bool? IsActive { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "Name";
    public bool SortAscending { get; set; } = true;
}

public class CustomerPagedResultDto
{
    public IEnumerable<CustomerDto> Customers { get; set; } = new List<CustomerDto>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasPrevious { get; set; }
    public bool HasNext { get; set; }
}
