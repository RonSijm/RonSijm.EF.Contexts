using Microsoft.EntityFrameworkCore;
using RonSijm.EF.Markdown.Infrastructure.Internal;

#pragma warning disable EF1001 // Internal EF Core API usage

namespace RonSijm.EF.Markdown.Tests.Extensions;

public class MarkdownDbContextOptionsExtensionsTests
{
    [Fact]
    public void UseMarkdown_AddsMarkdownOptionsExtension()
    {
        // Arrange
        var optionsBuilder = new DbContextOptionsBuilder();

        // Act
        optionsBuilder.UseMarkdown("TestDirectory");

        // Assert
        var extension = optionsBuilder.Options.FindExtension<MarkdownOptionsExtension>();
        Assert.NotNull(extension);
    }

    [Fact]
    public void UseMarkdown_SetsDirectoryPath()
    {
        // Arrange
        var optionsBuilder = new DbContextOptionsBuilder();

        // Act
        optionsBuilder.UseMarkdown("MyDataDirectory");

        // Assert
        var extension = optionsBuilder.Options.FindExtension<MarkdownOptionsExtension>();
        Assert.Equal("MyDataDirectory", extension?.DirectoryPath);
    }

    [Fact]
    public void UseMarkdown_WithAction_ConfiguresOptions()
    {
        // Arrange
        var optionsBuilder = new DbContextOptionsBuilder();

        // Act
        optionsBuilder.UseMarkdown("TestDir", builder =>
        {
            builder.UseFileExtension(".txt");
            builder.CreateDirectoryIfNotExists();
        });

        // Assert
        var extension = optionsBuilder.Options.FindExtension<MarkdownOptionsExtension>();
        Assert.NotNull(extension);
        Assert.Equal(".txt", extension.FileExtension);
        Assert.True(extension.CreateDirectoryIfNotExists);
    }

    [Fact]
    public void UseMarkdown_ReturnsOptionsBuilder()
    {
        // Arrange
        var optionsBuilder = new DbContextOptionsBuilder();

        // Act
        var result = optionsBuilder.UseMarkdown("TestDir");

        // Assert
        Assert.Same(optionsBuilder, result);
    }

    [Fact]
    public void UseMarkdown_GenericVersion_Works()
    {
        // Arrange
        var optionsBuilder = new DbContextOptionsBuilder<TestDbContextForExtensions>();

        // Act
        optionsBuilder.UseMarkdown("TestDir");

        // Assert
        var extension = optionsBuilder.Options.FindExtension<MarkdownOptionsExtension>();
        Assert.NotNull(extension);
        Assert.Equal("TestDir", extension.DirectoryPath);
    }

    [Fact]
    public void UseMarkdown_CalledTwice_UsesLastConfiguration()
    {
        // Arrange
        var optionsBuilder = new DbContextOptionsBuilder();

        // Act
        optionsBuilder.UseMarkdown("FirstDir");
        optionsBuilder.UseMarkdown("SecondDir");

        // Assert
        var extension = optionsBuilder.Options.FindExtension<MarkdownOptionsExtension>();
        Assert.Equal("SecondDir", extension?.DirectoryPath);
    }
}

public class TestDbContextForExtensions : DbContext
{
    public TestDbContextForExtensions(DbContextOptions<TestDbContextForExtensions> options) : base(options) { }
}

