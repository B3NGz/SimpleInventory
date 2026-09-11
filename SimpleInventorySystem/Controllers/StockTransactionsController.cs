using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleInventorySystem.Data;
using SimpleInventorySystem.Model.DTO;
using SimpleInventorySystem.Model.Entities;
using SimpleInventorySystem.Service;

namespace SimpleInventorySystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StockTransactionsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly AuditLogService _auditLogService;

    public StockTransactionsController(
        ApplicationDbContext context,
        AuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    // GET: api/stocktransactions
    [HttpGet]
    public async Task<IActionResult> GetTransactions()
    {
        var transactions = await _context.StockTransactions
            .Include(x => x.Item)
            .Include(x => x.Supplier)
            .Select(x => new
            {
                x.TransactionId,
                x.ItemId,
                ItemCode = x.Item!.ItemCode,
                ItemName = x.Item!.ItemName,

                x.SupplierId,
                SupplierCode = x.Supplier != null
                    ? x.Supplier.SupplierCode
                    : null,
                SupplierName = x.Supplier != null
                    ? x.Supplier.SupplierName
                    : null,

                x.TransactionType,
                x.Quantity,
                x.TransactionDate,
                x.Remarks,
                x.CreatedAt,
                x.UpdatedAt
            })
            .ToListAsync();

        return Ok(transactions);
    }

    // GET: api/stocktransactions/1
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTransaction(int id)
    {
        var transaction = await _context.StockTransactions
            .Include(x => x.Item)
            .Include(x => x.Supplier)
            .Where(x => x.TransactionId == id)
            .Select(x => new
            {
                x.TransactionId,
                x.ItemId,
                ItemCode = x.Item!.ItemCode,
                ItemName = x.Item!.ItemName,

                x.SupplierId,
                SupplierCode = x.Supplier != null
                    ? x.Supplier.SupplierCode
                    : null,
                SupplierName = x.Supplier != null
                    ? x.Supplier.SupplierName
                    : null,

                x.TransactionType,
                x.Quantity,
                x.TransactionDate,
                x.Remarks,
                x.CreatedAt,
                x.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (transaction == null)
        {
            return NotFound();
        }

        return Ok(transaction);
    }

    // POST: api/stocktransactions
    [HttpPost]
    public async Task<IActionResult> CreateTransaction(
        StockTransactionDto dto)
    {
        // Check Item
        var item = await _context.Items.FindAsync(dto.ItemId);

        if (item == null)
        {
            return BadRequest("Item not found.");
        }

        if (!item.IsActive)
        {
            return BadRequest("Item is inactive.");
        }

        // Check Supplier
        if (dto.SupplierId.HasValue)
        {
            var supplier = await _context.Suppliers
                .FindAsync(dto.SupplierId.Value);

            if (supplier == null)
            {
                return BadRequest("Supplier not found.");
            }

            if (!supplier.IsActive)
            {
                return BadRequest("Supplier is inactive.");
            }
        }

        // Validate Transaction Type
        if (dto.TransactionType != "IN" &&
            dto.TransactionType != "OUT")
        {
            return BadRequest(
                "TransactionType must be IN or OUT."
            );
        }

        // Validate Quantity
        if (dto.Quantity <= 0)
        {
            return BadRequest(
                "Quantity must be greater than zero."
            );
        }

        // Create transaction
        var transaction = new StockTransaction
        {
            ItemId = dto.ItemId,
            SupplierId = dto.SupplierId,
            TransactionType = dto.TransactionType,
            Quantity = dto.Quantity,

            TransactionDate = dto.TransactionDate == default
                ? DateTime.UtcNow
                : dto.TransactionDate,

            Remarks = dto.Remarks,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Update item quantity
        if (transaction.TransactionType == "IN")
        {
            item.Quantity += transaction.Quantity;
        }
        else
        {
            if (item.Quantity < transaction.Quantity)
            {
                return BadRequest("Insufficient stock.");
            }

            item.Quantity -= transaction.Quantity;
        }

        _context.StockTransactions.Add(transaction);

        await _context.SaveChangesAsync();

        // Audit Log
        _auditLogService.AddLog(
            null,
            "CREATE",
            "StockTransaction",
            transaction.TransactionId,
            null,
            new
            {
                transaction.TransactionId,
                transaction.ItemId,
                transaction.SupplierId,
                transaction.TransactionType,
                transaction.Quantity,
                transaction.TransactionDate,
                transaction.Remarks
            },
            "Stock transaction created"
        );

        await _context.SaveChangesAsync();

        // Return clean response
        return CreatedAtAction(
            nameof(GetTransaction),
            new { id = transaction.TransactionId },
            new
            {
                transaction.TransactionId,
                transaction.ItemId,
                transaction.SupplierId,
                transaction.TransactionType,
                transaction.Quantity,
                transaction.TransactionDate,
                transaction.Remarks,
                transaction.CreatedAt,
                transaction.UpdatedAt
            }
        );
    }

    // PUT: api/stocktransactions/1
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTransaction(
        int id,
        StockTransactionDto dto)
    {
        var transaction = await _context.StockTransactions
            .FirstOrDefaultAsync(x => x.TransactionId == id);

        if (transaction == null)
        {
            return NotFound();
        }

        // Validate Transaction Type
        if (dto.TransactionType != "IN" &&
            dto.TransactionType != "OUT")
        {
            return BadRequest(
                "TransactionType must be IN or OUT."
            );
        }

        // Validate Quantity
        if (dto.Quantity <= 0)
        {
            return BadRequest(
                "Quantity must be greater than zero."
            );
        }

        // Check Item
        var item = await _context.Items
            .FindAsync(transaction.ItemId);

        if (item == null)
        {
            return BadRequest("Item not found.");
        }

        // Check Supplier
        if (dto.SupplierId.HasValue)
        {
            var supplier = await _context.Suppliers
                .FindAsync(dto.SupplierId.Value);

            if (supplier == null)
            {
                return BadRequest("Supplier not found.");
            }

            if (!supplier.IsActive)
            {
                return BadRequest("Supplier is inactive.");
            }
        }

        // Save old values
        var oldValues = new
        {
            transaction.ItemId,
            transaction.SupplierId,
            transaction.TransactionType,
            transaction.Quantity,
            transaction.TransactionDate,
            transaction.Remarks
        };

        // Reverse old transaction from stock
        if (transaction.TransactionType == "IN")
        {
            item.Quantity -= transaction.Quantity;
        }
        else
        {
            item.Quantity += transaction.Quantity;
        }

        // Check resulting stock
        if (item.Quantity < 0)
        {
            return BadRequest(
                "Cannot update transaction because stock would become negative."
            );
        }

        // Apply new transaction
        if (dto.TransactionType == "IN")
        {
            item.Quantity += dto.Quantity;
        }
        else
        {
            if (item.Quantity < dto.Quantity)
            {
                return BadRequest("Insufficient stock.");
            }

            item.Quantity -= dto.Quantity;
        }

        // Update transaction
        transaction.SupplierId = dto.SupplierId;
        transaction.TransactionType = dto.TransactionType;
        transaction.Quantity = dto.Quantity;

        transaction.TransactionDate = dto.TransactionDate == default
            ? transaction.TransactionDate
            : dto.TransactionDate;

        transaction.Remarks = dto.Remarks;
        transaction.UpdatedAt = DateTime.UtcNow;

        var newValues = new
        {
            transaction.ItemId,
            transaction.SupplierId,
            transaction.TransactionType,
            transaction.Quantity,
            transaction.TransactionDate,
            transaction.Remarks
        };

        // Audit
        _auditLogService.AddLog(
            null,
            "UPDATE",
            "StockTransaction",
            transaction.TransactionId,
            oldValues,
            newValues,
            "Stock transaction updated"
        );

        await _context.SaveChangesAsync();

        return Ok(new
        {
            transaction.TransactionId,
            transaction.ItemId,
            transaction.SupplierId,
            transaction.TransactionType,
            transaction.Quantity,
            transaction.TransactionDate,
            transaction.Remarks,
            transaction.CreatedAt,
            transaction.UpdatedAt
        });
    }

    // DELETE: api/stocktransactions/1
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTransaction(int id)
    {
        var transaction = await _context.StockTransactions
            .FirstOrDefaultAsync(x => x.TransactionId == id);

        if (transaction == null)
        {
            return NotFound();
        }

        var item = await _context.Items
            .FindAsync(transaction.ItemId);

        if (item == null)
        {
            return BadRequest("Item not found.");
        }

        var oldValues = new
        {
            transaction.ItemId,
            transaction.SupplierId,
            transaction.TransactionType,
            transaction.Quantity,
            transaction.TransactionDate,
            transaction.Remarks
        };

        // Reverse transaction from stock
        if (transaction.TransactionType == "IN")
        {
            if (item.Quantity < transaction.Quantity)
            {
                return BadRequest(
                    "Cannot delete transaction because stock would become negative."
                );
            }

            item.Quantity -= transaction.Quantity;
        }
        else
        {
            item.Quantity += transaction.Quantity;
        }

        _context.StockTransactions.Remove(transaction);

        _auditLogService.AddLog(
            null,
            "DELETE",
            "StockTransaction",
            transaction.TransactionId,
            oldValues,
            null,
            "Stock transaction deleted"
        );

        await _context.SaveChangesAsync();

        return NoContent();
    }
}