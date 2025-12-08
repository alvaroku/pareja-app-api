namespace ParejaAppAPI.Models.Responses;

public class Response<T>
{
    public bool IsSuccess { get; set; }
    public int StatusCode { get; set; }
    public T? Data { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string>? Errors { get; set; }

    public static Response<T> Success(T data, int statusCode = 200, string message = "Operación exitosa")
    {
        return new Response<T>
        {
            IsSuccess = true,
            StatusCode = statusCode,
            Data = data,
            Message = message
        };
    }

    public static Response<T> Failure(int statusCode, string message, params string[] errors)
    {
        return new Response<T>
        {
            IsSuccess = false,
            StatusCode = statusCode,
            Message = message,
            Errors = errors?.Length > 0 ? errors.ToList() : null
        };
    }
}
