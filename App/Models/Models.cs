namespace App.Models;

public record Input1
{
    public Input1()
    {
        
    }
    public Input1(long id, string info)
    {
        Id = id;
        Info = info;
    }

    public long Id { get; set; }
    public string Info { get; set; }
}

public record Input2
{
    public Input2()
    {
            
    }
    public Input2(long id, string info)
    {
        Id = id;
        Info = info;
    }

    public long Id { get; set; }
    public string Info { get; set; }
}

public record Output1
{
    public Output1()
    {
            
    }
    public Output1(string info)
    {
        Info = info;
    }

    public string Info { get; set; }
}

public record Output2
{
    public Output2()
    {
        
    }
    public Output2(string info)
    {
        Info = info;
    }

    public string Info { get; set; }
}