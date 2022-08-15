namespace WebApplication1;

public record Input1
{
    public long Id { get; set; }
    public string? Info { get; set; }
}

public record Input2
{
    public long Id { get; set; }
    public string? Info { get; set; }
}

public record Output1
{
    public string? Info { get; set; }
}

public record Output2
{
    public string? Info { get; set; }
}