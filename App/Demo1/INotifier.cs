namespace App.Demo1;

public interface INotifier
{
    public Task<string> Notify(string content);
}