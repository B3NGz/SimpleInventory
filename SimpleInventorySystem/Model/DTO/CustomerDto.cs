namespace SimpleInventorySystem.Model.DTO
{
    public class CustomerDto
    {
        public int CustomerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
    }
}