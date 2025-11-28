using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anyware.OrdersAPI.Domain.Exceptions
{
    public class ValidationException : BaseCustomException
    {
        public IDictionary< string, string[]> Errors { get; }

        public ValidationException(string message): base(message, StatusCodes.Status400BadRequest, "VALIDATION_ERROR")
        {
            Errors = new Dictionary<string, string[] >();
        }

        public ValidationException(IDictionary<string, string[]> errors): base("One or more validation errors occurred.", StatusCodes.Status400BadRequest, "VALIDATION_ERROR")
        {
            Errors = errors ;
        }
    }
}
