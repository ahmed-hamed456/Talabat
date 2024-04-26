using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talabat.Core.Entities.Order_Aggregate
{
    public class Order : BaseEntitiy
    {
        public Order()
        {

        }
        public Order(string bayerEmail, Address shappingAddress, DeliveryMethod deliveryMethod, ICollection<OrderItem> items, decimal subtotal,string? paymentIntentId)
        {
            BayerEmail = bayerEmail;
            ShappingAddress = shappingAddress;
            DeliveryMethod = deliveryMethod;
            Items = items;
            Subtotal = subtotal;
            PaymentIntentId = paymentIntentId;
        }

        public string BayerEmail { get; set; }
        public DateTimeOffset OrderDate { get; set; } = DateTimeOffset.UtcNow;
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public Address ShappingAddress { get; set; }
        public DeliveryMethod DeliveryMethod { get; set; } // Navigational Property [ONE]

        public ICollection<OrderItem> Items { get; set; } = new HashSet<OrderItem>(); // Navigational Property [MANY]

        public decimal Subtotal { get; set; }

        public decimal GetTotal()
            => Subtotal + DeliveryMethod.Cost;

        public string PaymentIntentId { get; set; } 
    }
}
