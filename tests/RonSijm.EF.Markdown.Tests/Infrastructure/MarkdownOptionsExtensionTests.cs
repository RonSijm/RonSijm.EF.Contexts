using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using RonSijm.EF.Markdown.Infrastructure.Internal;

#pragma warning disable EF1001 // Internal EF Core API usage

namespace RonSijm.EF.Markdown.Tests.Infrastructure;

public class MarkdownOptionsExtensionTests
{
    [Fact]
    public void Constructor_SetsDefaultValues()
    {
        // Arrange & Act
        var extension = new MarkdownOptionsExtension();

        // Assert
        Assert.Equal(string.Empty, extension.DirectoryPath);
        Assert.Equal(".md", extension.FileExtension);
        Assert.True(extension.CreateDirectoryIfNotExists);
    }

    [Fact]
    public void WithDirectoryPath_SetsDirectoryPath()
    {
        // Arrange
        var original = new MarkdownOptionsExtension();

        // Act
        var cloned = original.WithDirectoryPath("TestDirectory");

        // Assert
        Assert.Equal("TestDirectory", cloned.DirectoryPath);
    }

    [Fact]
    public void WithDirectoryPath_ReturnsNewInstance()
    {
        // Arrange
        var original = new MarkdownOptionsExtension().WithDirectoryPath("Original");

        // Act
        var cloned = original.WithDirectoryPath("NewPath");

        // Assert
        Assert.NotSame(original, cloned);
        Assert.Equal("NewPath", cloned.DirectoryPath);
        Assert.Equal("Original", original.DirectoryPath);
    }

    [Fact]
    public void WithFileExtension_ReturnsNewInstance()
    {
        // Arrange
        var original = new MarkdownOptionsExtension();

        // Act
        var cloned = original.WithFileExtension(".markdown");

        // Assert
        Assert.NotSame(original, cloned);
        Assert.Equal(".markdown", cloned.FileExtension);
        Assert.Equal(".md", original.FileExtension);
    }

    [Fact]
    public void WithCreateDirectoryIfNotExists_ReturnsNewInstance()
    {
        // Arrange
        var original = new MarkdownOptionsExtension();

        // Act
        var cloned = original.WithCreateDirectoryIfNotExists(false);

        // Assert
        Assert.NotSame(original, cloned);
        Assert.False(cloned.CreateDirectoryIfNotExists);
        Assert.True(original.CreateDirectoryIfNotExists);
    }

    [Fact]
    public void Info_ReturnsExtensionInfo()
    {
        // Arrange
        var extension = new MarkdownOptionsExtension().WithDirectoryPath("TestDirectory");

        // Act
        var info = extension.Info;

        // Assert
        Assert.NotNull(info);
    }

    [Fact]
    public void Info_LogFragment_ContainsDirectoryPath()
    {
        // Arrange
        var extension = new MarkdownOptionsExtension().WithDirectoryPath("TestDirectory");

        // Act
        var logFragment = extension.Info.LogFragment;

        // Assert
        Assert.Contains("TestDirectory", logFragment);
    }

    [Fact]
    public void ApplyServices_AddsMarkdownServices()
    {
        // Arrange
        var options = new DbContextOptionsBuilder()
            .UseMarkdown("TestDirectory")
            .Options;

        // Act & Assert - Should not throw
        using var context = new TestDbContextForOptions(options);
    }

    [Fact]
    public void Validate_DoesNotThrow_ForValidOptions()
    {
        // Arrange
        var extension = new MarkdownOptionsExtension().WithDirectoryPath("TestDirectory");
        var options = new DbContextOptionsBuilder()
            .UseMarkdown("TestDirectory")
            .Options;

        // Act & Assert - Should not throw
        extension.Validate(options);
    }

