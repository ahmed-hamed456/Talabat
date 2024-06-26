﻿using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Talabat.APIs.Dtos;
using Talabat.APIs.Errors;
using Talabat.Core.Entities;
using Talabat.Core.Repository.Contract;

namespace Talabat.APIs.Controllers
{
    public class BasketsController : BaseApiController
    {
        private readonly IBasketRepository _basketRepository;
        private readonly IMapper _mapper;

        public BasketsController(
            IBasketRepository basketRepository,
            IMapper mapper
            )
        {
            _basketRepository = basketRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<CustomerBasket>> GetBasket(string id)
        {
            var basket = await _basketRepository.GetBasketAsync(id);

            return basket ?? new CustomerBasket(id);
        }

        [HttpPost]
        public async Task<ActionResult<CustomerBasket>> UpdateBasket(CustomerBasketDto basket)
        {
            var mappedBasket = _mapper.Map<CustomerBasketDto,CustomerBasket>(basket);
              
            var createdOrUpdatedBasket = await _basketRepository.UpdateBasketAsync(mappedBasket);

            if (createdOrUpdatedBasket is null) return BadRequest(new ApiResponse(400));

            return createdOrUpdatedBasket;
        }

        [HttpDelete]
        public async Task<ActionResult<bool>> DeleteBasket(string id)
        {
            return await _basketRepository.DeleteBasketAsync(id);
        }
    }
}
