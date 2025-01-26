using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Common
{
    public class ApiResponse<T>
    {
        public T? Data { get; }
        public string? UserMessage { get; }

        public ApiResponse(T? data, string? userMessage)
        {
            Data = data;
            UserMessage = userMessage;
        }
    }
}
