namespace ShopApi.Exceptions;

public class AppException : Exception
{
    public int StatusCode { get; }

    public string ErrCode { get; }

    public AppException(
        string message,
        int statusCode,
        string errCode
    ) : base(message)
    {
        StatusCode = statusCode;
        ErrCode = errCode;
    }
}