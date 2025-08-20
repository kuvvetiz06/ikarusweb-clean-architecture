namespace IKARUSWEB.UI.Models.Api
{
    public class Result<T>
    {
        public bool Succeeded { get; init; }
        public string? Message { get; init; }
        public T? Data { get; init; }

        public static Result<T> Success(T data, string? msg = null) => new() { Succeeded = true, Data = data, Message = msg };
        public static Result<T> Failure(string msg) => new() { Succeeded = false, Message = msg };
    }
}
