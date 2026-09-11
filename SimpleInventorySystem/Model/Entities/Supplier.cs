namespace SimpleInventorySystem.Model.Entities;

public class Supplier
{
    public int SupplierId { get; set; }
    public string SupplierCode { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? ContactNumber { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<StockTransaction> StockTransactions { get; set; }
        = new List<StockTransaction>();
}