    [Fact]
    public void Validate_Throws_WhenDirectoryPathEmpty()
    {
        // Arrange
        var extension = new MarkdownOptionsExtension();
        var options = new DbContextOptionsBuilder().Options;

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => extension.Validate(options));
    }

    [Fact]
    public void Info_IsDatabaseProvider_ReturnsTrue()
    {
        // Arrange
        var extension = new MarkdownOptionsExtension().WithDirectoryPath("TestDirectory");

        // Act & Assert
        Assert.True(extension.Info.IsDatabaseProvider);
    }

    [Fact]
    public void Info_GetServiceProviderHashCode_ReturnsSameHashForSameConfig()
    {
        // Arrange
        var extension1 = new MarkdownOptionsExtension()
            .WithDirectoryPath("TestDirectory")
            .WithFileExtension(".md");
        var extension2 = new MarkdownOptionsExtension()
            .WithDirectoryPath("TestDirectory")
            .WithFileExtension(".md");

        // Act
        var hash1 = extension1.Info.GetServiceProviderHashCode();
        var hash2 = extension2.Info.GetServiceProviderHashCode();

        // Assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void Info_GetServiceProviderHashCode_ReturnsDifferentHashForDifferentConfig()
    {
        // Arrange
        var extension1 = new MarkdownOptionsExtension()
            .WithDirectoryPath("Directory1")
            .WithFileExtension(".md");
        var extension2 = new MarkdownOptionsExtension()
            .WithDirectoryPath("Directory2")
            .WithFileExtension(".md");

        // Act
        var hash1 = extension1.Info.GetServiceProviderHashCode();
        var hash2 = extension2.Info.GetServiceProviderHashCode();

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void Info_ShouldUseSameServiceProvider_ReturnsTrueForSameConfig()
    {
        // Arrange
        var extension1 = new MarkdownOptionsExtension()
            .WithDirectoryPath("TestDirectory")
            .WithFileExtension(".md");
        var extension2 = new MarkdownOptionsExtension()
            .WithDirectoryPath("TestDirectory")
            .WithFileExtension(".md");

        // Act
        var result = extension1.Info.ShouldUseSameServiceProvider(extension2.Info);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Info_ShouldUseSameServiceProvider_ReturnsFalseForDifferentConfig()
    {
        // Arrange
        var extension1 = new MarkdownOptionsExtension()
            .WithDirectoryPath("Directory1")
            .WithFileExtension(".md");
        var extension2 = new MarkdownOptionsExtension()
            .WithDirectoryPath("Directory2")
            .WithFileExtension(".md");

        // Act
        var result = extension1.Info.ShouldUseSameServiceProvider(extension2.Info);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Info_PopulateDebugInfo_AddsExpectedKeys()
    {
        // Arrange
        var extension = new MarkdownOptionsExtension()
            .WithDirectoryPath("TestDirectory")
            .WithFileExtension(".markdown");
        var debugInfo = new Dictionary<string, string>();

        // Act
        extension.Info.PopulateDebugInfo(debugInfo);

        // Assert
        Assert.True(debugInfo.ContainsKey("Markdown:DirectoryPath"));
        Assert.True(debugInfo.ContainsKey("Markdown:FileExtension"));
        Assert.Equal("TestDirectory", debugInfo["Markdown:DirectoryPath"]);
        Assert.Equal(".markdown", debugInfo["Markdown:FileExtension"]);
    }

    [Fact]
    public void Info_LogFragment_IsCached()
    {
        // Arrange
        var extension = new MarkdownOptionsExtension().WithDirectoryPath("TestDirectory");

        // Act
        var logFragment1 = extension.Info.LogFragment;
        var logFragment2 = extension.Info.LogFragment;

        // Assert
        Assert.Same(logFragment1, logFragment2);
    }
}

#pragma warning restore EF1001

public class TestDbContextForOptions : DbContext
{
    public TestDbContextForOptions(DbContextOptions options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SimpleEntity>().HasKey(e => e.Id);
    }
}

public class SimpleEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

