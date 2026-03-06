namespace ProductComparisonApi.Domain.Models
{

    public class Response<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public static Response<T> Ok(T data, string message = "OK") =>
            new() { Success = true, Message = message, Data = data };

        public static Response<T> Fail(string message) =>
            new() { Success = false, Message = message };

        public static Response<object> Empty(string message) =>
            new() { Success = true, Message = message, Data = null };
    }
}