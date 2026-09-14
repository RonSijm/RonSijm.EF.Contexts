// Sample application demonstrating the Markdown EF Core provider

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SampleApp.Data;
using SampleApp.Models;

Console.WriteLine("=== EF Core Markdown Provider Sample ===\n");

// Setup the data directory
var dataDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Entities");
Console.WriteLine($"Data directory: {dataDirectory}\n");

// Configure services
var services = new ServiceCollection();
services.AddDbContext<SampleDbContext>(options =>
    options.UseMarkdown(dataDirectory));

var serviceProvider = services.BuildServiceProvider();

// Get the DbContext
using var scope = serviceProvider.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<SampleDbContext>();

// Ensure the database is created
Console.WriteLine("Creating database...");
context.Database.EnsureCreated();
Console.WriteLine("Database created.\n");

// Add some sample data
Console.WriteLine("Adding sample data...");

var category1 = new Category { Name = "Electronics", Description = "Electronic devices and gadgets" };
var category2 = new Category { Name = "Books", Description = "Physical and digital books" };

context.Categories.Add(category1);
context.Categories.Add(category2);

var product1 = new Product
{
    Name = "Laptop",
    Price = 999.99m,
    Quantity = 10,
    CreatedAt = DateTime.UtcNow,
    IsActive = true
};

var product2 = new Product
{
    Name = "Smartphone",
    Price = 699.99m,
    Quantity = 25,
    CreatedAt = DateTime.UtcNow,
    IsActive = true
};

var product3 = new Product
{
    Name = "C# Programming Book",
    Price = 49.99m,
    Quantity = 100,
    CreatedAt = DateTime.UtcNow,
    IsActive = true
};

context.Products.Add(product1);
context.Products.Add(product2);
context.Products.Add(product3);

context.SaveChanges();
Console.WriteLine("Sample data added.\n");

// Query the data
Console.WriteLine("Querying products...");
var products = context.Products.ToList();
Console.WriteLine($"Found {products.Count} products:");
foreach (var product in products)
{
    Console.WriteLine($"  - {product.Name}: ${product.Price} (Qty: {product.Quantity})");
}

Console.WriteLine("\nQuerying categories...");
var categories = context.Categories.ToList();
Console.WriteLine($"Found {categories.Count} categories:");
foreach (var category in categories)
{
    Console.WriteLine($"  - {category.Name}: {category.Description}");
}

// Show the generated markdown files
Console.WriteLine("\n=== Generated Markdown Files ===\n");

if (Directory.Exists(dataDirectory))
{
    foreach (var file in Directory.GetFiles(dataDirectory, "*.md"))
    {
        Console.WriteLine($"--- {Path.GetFileName(file)} ---");
        Console.WriteLine(File.ReadAllText(file));
        Console.WriteLine();
    }
}

Console.WriteLine("=== Sample Complete ===");
