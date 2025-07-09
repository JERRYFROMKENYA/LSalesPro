using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using SalesService.Application.DTOs;
using SalesService.Application.Interfaces;
using SalesService.Application.Services;
using SalesService.Domain.Entities;
using Xunit;

namespace SalesService.Tests
{
    public class OrderServiceTests
    {
        private readonly Mock<IOrderRepository> _orderRepositoryMock = new();
        private readonly Mock<IOrderItemRepository> _orderItemRepositoryMock = new();
        private readonly Mock<ICustomerRepository> _customerRepositoryMock = new();
        private readonly Mock<IInventoryServiceClient> _inventoryServiceClientMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<ILogger<OrderService>> _loggerMock = new();

        private OrderService CreateService()
        {
            return new OrderService(
                _orderRepositoryMock.Object,
                _orderItemRepositoryMock.Object,
                _customerRepositoryMock.Object,
                _inventoryServiceClientMock.Object,
                _mapperMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task GetAllAsync_ReturnsMappedOrderDtos()
        {
            // Arrange
            var orders = new List<Order> { new Order { Id = Guid.NewGuid() } };
            var orderDtos = new List<OrderDto> { new OrderDto { Id = orders[0].Id } };
            _orderRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(orders);
            _mapperMock.Setup(m => m.Map<IEnumerable<OrderDto>>(orders)).Returns(orderDtos);
            var service = CreateService();

            // Act
            var result = await service.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            var resultList = new List<OrderDto>(result);
            Assert.Single(resultList);
            Assert.Equal(orders[0].Id, resultList[0].Id);
        }

        [Fact]
        public async Task GetByIdAsync_OrderExists_ReturnsMappedOrderDto()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = new Order { Id = orderId };
            var orderDto = new OrderDto { Id = orderId };
            _orderRepositoryMock.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync(order);
            _mapperMock.Setup(m => m.Map<OrderDto>(order)).Returns(orderDto);
            var service = CreateService();

            // Act
            var result = await service.GetByIdAsync(orderId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(orderId, result.Id);
        }

        [Fact]
        public async Task GetByIdAsync_OrderDoesNotExist_ReturnsNull()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            _orderRepositoryMock.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync((Order)null);
            var service = CreateService();

            // Act
            var result = await service.GetByIdAsync(orderId);

            // Assert
            Assert.Null(result);
        }
    }
}
