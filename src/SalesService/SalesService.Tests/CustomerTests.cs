using System;
using SalesService.Domain.Entities;
using Xunit;

namespace SalesService.Tests
{
    public class CustomerTests
    {
        [Fact]
        public void Customer_CreatedWithDefaults_HasValidDefaults()
        {
            // Arrange
            var customer = new Customer();

            // Assert
            Assert.NotEqual(Guid.Empty, customer.Id);
            Assert.NotNull(customer.Name);
            Assert.True(customer.IsActive);
            Assert.Equal(0, customer.CurrentBalance);
        }
    }
}

