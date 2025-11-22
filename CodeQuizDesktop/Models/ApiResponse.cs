using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuizDesktop.Models
{
    public class ApiResponse<T>
    {
        public required bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
    }
}
