using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anyware.OrdersAPI.Domain.Exceptions
{
    public abstract class BaseCustomException : Exception
    {
        public int StatusCode { get; }
        public string ErrorCode { get; }

        protected BaseCustomException(string message, int statusCode, string errorCode)
            : base(message)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
        }

        protected BaseCustomException(string message, Exception innerException, int statusCode, string errorCode)
            : base(message, innerException)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
        }
    }
}
