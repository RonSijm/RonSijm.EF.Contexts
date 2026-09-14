using System.Linq.Expressions;
using System.Text;
using Microsoft.EntityFrameworkCore.Query;

namespace RonSijm.EF.Markdown.Tests.Query;

/// <summary>
/// A simple test implementation of ExpressionPrinter for testing IPrintableExpression.Print methods.
/// </summary>
public class TestExpressionPrinter : ExpressionPrinter
{
    private readonly StringBuilder _stringBuilder = new();

    public TestExpressionPrinter() : base()
    {
    }

    public string Output => _stringBuilder.ToString();
}

