namespace UrlShort.Application.Dto;

public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }

    public T? Data { get; set; }

    public int StatusCode { get; set; }

    public List<string?> Errors { get; set; }

    public ApiResponse()
    {
        IsSuccess = true;
        Errors = new List<string?>();
    }    
 
    public ApiResponse(T data, int statusCode) : this()
    {
        IsSuccess = true;
        Data = data;
        StatusCode = statusCode;
    }

    public ApiResponse(List<string?> errors, int statusCode) : this()
    {
        IsSuccess = false;
        Errors = errors;
        StatusCode = statusCode;
    }

    public ApiResponse(bool isSuccess, int statusCode)
    {
        IsSuccess = isSuccess;
        StatusCode = statusCode;
        Errors = new List<string?>();
    }
}