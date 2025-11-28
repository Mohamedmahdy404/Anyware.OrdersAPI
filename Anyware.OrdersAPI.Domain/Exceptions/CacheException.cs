using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anyware.OrdersAPI.Domain.Exceptions
{
    public class CacheException : BaseCustomException
    {
        public CacheException(string message): base(message, StatusCodes.Status500InternalServerError, "CACHE_ERROR")
        {}

        public CacheException(string message, Exception innerException):base(message, innerException, StatusCodes.Status500InternalServerError, "CACHE_ERROR")
        {}
    }
}
