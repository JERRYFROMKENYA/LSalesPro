using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalesService.Domain.Entities;

public class Customer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [MaxLength(100)]
    public string? FirstName { get; set; }
    
    [MaxLength(100)]
    public string? LastName { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(20)]
    public string CustomerType { get; set; } = string.Empty; // Individual, Business, etc.
    
    [Required]
    [MaxLength(20)]
    public string CustomerCategory { get; set; } = string.Empty; // Regular, Premium, Enterprise
    
    [Required]
    [MaxLength(100)]
    public string ContactPerson { get; set; } = string.Empty;
    
    [MaxLength(20)]
    public string? Phone { get; set; }
    
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }
    
    [EmailAddress]
    [MaxLength(256)]
    public string? Email { get; set; }
    
    [MaxLength(50)]
    public string? TaxId { get; set; }
    
    public int PaymentTerms { get; set; } // Days
    
    public decimal CreditLimit { get; set; }
    
    public decimal CurrentBalance { get; set; }
    
    public double? Latitude { get; set; }
    
    public double? Longitude { get; set; }
    
    [MaxLength(500)]
    public string? Address { get; set; }
    
    [MaxLength(100)]
    public string? City { get; set; }
    
    [MaxLength(50)]
    public string? State { get; set; }
    
    [MaxLength(20)]
    public string? ZipCode { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    // Computed properties
    public decimal AvailableCredit => CreditLimit - CurrentBalance;
    
    public bool HasCreditLimit => CreditLimit > 0;
    
    // Navigation properties
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(20)]
    public string OrderNumber { get; set; } = string.Empty;
    
    public Guid CustomerId { get; set; }
    
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Pending"; // Pending, Confirmed, Processing, Shipped, Delivered, Cancelled
    
    public decimal SubTotal { get; set; }
    
    public decimal TaxAmount { get; set; }
    
    public decimal DiscountAmount { get; set; }
    
    public decimal TotalAmount { get; set; }
    
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    
    public DateTime? ConfirmedAt { get; set; }
    
    public DateTime? ShippedAt { get; set; }
    
    public DateTime? DeliveredAt { get; set; }
    
    public DateTime? CancelledAt { get; set; }
    
    [MaxLength(200)]
    public string? CancellationReason { get; set; }
    
    [MaxLength(500)]
    public string? ShippingAddress { get; set; }
    
    [MaxLength(100)]
    public string? ShippingCity { get; set; }
    
    [MaxLength(50)]
    public string? ShippingState { get; set; }
    
    [MaxLength(20)]
    public string? ShippingZipCode { get; set; }
    
    [MaxLength(500)]
    public string? BillingAddress { get; set; }
    
    [MaxLength(100)]
    public string? BillingCity { get; set; }
    
    [MaxLength(50)]
    public string? BillingState { get; set; }
    
    [MaxLength(20)]
    public string? BillingZipCode { get; set; }
    
    [MaxLength(50)]
    public string? PaymentMethod { get; set; }
    
    [MaxLength(100)]
    public string? CreatedBy { get; set; }
    
    [MaxLength(100)]
    public string? UpdatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Customer Customer { get; set; } = null!;
    
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}

public class OrderItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid OrderId { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string ProductSku { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string ProductName { get; set; } = string.Empty;
    
    public int Quantity { get; set; }
    
    public decimal UnitPrice { get; set; }
    
    public decimal LineTotal { get; set; }
    
    public decimal DiscountAmount { get; set; }
    
    public decimal TaxAmount { get; set; }
    
    // For compatibility with initializer
    public decimal Discount { get; set; }
    
    [MaxLength(50)]
    public string? WarehouseCode { get; set; }
    
    [MaxLength(100)]
    public string? ReservationId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Order Order { get; set; } = null!;
}

public class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty; // OrderConfirmation, LowStock, PasswordReset, etc.
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(1000)]
    public string Message { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? RecipientId { get; set; }
    
    [EmailAddress]
    [MaxLength(256)]
    public string? RecipientEmail { get; set; }
    
    [MaxLength(50)]
    public string Priority { get; set; } = "Normal"; // Low, Normal, High, Critical
    
    public bool IsRead { get; set; } = false;
    
    public DateTime? ReadAt { get; set; }
    
    public bool IsSent { get; set; } = false;
    
    public DateTime? SentAt { get; set; }
    
    [MaxLength(500)]
    public string? ErrorMessage { get; set; }
    
    public int RetryCount { get; set; } = 0;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // JSON data for additional context
    [Column(TypeName = "TEXT")]
    public string? Data { get; set; }
}
