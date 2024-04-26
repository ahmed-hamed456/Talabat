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

namespace Talabat.Service
{
    public class OrderService : IOrderService
    {
        private readonly IBasketRepository _basketRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentService _paymentService;

        public OrderService(
            IBasketRepository basketRepo,
            IUnitOfWork unitOfWork,
            IPaymentService paymentService
            )
        {
            _basketRepo = basketRepo;
            _unitOfWork = unitOfWork;
            _paymentService = paymentService;
        }
        public async Task<Order?> CreateOrderAsync(string buyerEmail, string basketId, int deliveryMethodId, Address shappingAddress)
        {
            // 1. Get basket from baskets Repo

            var basket = await _basketRepo.GetBasketAsync(basketId);

            // 2. Get selected Items at basket from product Repo
            var orderItems = new List<OrderItem>();

            if(basket?.Items?.Count() > 0)
            {
                var productRepository = _unitOfWork.Repository<Product>();
                foreach (var item in basket.Items)
                {
                    var product = await productRepository.GetByIdAsync(item.Id);

                    var productItemOrdered = new ProductItemOrdered(item.Id, product.Name, product.PictureUrl);

                    var orderItem = new OrderItem(productItemOrdered, product.Price, item.Quantity);

                    orderItems.Add(orderItem);
                }
            }

            // 3. Calculate Subtotal
            var subtotal = orderItems.Sum(orderItem => orderItem.Price * orderItem.Quantity);

            // 4. Get deliveryMethod from deliveryMethod Repo
            var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>().GetByIdAsync(deliveryMethodId);

            var orderRepo = _unitOfWork.Repository<Order>();

            var orderspecs = new OrderWithPaymentIntentSpecifications(basket.PaymentIntentId);

            var existingOrder = await orderRepo.GetEntityWithSpecAsync(orderspecs);

            if (existingOrder != null)
            {
                orderRepo.Delete(existingOrder);
                await _paymentService.CreateOrUpdatePaymentIntent(basketId);
            }
            // 5. Create Order

            var order = new Order(buyerEmail,shappingAddress,deliveryMethod, orderItems,subtotal,basket.PaymentIntentId);
            await orderRepo.AddAsync(order);

            // 6. Save To Database [TODO]
            var result = await _unitOfWork. CompeleteAsync();
            if (result <= 0) return null;
            
            return order;
        }
        public async Task<IReadOnlyList<Order>> GetOrdersForUserAsync(string buyerEmail)
        {
            var orderRepo = _unitOfWork.Repository<Order>();

            var spec = new OrderSpecifications(buyerEmail);

            var orders = await orderRepo.GetAllWithSpecAsync(spec);

            return orders;
        }
        public Task<Order?> GetOrderByIdForUserAsync(int orderId, string buyerEmail)
        {
            var orderRepo = _unitOfWork.Repository<Order>();

            var spec = new OrderSpecifications(orderId, buyerEmail);

            var order = orderRepo.GetEntityWithSpecAsync(spec);

            return order;
        }

        public async Task<IReadOnlyList<DeliveryMethod>> GetDeliveryMethodsAsync()
            => await _unitOfWork.Repository<DeliveryMethod>().GetAllAsync();
    }
}
