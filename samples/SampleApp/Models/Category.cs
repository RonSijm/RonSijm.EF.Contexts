// Sample entity model

namespace SampleApp.Models;

/// <summary>
/// Represents a category entity.
/// </summary>
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

