using Microsoft.AspNetCore.Mvc;
using InventoryService.Application.DTOs;
using InventoryService.Application.Interfaces;

namespace InventoryService.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class WarehousesController : ControllerBase
{
    private readonly IWarehouseService _warehouseService;
    private readonly ILogger<WarehousesController> _logger;

    public WarehousesController(IWarehouseService warehouseService, ILogger<WarehousesController> logger)
    {
        _warehouseService = warehouseService;
        _logger = logger;
    }

    /// <summary>
    /// Get all warehouses
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WarehouseDto>>> GetAll()
    {
        try
        {
            var warehouses = await _warehouseService.GetAllAsync();
            return Ok(warehouses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all warehouses");
            return StatusCode(500, "An error occurred while retrieving warehouses");
        }
    }

    /// <summary>
    /// Get warehouse by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WarehouseDto>> GetById(Guid id)
    {
        try
        {
            var warehouse = await _warehouseService.GetByIdAsync(id);
            if (warehouse == null)
                return NotFound($"Warehouse with ID {id} not found");

            return Ok(warehouse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving warehouse {WarehouseId}", id);
            return StatusCode(500, "An error occurred while retrieving the warehouse");
        }
    }

    /// <summary>
    /// Get warehouse by code
    /// </summary>
    [HttpGet("code/{code}")]
    public async Task<ActionResult<WarehouseDto>> GetByCode(string code)
    {
        try
        {
            var warehouse = await _warehouseService.GetByCodeAsync(code);
            if (warehouse == null)
                return NotFound($"Warehouse with code '{code}' not found");

            return Ok(warehouse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving warehouse by code {WarehouseCode}", code);
            return StatusCode(500, "An error occurred while retrieving the warehouse");
        }
    }

    /// <summary>
    /// Create a new warehouse
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<WarehouseDto>> Create(CreateWarehouseDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var warehouse = await _warehouseService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetById), new { id = warehouse.Id }, warehouse);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating warehouse");
            return StatusCode(500, "An error occurred while creating the warehouse");
        }
    }

    /// <summary>
    /// Update an existing warehouse
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<WarehouseDto>> Update(Guid id, UpdateWarehouseDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var warehouse = await _warehouseService.UpdateAsync(id, updateDto);
            if (warehouse == null)
                return NotFound($"Warehouse with ID {id} not found");

            return Ok(warehouse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating warehouse {WarehouseId}", id);
            return StatusCode(500, "An error occurred while updating the warehouse");
        }
    }

    /// <summary>
    /// Delete a warehouse (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _warehouseService.DeleteAsync(id);
            if (!result)
                return NotFound($"Warehouse with ID {id} not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting warehouse {WarehouseId}", id);
            return StatusCode(500, "An error occurred while deleting the warehouse");
        }
    }

    /// <summary>
    /// Search warehouses with advanced filtering and pagination
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<WarehousePagedResultDto>> Search([FromQuery] WarehouseSearchDto searchDto)
    {
        try
        {
            var result = await _warehouseService.SearchPagedAsync(searchDto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching warehouses");
            return StatusCode(500, "An error occurred while searching warehouses");
        }
    }

    /// <summary>
    /// Get warehouses by type
    /// </summary>
    [HttpGet("type/{type}")]
    public async Task<ActionResult<IEnumerable<WarehouseDto>>> GetByType(string type)
    {
        try
        {
            var warehouses = await _warehouseService.GetByTypeAsync(type);
            return Ok(warehouses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving warehouses by type {WarehouseType}", type);
            return StatusCode(500, "An error occurred while retrieving warehouses");
        }
    }

    /// <summary>
    /// Get active warehouses only
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<WarehouseDto>>> GetActive()
    {
        try
        {
            var warehouses = await _warehouseService.GetActiveWarehousesAsync();
            return Ok(warehouses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active warehouses");
            return StatusCode(500, "An error occurred while retrieving warehouses");
        }
    }

    /// <summary>
    /// Get nearby warehouses within specified radius
    /// </summary>
    [HttpGet("nearby")]
    public async Task<ActionResult<IEnumerable<WarehouseDto>>> GetNearby(
        [FromQuery] double latitude, 
        [FromQuery] double longitude, 
        [FromQuery] double radiusKm = 50)
    {
        try
        {
            var warehouses = await _warehouseService.GetNearbyWarehousesAsync(latitude, longitude, radiusKm);
            return Ok(warehouses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving nearby warehouses");
            return StatusCode(500, "An error occurred while retrieving nearby warehouses");
        }
    }

    /// <summary>
    /// Get warehouse capacity information
    /// </summary>
    [HttpGet("{id:guid}/capacity")]
    public async Task<ActionResult<WarehouseCapacityDto>> GetCapacity(Guid id)
    {
        try
        {
            var capacity = await _warehouseService.GetWarehouseCapacityAsync(id);
            return Ok(capacity);
        }
        catch (ArgumentException)
        {
            return NotFound($"Warehouse with ID {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving warehouse capacity for {WarehouseId}", id);
            return StatusCode(500, "An error occurred while retrieving warehouse capacity");
        }
    }

    /// <summary>
    /// Get capacity information for all warehouses
    /// </summary>
    [HttpGet("capacity/all")]
    public async Task<ActionResult<IEnumerable<WarehouseCapacityDto>>> GetAllCapacities()
    {
        try
        {
            var capacities = await _warehouseService.GetAllWarehouseCapacitiesAsync();
            return Ok(capacities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all warehouse capacities");
            return StatusCode(500, "An error occurred while retrieving warehouse capacities");
        }
    }

    /// <summary>
    /// Get warehouses with available capacity above minimum threshold
    /// </summary>
    [HttpGet("capacity/available")]
    public async Task<ActionResult<IEnumerable<WarehouseDto>>> GetWithCapacity([FromQuery] int minimumCapacity = 100)
    {
        try
        {
            var warehouses = await _warehouseService.GetWarehousesWithCapacityAsync(minimumCapacity);
            return Ok(warehouses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving warehouses with capacity");
            return StatusCode(500, "An error occurred while retrieving warehouses");
        }
    }

    /// <summary>
    /// Get all warehouse types
    /// </summary>
    [HttpGet("types")]
    public async Task<ActionResult<IEnumerable<string>>> GetTypes()
    {
        try
        {
            var types = await _warehouseService.GetWarehouseTypesAsync();
            return Ok(types);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving warehouse types");
            return StatusCode(500, "An error occurred while retrieving warehouse types");
        }
    }

    /// <summary>
    /// Activate a warehouse
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult> Activate(Guid id)
    {
        try
        {
            var result = await _warehouseService.ActivateWarehouseAsync(id);
            if (!result)
                return NotFound($"Warehouse with ID {id} not found");

            return Ok(new { message = "Warehouse activated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating warehouse {WarehouseId}", id);
            return StatusCode(500, "An error occurred while activating the warehouse");
        }
    }

    /// <summary>
    /// Deactivate a warehouse
    /// </summary>
    [HttpPost("{id:guid}/deactivate")]
    public async Task<ActionResult> Deactivate(Guid id)
    {
        try
        {
            var result = await _warehouseService.DeactivateWarehouseAsync(id);
            if (!result)
                return NotFound($"Warehouse with ID {id} not found");

            return Ok(new { message = "Warehouse deactivated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating warehouse {WarehouseId}", id);
            return StatusCode(500, "An error occurred while deactivating the warehouse");
        }
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "Warehouses Controller is healthy", timestamp = DateTime.UtcNow });
    }
}
