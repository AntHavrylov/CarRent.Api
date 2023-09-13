using CarRent.Api.Auth;
using CarRent.Api.Mapping;
using CarRent.Application.Models;
using CarRent.Application.Services;
using CarRent.Contracts.Requests;
using CarRent.Contracts.Responses;
using FluentValidation;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers
{

    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrdersService _ordersService;
        private readonly IValidator<CreateOrUpdateOrderRequest> _requestValidator;

        public OrdersController(IOrdersService ordersService,
            IValidator<CreateOrUpdateOrderRequest> requestValidator)
        {
            _ordersService = ordersService;
            _requestValidator = requestValidator;
        }

        [Authorize]
        [HttpPost(ApiEndpoints.Orders.Create)]
        public async Task<IActionResult> Create([FromBody] CreateOrUpdateOrderRequest request,
            CancellationToken token)
        {
            await _requestValidator.ValidateAndThrowAsync(request, token);
            var userId = HttpContext.GetUserId();
            var order = (request, Guid.NewGuid(), userId).Adapt<Order>(MapsterConfiguration.OrderConfig);
            var result = await _ordersService.CreateAsync(order, token);
            if (!result)
            {
                return Conflict();
            }
            return CreatedAtAction(nameof(Create), userId, order.Adapt<OrderResponse>());
        }

        [Authorize]
        [HttpGet(ApiEndpoints.Orders.GetUserOrders)]
        public async Task<IActionResult> GetUserOrders(CancellationToken token)
        {
            var userId = HttpContext.GetUserId();
            var result = await _ordersService.GetAllByUserIdAsync(userId!.Value, token);
            return Ok(result.Adapt<OrderResponse>());
        }

        [Authorize]
        [HttpPut(ApiEndpoints.Orders.Update)]
        public async Task<IActionResult> Update([FromRoute] Guid id,
            [FromBody] CreateOrUpdateOrderRequest request,
            CancellationToken token)
        {
            await _requestValidator.ValidateAndThrowAsync(request, token);
            var userId = HttpContext.GetUserId();
            var order = (request, id, userId).Adapt<Order>(MapsterConfiguration.OrderConfig);
            var updatedOrder = await _ordersService.UpdateAsync(order, token);
            return updatedOrder is not null ? Ok(updatedOrder!.Adapt<OrderResponse>()) : NotFound();
        }

        [Authorize]
        [HttpDelete(ApiEndpoints.Orders.CancelUserOrder)]
        public async Task<IActionResult> CancelOrder(Guid id,
            CancellationToken token)
        {
            var userId = HttpContext.GetUserId();
            var result = await _ordersService.CancelAsync(userId!.Value, id, token);
            return result ? Ok(result) : NotFound();
        }

        [Authorize(AuthConstants.TrustedMemberPolicyName)]
        [HttpDelete(ApiEndpoints.Orders.Delete)]
        public async Task<IActionResult> Delete([FromRoute] Guid id,
            CancellationToken token)
        {
            var result = await _ordersService.DeleteByIdAsync(id, token);
            return result ? Ok(result) : NotFound();
        }


    }
}
