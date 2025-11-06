using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPartsShop.Domain.Dtos
{
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public T? Data { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public string? ErrorCode { get; set; }

        public static ApiResponse<T> Success(T data) => new()
        {
            IsSuccess = true,
            Data = data,
            StatusCode = 200
        };

        public static ApiResponse<T> Failure(string errorMessage, int statusCode, string? errorCode = null) => new()
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            StatusCode = statusCode,
            ErrorCode = errorCode
        };
    }


}
