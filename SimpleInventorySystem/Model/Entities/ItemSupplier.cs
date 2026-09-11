namespace SimpleInventorySystem.Model.Entities;

public class ItemSupplier
{
    public int ItemSupplierId { get; set; }

    public int ItemId { get; set; }
    public int SupplierId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Item? Item { get; set; }
    public Supplier? Supplier { get; set; }
}