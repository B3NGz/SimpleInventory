using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleInventorySystem.Data;
using SimpleInventorySystem.Model.DTO;
using SimpleInventorySystem.Model.Entities;
using SimpleInventorySystem.Service;

namespace SimpleInventorySystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SuppliersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly AuditLogService _auditLogService;

    public SuppliersController(
        ApplicationDbContext context,
        AuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    // GET: api/suppliers
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Supplier>>> GetSuppliers()
    {
        var suppliers = await _context.Suppliers
            .Where(x => x.IsActive)
            .ToListAsync();

        return Ok(suppliers);
    }

    // GET: api/suppliers/1
    [HttpGet("{id}")]
    public async Task<ActionResult<Supplier>> GetSupplier(int id)
    {
        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(x => x.SupplierId == id && x.IsActive);

        if (supplier == null)
        {
            return NotFound();
        }

        return Ok(supplier);
    }

    // POST: api/suppliers
    [HttpPost]
    public async Task<ActionResult<Supplier>> CreateSupplier(SupplierDto dto)
    {
        var supplier = new Supplier
        {
            SupplierCode = dto.SupplierCode,
            SupplierName = dto.SupplierName,
            ContactPerson = dto.ContactPerson,
            ContactNumber = dto.ContactNumber,
            Email = dto.Email,
            Address = dto.Address,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Suppliers.Add(supplier);

        await _context.SaveChangesAsync();

        _auditLogService.AddLog(
            null,
            "CREATE",
            "Supplier",
            supplier.SupplierId,
            null,
            new
            {
                supplier.SupplierCode,
                supplier.SupplierName,
                supplier.ContactPerson,
                supplier.ContactNumber,
                supplier.Email,
                supplier.Address,
                supplier.IsActive
            },
            "Supplier created"
        );

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetSupplier),
            new { id = supplier.SupplierId },
            supplier
        );
    }

    // PUT: api/suppliers/1
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSupplier(int id, SupplierDto dto)
    {
        var supplier = await _context.Suppliers.FindAsync(id);

        if (supplier == null)
        {
            return NotFound();
        }

        var oldValues = new
        {
            supplier.SupplierCode,
            supplier.SupplierName,
            supplier.ContactPerson,
            supplier.ContactNumber,
            supplier.Email,
            supplier.Address,
            supplier.IsActive
        };

        supplier.SupplierCode = dto.SupplierCode;
        supplier.SupplierName = dto.SupplierName;
        supplier.ContactPerson = dto.ContactPerson;
        supplier.ContactNumber = dto.ContactNumber;
        supplier.Email = dto.Email;
        supplier.Address = dto.Address;
        supplier.IsActive = dto.IsActive;
        supplier.UpdatedAt = DateTime.UtcNow;

        var newValues = new
        {
            supplier.SupplierCode,
            supplier.SupplierName,
            supplier.ContactPerson,
            supplier.ContactNumber,
            supplier.Email,
            supplier.Address,
            supplier.IsActive
        };

        _auditLogService.AddLog(
            null,
            "UPDATE",
            "Supplier",
            supplier.SupplierId,
            oldValues,
            newValues,
            "Supplier updated"
        );

        await _context.SaveChangesAsync();

        return Ok(supplier);
    }

    // DELETE: api/suppliers/1
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSupplier(int id)
    {
        var supplier = await _context.Suppliers.FindAsync(id);

        if (supplier == null)
        {
            return NotFound();
        }

        var oldValues = new
        {
            supplier.SupplierCode,
            supplier.SupplierName,
            supplier.ContactPerson,
            supplier.ContactNumber,
            supplier.Email,
            supplier.Address,
            supplier.IsActive
        };

        supplier.IsActive = false;
        supplier.UpdatedAt = DateTime.UtcNow;

        var newValues = new
        {
            supplier.SupplierCode,
            supplier.SupplierName,
            supplier.ContactPerson,
            supplier.ContactNumber,
            supplier.Email,
            supplier.Address,
            supplier.IsActive
        };

        _auditLogService.AddLog(
            null,
            "DELETE",
            "Supplier",
            supplier.SupplierId,
            oldValues,
            newValues,
            "Supplier deactivated"
        );

        await _context.SaveChangesAsync();

        return Ok(supplier);
    }
}