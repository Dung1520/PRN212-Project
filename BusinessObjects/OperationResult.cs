using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects
{
    public class OperationResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;

        public static OperationResult Success(string message)
            => new OperationResult { IsSuccess = true, Message = message };

        public static OperationResult Failure(string message)
            => new OperationResult { IsSuccess = false, Message = message };
    }
}
