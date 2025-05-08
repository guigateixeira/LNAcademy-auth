namespace LNAcademy.AuthService.Errors;

public class AuthError : Exception
{
    public int StatusCode { get; }

    public AuthError(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }
}