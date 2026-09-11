namespace SimpleInventorySystem.Model.Entities;

public class Customer
{
    public int CustomerId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Address { get; set; }
}