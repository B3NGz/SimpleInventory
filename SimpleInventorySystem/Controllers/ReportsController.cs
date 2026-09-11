using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleInventorySystem.Data;
using SimpleInventorySystem.Model.Entities;

namespace SimpleInventorySystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ReportsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // =========================================================
    // 1. AUDIT LOG REPORT
    // GET: api/reports/audit-logs
    // =========================================================
    [HttpGet("audit-logs")]
    public async Task<ActionResult<IEnumerable<AuditLog>>> GetAuditLogs()
    {
        var auditLogs = await _context.AuditLogs
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(auditLogs);
    }

    // =========================================================
    // 2. ITEM + SUPPLIER REPORT
    // GET: api/reports/item-supplier
    // =========================================================
    [HttpGet("item-supplier")]
    public async Task<IActionResult> GetItemSupplierReport()
    {
        var report = await _context.ItemSuppliers
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
                SupplierName = x.Supplier.SupplierName
            })
            .ToListAsync();

        return Ok(report);
    }

    // =========================================================
    // 3. ITEM + TRANSACTION REPORT
    // GET: api/reports/item-transactions
    // =========================================================
    [HttpGet("item-transactions")]
    public async Task<IActionResult> GetItemTransactionReport()
    {
        var report = await _context.StockTransactions
            .Include(x => x.Item)
            .Select(x => new
            {
                x.TransactionId,

                x.ItemId,
                ItemCode = x.Item!.ItemCode,
                ItemName = x.Item.ItemName,

                x.TransactionType,
                x.Quantity,
                x.TransactionDate,
                x.Remarks
            })
            .ToListAsync();

        return Ok(report);
    }

    // =========================================================
    // 4. ITEM + SUPPLIER + TRANSACTION REPORT
    // GET: api/reports/item-supplier-transactions
    // =========================================================
    [HttpGet("item-supplier-transactions")]
    public async Task<IActionResult> GetItemSupplierTransactionReport()
    {
        var report = await _context.StockTransactions
            .Include(x => x.Item)
            .Include(x => x.Supplier)
            .Select(x => new
            {
                x.TransactionId,

                x.ItemId,
                ItemCode = x.Item!.ItemCode,
                ItemName = x.Item.ItemName,

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
                x.Remarks
            })
            .ToListAsync();

        return Ok(report);
    }
}