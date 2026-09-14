using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleInventorySystem.Data;
using SimpleInventorySystem.Model.DTO;
using SimpleInventorySystem.Model.Entities;
using SimpleInventorySystem.Service;
using Microsoft.AspNetCore.Authorization;

namespace SimpleInventorySystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ItemsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly AuditLogService _auditLogService;

    public ItemsController(
        ApplicationDbContext context,
        AuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    // GET: api/items
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Item>>> GetItems()
    {
        var items = await _context.Items.ToListAsync();

        return Ok(items);
    }

    // GET: api/items/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Item>> GetItem(int id)
    {
        var item = await _context.Items.FindAsync(id);

        if (item == null)
        {
            return NotFound();
        }

        return Ok(item);
    }

    // POST: api/items
    [HttpPost]
    public async Task<ActionResult<Item>> CreateItem(ItemDto dto)
    {
        var item = new Item
        {
            ItemCode = dto.ItemCode,
            ItemName = dto.ItemName,
            Category = dto.Category,
            Unit = dto.Unit,
            Quantity = dto.Quantity,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Items.Add(item);

        await _context.SaveChangesAsync();

        _auditLogService.AddLog(
            null,
            "CREATE",
            "Item",
            item.ItemId,
            null,
            item,
            "Item created"
        );

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetItem),
            new { id = item.ItemId },
            item
        );
    }

    // PUT: api/items/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateItem(int id, ItemDto dto)
    {
        var item = await _context.Items.FindAsync(id);

        if (item == null)
        {
            return NotFound();
        }

        var oldValues = new
        {
            item.ItemCode,
            item.ItemName,
            item.Category,
            item.Unit,
            item.Quantity,
            item.IsActive
        };

        item.ItemCode = dto.ItemCode;
        item.ItemName = dto.ItemName;
        item.Category = dto.Category;
        item.Unit = dto.Unit;
        item.Quantity = dto.Quantity;
        item.IsActive = dto.IsActive;
        item.UpdatedAt = DateTime.UtcNow;

        _auditLogService.AddLog(
            null,
            "UPDATE",
            "Item",
            item.ItemId,
            oldValues,
            item,
            "Item updated"
        );

        await _context.SaveChangesAsync();

        return Ok(item);
    }

    // DELETE: api/items/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteItem(int id)
    {
        var item = await _context.Items.FindAsync(id);

        if (item == null)
        {
            return NotFound();
        }

        var oldValues = new
        {
            item.ItemCode,
            item.ItemName,
            item.Category,
            item.Unit,
            item.Quantity,
            item.IsActive
        };

        item.IsActive = false;
        item.UpdatedAt = DateTime.UtcNow;

        var newValues = new
        {
            item.ItemCode,
            item.ItemName,
            item.Category,
            item.Unit,
            item.Quantity,
            item.IsActive
        };

        _auditLogService.AddLog(
            null,
            "DELETE",
            "Item",
            item.ItemId,
            oldValues,
            newValues,
            "Item deactivated"
        );

        await _context.SaveChangesAsync();

        return Ok(item);
    }
}