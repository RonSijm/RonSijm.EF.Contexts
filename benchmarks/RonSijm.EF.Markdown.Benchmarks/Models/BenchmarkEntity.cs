// Licensed under the MIT license.

namespace RonSijm.EF.Markdown.Benchmarks.Models;

/// <summary>
/// Entity used for benchmarking.
/// </summary>
public class BenchmarkEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public Guid ExternalId { get; set; }
}

