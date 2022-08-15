
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using ToolsPack.String;

namespace WebApplication1;
/**
dotnet user-secrets set ConnectionStrings_MassTransit_Mediator_UnitTest "Data Source=CHATAIGNE\\SQLEXPRESS;Initial Catalog=MassTransit_Mediator_UnitTest; User Id=***; Password=***;" --id ConnectionStrings_MassTransit_Mediator_UnitTest
dotnet ef dbcontext scaffold Name=ConnectionStrings_MassTransit_Mediator_UnitTests Microsoft.EntityFrameworkCore.SqlServer
dotnet ef migrations add InitialCreate
dotnet ef database update
 */
public class Context : DbContext
{
    private readonly DbConnection? _connection;
    public Context(DbConnection connection)
    {
        _connection = connection;
    }

    public Context(DbContextOptions<Context> options) : base(options)
    {
    }

    public DbSet<Input1History>? Input1s { get; set; }
    public DbSet<Input2History>? Input2s { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured && _connection is not null)
        {
            optionsBuilder.UseSqlServer(_connection);
        }
    }
}

public record Input1History
{
    public long Id { get; set; }
    public string? Input { get; set; }
    public string? Output { get; set; }
}
public record Input2History
{
    public long Id { get; set; }
    public string? Input { get; set; }
    public string? Output { get; set; }
}


public interface IScopedSample1
{
    string GetId();
}

public class ScopedSample1 : IScopedSample1
{
    public readonly string Id = "1" + StringGenerator.CreateRandomString(4, 0, "abcdefghijklmnopqrstuvwxyz");
    public string GetId() => Id;
}

public interface IScopedSample21
{
    string GetId();
}

public class ScopedSample21 : IScopedSample21
{
    public readonly string Id = "21" + StringGenerator.CreateRandomString(4, 0, "abcdefghijklmnopqrstuvwxyz");
    public readonly IScopedSample1 _sample1;

    public ScopedSample21(IScopedSample1 sample1)
    {
        _sample1 = sample1;
    }

    public string GetId() => _sample1.GetId() + "." + Id;
}

public interface IScopedSample22
{
    string GetId();
}

public class ScopedSample22 : IScopedSample22
{
    public readonly string Id = "22" + StringGenerator.CreateRandomString(4, 0, "abcdefghijklmnopqrstuvwxyz");
    public readonly IScopedSample1 _sample1;

    public ScopedSample22(IScopedSample1 sample1)
    {
        _sample1 = sample1;
    }

    public string GetId() => _sample1.GetId() + "." + Id;
}