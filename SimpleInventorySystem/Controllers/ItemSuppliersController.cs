using Microsoft.AspNetCore.Authorization;
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
public class ItemSuppliersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly AuditLogService _auditLogService;

    public ItemSuppliersController(
        ApplicationDbContext context,
        AuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    // GET: api/itemsuppliers
    [HttpGet]
    public async Task<IActionResult> GetItemSuppliers()
    {
        var itemSuppliers = await _context.ItemSuppliers
            .Include(x => x.Item)
            .Include(x => x.Supplier)
            .Select(x => new
            {
                x.ItemSupplierId,

                x.ItemId,
                ItemCode = x.Item!.ItemCode,
                ItemName = x.Item.ItemName,

                x.SupplierId,
                SupplierCode = x.Supplier!.SupplierCode,
                SupplierName = x.Supplier.SupplierName,

                x.CreatedAt,
                x.UpdatedAt
            })
            .ToListAsync();

        return Ok(itemSuppliers);
    }

    // GET: api/itemsuppliers/1
    [HttpGet("{id}")]
    public async Task<IActionResult> GetItemSupplier(int id)
    {
        var itemSupplier = await _context.ItemSuppliers
            .Include(x => x.Item)
            .Include(x => x.Supplier)
            .Where(x => x.ItemSupplierId == id)
            .Select(x => new
            {
                x.ItemSupplierId,

                x.ItemId,
                ItemCode = x.Item!.ItemCode,
                ItemName = x.Item.ItemName,

                x.SupplierId,
                SupplierCode = x.Supplier!.SupplierCode,
                SupplierName = x.Supplier.SupplierName,

                x.CreatedAt,
                x.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (itemSupplier == null)
        {
            return NotFound();
        }

        return Ok(itemSupplier);
    }

    // POST: api/itemsuppliers
    [HttpPost]
    public async Task<IActionResult> CreateItemSupplier(
        ItemSupplierDto dto)
    {
        // Check Item
        var item = await _context.Items
            .FindAsync(dto.ItemId);

        if (item == null)
        {
            return BadRequest("Item not found.");
        }

        if (!item.IsActive)
        {
            return BadRequest("Item is inactive.");
        }

        // Check Supplier
        var supplier = await _context.Suppliers
            .FindAsync(dto.SupplierId);

        if (supplier == null)
        {
            return BadRequest("Supplier not found.");
        }

        if (!supplier.IsActive)
        {
            return BadRequest("Supplier is inactive.");
        }

        // Check duplicate relationship
        var exists = await _context.ItemSuppliers
            .AnyAsync(x =>
                x.ItemId == dto.ItemId &&
                x.SupplierId == dto.SupplierId);

        if (exists)
        {
            return BadRequest(
                "This item and supplier relationship already exists."
            );
        }

        // Create relationship
        var itemSupplier = new ItemSupplier
        {
            ItemId = dto.ItemId,
            SupplierId = dto.SupplierId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.ItemSuppliers.Add(itemSupplier);

        await _context.SaveChangesAsync();

        // Audit
        _auditLogService.AddLog(
            null,
            "CREATE",
            "ItemSupplier",
            itemSupplier.ItemSupplierId,
            null,
            new
            {
                itemSupplier.ItemId,
                itemSupplier.SupplierId
            },
            "Item supplier relationship created"
        );

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetItemSupplier),
            new { id = itemSupplier.ItemSupplierId },
            itemSupplier
        );
    }

    // DELETE: api/itemsuppliers/1
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteItemSupplier(int id)
    {
        var itemSupplier = await _context.ItemSuppliers
            .FirstOrDefaultAsync(x => x.ItemSupplierId == id);

        if (itemSupplier == null)
        {
            return NotFound();
        }

        var oldValues = new
        {
            itemSupplier.ItemId,
            itemSupplier.SupplierId
        };

        _context.ItemSuppliers.Remove(itemSupplier);

        _auditLogService.AddLog(
            null,
            "DELETE",
            "ItemSupplier",
            itemSupplier.ItemSupplierId,
            oldValues,
            null,
            "Item supplier relationship deleted"
        );

        await _context.SaveChangesAsync();

        return NoContent();
    }
}