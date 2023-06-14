namespace OCore.Services;

[Service("StringFun")]
public interface IStringFun : IGrainWithStringKey
{
    Task<string> ReverseString(string input);

    Task<string> Capitalize(string input);
}

public class StringFun : Grain, IStringFun
{
    private int callCount = 0;
    
    static string Reverse(string s)
    {
        char[] charArray = s.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }
    
    public Task<string> ReverseString(string input)
    {
        callCount++;
        return Task.FromResult(Reverse(input));
    }

    public Task<string> Capitalize(string input)
    {
        callCount++;
        return Task.FromResult(input.ToUpper());
    }
}