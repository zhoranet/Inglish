namespace Inglish
{
    public interface IParser
    {
        TokenCommand DoCommand(string text);
    }
}