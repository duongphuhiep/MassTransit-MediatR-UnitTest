
using Microsoft.EntityFrameworkCore;

namespace WebApplication1;
/**
dotnet user-secrets set ConnectionStrings_MassTransit_Mediator_UnitTest "Data Source=CHATAIGNE\\SQLEXPRESS;Initial Catalog=MassTransit_Mediator_UnitTest; User Id=***; Password=***;" --id ConnectionStrings_MassTransit_Mediator_UnitTest
dotnet ef dbcontext scaffold Name=ConnectionStrings_MassTransit_Mediator_UnitTests Microsoft.EntityFrameworkCore.SqlServer
dotnet ef migrations add InitialCreate
dotnet ef database update
 */
public class Context : DbContext
{
    public readonly Guid InstanceId = Guid.NewGuid();
    public DbSet<Input1History>? Input1s { get; set; }
    public DbSet<Input2History>? Input2s { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Data Source=CHATAIGNE\\SQLEXPRESS;Initial Catalog=MassTransit_Mediator_UnitTest; User Id=***; Password=***");
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
