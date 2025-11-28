using Anyware.OrdersAPI.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anyware.OrdersAPI.Domain.Entities
{
    public class Order: BaseEntity
    {
        public string CustomerName { get; set; } = string.Empty;
        public string Product { get; set; } = string.Empty;
        public decimal Amount { get; set; }     
    }
}
