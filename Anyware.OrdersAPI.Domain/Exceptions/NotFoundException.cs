using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anyware.OrdersAPI.Domain.Exceptions
{
    public class NotFoundException : BaseCustomException
    {
        public NotFoundException(string message): base(message, StatusCodes.Status404NotFound, "NOT_FOUND")
        {}

        public NotFoundException(string resourceName, object key)
            : base($"{resourceName} with id '{key}' was not found.", StatusCodes.Status404NotFound, "NOT_FOUND")
        {}
    }

    public static class StatusCodes
    {
        public const int Status404NotFound = 404;
        public const int Status400BadRequest = 400;
        public const int Status409Conflict = 409;
        public const int Status500InternalServerError = 500;
    }
}
