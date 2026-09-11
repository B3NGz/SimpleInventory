namespace SimpleInventorySystem.Model.Entities;

public class StockTransaction
{
    public int TransactionId { get; set; }

    public int ItemId { get; set; }
    public int? SupplierId { get; set; }

    public string TransactionType { get; set; } = string.Empty;
    public decimal Quantity { get; set; }

    public DateTime TransactionDate { get; set; }
    public string? Remarks { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Item? Item { get; set; }
    public Supplier? Supplier { get; set; }
}