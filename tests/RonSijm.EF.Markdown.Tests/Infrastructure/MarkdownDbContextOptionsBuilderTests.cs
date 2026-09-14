using Microsoft.EntityFrameworkCore;
using RonSijm.EF.Markdown.Infrastructure;
using RonSijm.EF.Markdown.Infrastructure.Internal;

#pragma warning disable EF1001 // Internal EF Core API usage

namespace RonSijm.EF.Markdown.Tests.Infrastructure;

public class MarkdownDbContextOptionsBuilderTests
{
    [Fact]
    public void UseFileExtension_SetsFileExtension()
    {
        // Arrange
        var optionsBuilder = new DbContextOptionsBuilder();
        
        // Act
        optionsBuilder.UseMarkdown("TestDir", builder => builder.UseFileExtension(".markdown"));
        
        // Assert
        var extension = optionsBuilder.Options.FindExtension<MarkdownOptionsExtension>();
        Assert.NotNull(extension);
        Assert.Equal(".markdown", extension.FileExtension);
    }

    [Fact]
    public void CreateDirectoryIfNotExists_SetsOption()
    {
        // Arrange
        var optionsBuilder = new DbContextOptionsBuilder();
        
        // Act
        optionsBuilder.UseMarkdown("TestDir", builder => builder.CreateDirectoryIfNotExists());
        
        // Assert
        var extension = optionsBuilder.Options.FindExtension<MarkdownOptionsExtension>();
        Assert.NotNull(extension);
        Assert.True(extension.CreateDirectoryIfNotExists);
    }

    [Fact]
    public void ChainedConfiguration_AppliesAllOptions()
    {
        // Arrange
        var optionsBuilder = new DbContextOptionsBuilder();
        
        // Act
        optionsBuilder.UseMarkdown("TestDir", builder => 
            builder
                .UseFileExtension(".txt")
                .CreateDirectoryIfNotExists());
        
        // Assert
        var extension = optionsBuilder.Options.FindExtension<MarkdownOptionsExtension>();
        Assert.NotNull(extension);
        Assert.Equal(".txt", extension.FileExtension);
        Assert.True(extension.CreateDirectoryIfNotExists);
    }

    [Fact]
    public void UseMarkdown_WithoutBuilder_UsesDefaults()
    {
        // Arrange
        var optionsBuilder = new DbContextOptionsBuilder();

        // Act
        optionsBuilder.UseMarkdown("TestDir");

        // Assert
        var extension = optionsBuilder.Options.FindExtension<MarkdownOptionsExtension>();
        Assert.NotNull(extension);
        Assert.Equal("TestDir", extension.DirectoryPath);
        Assert.Equal(".md", extension.FileExtension);
        Assert.True(extension.CreateDirectoryIfNotExists); // Default is true
    }
}

