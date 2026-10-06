using System.Text.Json.Serialization;

namespace ShopApi.Models.Responses;

public class ApiResponse<T>
{
    public T? Data { get; set; }

    public string? Message { get; set; }

    [JsonIgnore(
        Condition = JsonIgnoreCondition.WhenWritingNull
    )]
    public string? ErrCode { get; set; }

    public int StatusCode { get; set; }

    [JsonIgnore(
        Condition = JsonIgnoreCondition.WhenWritingNull
    )]
    public PaginationMeta? Pagination { get; set; }

    public static ApiResponse<T> Success(
        T data,
        int statusCode = 200,
        string? message = null,
        PaginationMeta? pagination = null
    )
    {
        return new ApiResponse<T>
        {
            Data = data,
            Message = message,
            ErrCode = null,
            StatusCode = statusCode,
            Pagination = pagination
        };
    }

    public static ApiResponse<T> Error(
        int statusCode,
        string message,
        string errCode
    )
    {
        return new ApiResponse<T>
        {
            Data = default,
            Message = message,
            ErrCode = errCode,
            StatusCode = statusCode,
            Pagination = null
        };
    }
}