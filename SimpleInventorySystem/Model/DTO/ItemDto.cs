namespace SimpleInventorySystem.Model.DTO;

public class ItemDto
{
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public bool IsActive { get; set; } = true;
}