using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IKARUSWEB.Application.Common.Results
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
