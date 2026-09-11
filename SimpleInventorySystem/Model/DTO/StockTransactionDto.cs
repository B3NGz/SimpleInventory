namespace SimpleInventorySystem.Model.DTO;

public class StockTransactionDto
{
    public int ItemId { get; set; }
    public int? SupplierId { get; set; }

    public string TransactionType { get; set; } = string.Empty;
    public decimal Quantity { get; set; }

    public DateTime TransactionDate { get; set; }
    public string? Remarks { get; set; }
}