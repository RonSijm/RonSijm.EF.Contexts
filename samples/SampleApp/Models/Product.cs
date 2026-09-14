// Sample entity model

namespace SampleApp.Models;

/// <summary>
/// Represents a product entity.
/// </summary>
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}

