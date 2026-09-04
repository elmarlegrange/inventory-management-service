using InventoryManagement.Application.Stock;
using InventoryManagement.Application.Warehouses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("warehouses")]
[Produces("application/json", "application/problem+json")]
public sealed class WarehousesController : ControllerBase
{
    private readonly IWarehouseService _warehouseService;

    public WarehousesController(IWarehouseService warehouseService)
    {
        _warehouseService = warehouseService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<WarehouseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<WarehouseDto>>> GetAll(CancellationToken cancellationToken)
    {
        var warehouses = await _warehouseService.GetAllWarehousesAsync(cancellationToken);
        return Ok(warehouses);
    }

    [HttpGet("{code}")]
    [ProducesResponseType(typeof(WarehouseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WarehouseDto>> GetByCode(string code, CancellationToken cancellationToken)
    {
        var warehouse = await _warehouseService.GetWarehouseByCodeAsync(code, cancellationToken);
        return Ok(warehouse);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(WarehouseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<WarehouseDto>> Create([FromBody] CreateWarehouseRequest request, CancellationToken cancellationToken)
    {
        var created = await _warehouseService.CreateWarehouseAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetByCode), new { code = created.Code }, created);
    }

    [HttpGet("{code}/stock")]
    [ProducesResponseType(typeof(IReadOnlyList<WarehouseStockItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<WarehouseStockItemDto>>> GetStock(
        [FromRoute] string code,
        CancellationToken cancellationToken)
    {
        var stock = await _warehouseService.GetStockForWarehouseAsync(code, cancellationToken);
        return Ok(stock);
    }

    [HttpPost("{code}/stock")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddStock(
        [FromRoute] string code,
        [FromBody] AddStockItemRequest request,
        CancellationToken cancellationToken)
    {
        await _warehouseService.AddStockToWarehouseAsync(code, request, cancellationToken);
        return Ok();
    }
}
