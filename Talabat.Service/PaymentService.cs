using Microsoft.Extensions.Configuration;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core;
using Talabat.Core.Entities;
using Talabat.Core.Entities.Order_Aggregate;
using Talabat.Core.Repository.Contract;
using Talabat.Core.Services.Contract;
using Talabat.Core.Specifications.Order_Specs;
using Product = Talabat.Core.Entities.Product;

namespace Talabat.Service
{
    public class PaymentService : IPaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly IBasketRepository _basketRepo;
        private readonly IUnitOfWork _unitOfWork;

        public PaymentService(
            IConfiguration configuration,
            IBasketRepository basketRepo,
            IUnitOfWork unitOfWork
            )
        {
            _configuration = configuration;
            _basketRepo = basketRepo;
            _unitOfWork = unitOfWork;
        }
        public async Task<CustomerBasket?> CreateOrUpdatePaymentIntent(string basketId)
        {
            StripeConfiguration.ApiKey = _configuration["StripeSettings:Secretkey"];

            var basket = await _basketRepo.GetBasketAsync(basketId);

            if(basket is null) return null;

            var shappingPrice = 0m;

            if(basket.DeliveryMethodId.HasValue)
            {
                var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>().GetByIdAsync(basket.DeliveryMethodId.Value);
                basket.ShappingPrice = deliveryMethod.Cost;
                shappingPrice = deliveryMethod.Cost;
            }

            if(basket?.Items.Count > 0)
            {
                foreach( var item in basket.Items)
                {
                    var product = await _unitOfWork.Repository<Product>().GetByIdAsync(item.Id);
                    if(item.Price != product.Price)
                        item.Price = product.Price;
                }
            }

            PaymentIntentService paymentIntentService = new PaymentIntentService();
            PaymentIntent paymentIntent;

            if(string.IsNullOrEmpty(basket.PaymentIntentId)) //Create New Payment Intent
            {
                var createOptions = new PaymentIntentCreateOptions()
                {
                    Amount = (long) basket.Items.Sum(item => item.Price * 100 * item.Quantity) + (long)shappingPrice * 100,
                    Currency = "usd",
                    PaymentMethodTypes = new List<string>() {"card"}
                };

                paymentIntent =await paymentIntentService.CreateAsync(createOptions); //Integrate with stripe

                basket.PaymentIntentId = paymentIntent.Id;
                basket.ClientSecret= paymentIntent.ClientSecret;
            }
            else // Update Existing Payment Intent
            {
                var updateOptions = new PaymentIntentUpdateOptions()
                {
                    Amount = (long)basket.Items.Sum(item => item.Price * 100 * item.Quantity) + (long)shappingPrice * 100,
                };

                await paymentIntentService.UpdateAsync(basket.PaymentIntentId, updateOptions);
            }
            await _basketRepo.UpdateBasketAsync(basket);

            return basket;
        }

        public async Task<Order> UpdatePaymentIntentToSucceededOrFailed(string paymentIntent, bool isSucceeded)
        {
            var spec = new OrderWithPaymentIntentSpecifications(paymentIntent);
            
            var order = await _unitOfWork.Repository<Order>().GetEntityWithSpecAsync(spec);

            if (isSucceeded)
                order.Status = OrderStatus.PaymentReceived;
            else
                order.Status = OrderStatus.PaymentFailed;

            _unitOfWork.Repository<Order>().Update(order);

            await _unitOfWork.CompeleteAsync();

            return order;
        }
    }
}
