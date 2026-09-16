# RonSijm.EF.Contexts

File-backed database providers for Entity Framework Core.

This repository contains providers that persist EF Core entities as human-readable Markdown, JSON, or CSV files. They are intended for small datasets, samples, local tools, generated content, and scenarios where editable files are more useful than a relational database.

## Why use a file-backed provider?

Using Markdown or another text format as a database may seem unusual, but it can be useful when the persisted output is part of what you want to test or inspect.

One example is snapshot testing. A test can run against the Markdown provider, save its entities, and compare the generated `.md` files with approved snapshots using a library such as [Verify](https://github.com/VerifyTests/Verify). This makes changes to the persisted data visible in the test diff without requiring database queries or custom diagnostic output.

The files are also easy to inspect manually. After a test, sample, or local tool has run, you can open the generated Markdown and immediately see:

- Which entities were written.
- Which values were stored.
- How updates and deletions changed the data.
- Whether relationships contain the expected key values.

This makes file-backed providers useful for integration tests, reproducible examples, debugging, and reviewing generated datasets. If an application normally uses another database provider, its entities can also be copied into a context configured with the Markdown provider to produce a readable snapshot.

## Providers

| Package | Storage format | Default layout |
|---|---|---|
| `RonSijm.EF.Markdown` | Markdown tables | One `.md` file per entity type |
| `RonSijm.EF.Json` | JSON | One `.json` file per entity type, or one file for the complete context |
| `RonSijm.EF.Csv` | CSV | One `.csv` file per entity type |
| `RonSijm.EF.FileStore.Core` | Shared infrastructure | Referenced by the format-specific providers |

All projects target .NET 9 and use Entity Framework Core 9.

## Features

- Standard EF Core change tracking.
  - Add, update, and delete entities through `DbSet<TEntity>` and `SaveChanges`.
- Synchronous and asynchronous persistence.
  - `SaveChangesAsync`, `EnsureCreatedAsync`, and `EnsureDeletedAsync` are available.
- File-based database lifecycle.
  - `EnsureCreated` creates the storage directory and entity files.
  - `EnsureDeleted` removes the configured storage directory.
- Generated keys.
  - Integer, long, and GUID keys are supported.
- Common scalar values.
  - Includes strings, numeric types, booleans, enums, GUIDs, `DateTime`, `DateTimeOffset`, and nullable values.
- Optional foreign-key validation.
  - Insert, update, and delete operations can validate referential integrity.
- Configurable output.
  - File extensions and directory creation can be configured for every provider.
  - JSON additionally supports single-file storage, indentation, property naming policies, and reference modes.

## Installation

Install the provider for the format you want:

```powershell
dotnet add package RonSijm.EF.Markdown
dotnet add package RonSijm.EF.Json
dotnet add package RonSijm.EF.Csv
```

When working directly from this repository, reference the corresponding project under `src`.

## Quick start

Define a normal EF Core model:

```csharp
using Microsoft.EntityFrameworkCore;

public sealed class ProductDbContext(DbContextOptions<ProductDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
}

public sealed class Product
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
}
```

Configure one of the file providers:

```csharp
var dataDirectory = Path.Combine(AppContext.BaseDirectory, "Data");

var options = new DbContextOptionsBuilder<ProductDbContext>()
    .UseMarkdown(dataDirectory)
    .Options;

await using var context = new ProductDbContext(options);
await context.Database.EnsureCreatedAsync();

context.Products.Add(new Product
{
    Name = "Mechanical keyboard",
    Price = 129.95m,
    IsActive = true
});

await context.SaveChangesAsync();

var products = await context.Products.ToListAsync();
var activeProducts = products.Where(product => product.IsActive).ToList();
```

The Markdown provider writes a file similar to:

```markdown
# Product

| Id | IsActive | Name | Price |
|---|---|---|---|
| 1 | true | Mechanical keyboard | 129.95 |
```

## Provider configuration

### Markdown

```csharp
options.UseMarkdown(dataDirectory, markdown =>
{
    markdown.UseFileExtension(".md");
    markdown.CreateDirectoryIfNotExists();
    markdown.EnforceForeignKeys();
});
```

Defaults:

| Option | Default |
|---|---|
| File extension | `.md` |
| Create missing directory | `true` |
| Enforce foreign keys | `false` |

Markdown files use pipe-delimited table rows, so string values should not contain unescaped `|` characters.

### CSV

```csharp
options.UseCsv(dataDirectory, csv =>
{
    csv.UseFileExtension(".csv");
    csv.CreateDirectoryIfNotExists();
    csv.EnforceForeignKeys();
});
```

Defaults:

| Option | Default |
|---|---|
| File extension | `.csv` |
| Create missing directory | `true` |
| Enforce foreign keys | `false` |

Fields containing commas or quotes are CSV-escaped.

### JSON

```csharp
using System.Text.Json;
using RonSijm.EF.Json.Serialization;

options.UseJson(dataDirectory, json =>
{
    json.UseMultipleFiles();
    json.UseIndentation();
    json.UsePropertyNaming(JsonNamingPolicy.CamelCase);
    json.UseReferenceMode(JsonReferenceMode.Flat);
    json.EnforceForeignKeys();
});
```

Defaults:

| Option | Default |
|---|---|
| Storage mode | `JsonStorageMode.MultipleFiles` |
| File extension | `.json` |
| Indented output | `true` |
| Property naming | Model property names |
| Reference mode | `JsonReferenceMode.Flat` |
| Create missing directory | `true` |
| Enforce foreign keys | `false` |

To store every entity type in one JSON document:

```csharp
options.UseJson(dataDirectory, json =>
{
    json.UseSingleFile("database.json");
});
```

JSON reference modes:

| Mode | Representation |
|---|---|
| `Flat` | Stores the foreign-key scalar value |
| `JsonReference` | Stores a JSON Reference object using `$ref` |
| `Inline` | Stores an object containing the referenced primary key |

## Dependency injection

The providers integrate with `AddDbContext`:

```csharp
services.AddDbContext<ProductDbContext>(options =>
    options.UseMarkdown(Path.Combine(AppContext.BaseDirectory, "Data")));
```

Equivalent `UseJson` and `UseCsv` extension methods are available.

## Foreign keys

Foreign-key validation is disabled by default. Enable it when file data must preserve referential integrity:

```csharp
options.UseMarkdown(dataDirectory, markdown =>
    markdown.EnforceForeignKeys());
```

When enabled, the provider validates:

- Inserts and updates.
  - Non-null foreign keys must reference an existing principal entity.
- Deletes.
  - A principal entity cannot be deleted while dependent rows still reference it.
- Optional relationships.
  - A nullable foreign key may remain `null`.

Violations throw `ForeignKeyValidationException`.

## Limitations

- Server-side LINQ translation is not implemented.
  - Materialize a set with `ToList` or `ToListAsync`, then apply `Where`, `OrderBy`, `Select`, `FirstOrDefault`, `Count`, and similar operators in memory.
- Transactions do not provide atomicity or rollback.
  - Transaction APIs return a no-op transaction for EF Core compatibility.
- EF Core migrations are not supported.
  - Use `EnsureCreated` and `EnsureDeleted` to manage file storage.
- Storage is rewritten on save.
  - These providers are best suited to small datasets and low-write workloads.
- Some format-specific string values are restricted.
  - Markdown strings should not contain `|`, and CSV strings should not contain embedded line breaks.
- Automated coverage is currently concentrated on Markdown.
  - JSON and CSV share the core architecture but have less repository-level test coverage.

Do not use these providers where you need relational query execution, transactional guarantees, high write concurrency, or large-scale data storage.

## Repository structure

| Path | Purpose |
|---|---|
| `src/RonSijm.EF.FileStore.Core` | Shared EF Core storage, query, key generation, and foreign-key infrastructure |
| `src/RonSijm.EF.Markdown` | Markdown provider |
| `src/RonSijm.EF.Json` | JSON provider |
| `src/RonSijm.EF.Csv` | CSV provider |
| `tests/RonSijm.EF.Markdown.Tests` | Markdown provider tests |
| `samples/SampleApp` | Runnable Markdown example |
| `benchmarks/RonSijm.EF.Markdown.Benchmarks` | Markdown, CSV, SQLite, and in-memory comparisons |

## Build and test

```powershell
dotnet restore .\RonSijm.EF.Contexts.slnx
dotnet build .\RonSijm.EF.Contexts.slnx --configuration Release
dotnet test .\tests\RonSijm.EF.Markdown.Tests\RonSijm.EF.Markdown.Tests.csproj --configuration Release
```

Run the sample:

```powershell
dotnet run --project .\samples\SampleApp\SampleApp.csproj
```
