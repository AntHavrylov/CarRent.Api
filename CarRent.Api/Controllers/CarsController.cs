using CarRent.Api.Auth;
using CarRent.Api.Mapping;
using CarRent.Api.Validators;
using CarRent.Application.Models;
using CarRent.Application.Services;
using CarRent.Contracts.Requests;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRent.Api.Controllers
{

    [ApiController]
    public class CarsController : ControllerBase
    {
        private readonly ICarsService _carsService;
        private readonly IValidator<CreateOrUpdateCarRequest> _createCarRequestValidator;
        private readonly IValidator<GetAllCarsRequest> _getAllCarsRequestValidator;

        public CarsController(ICarsService carsService,
            IValidator<CreateOrUpdateCarRequest> createCarRequestValidator,
            IValidator<GetAllCarsRequest> getAllCarsRequestValidator)
        {
            _carsService = carsService;
            _createCarRequestValidator = createCarRequestValidator;
            _getAllCarsRequestValidator = getAllCarsRequestValidator;
        }

        [Authorize(AuthConstants.TrustedMemberPolicyName)]
        [HttpPost(ApiEndpoints.Cars.Create)]
        public async Task<IActionResult> Create([FromBody] CreateOrUpdateCarRequest request,
            CancellationToken token)
        {
            await _createCarRequestValidator.ValidateAndThrowAsync(request,token);
            var car = request.MapToCar();
            var result = await _carsService.CreateAsync(car, token);
            if (!result)
            {
                return Conflict();
            }
            return CreatedAtAction(nameof(GetById), new { id = car.Id }, car.MapToCarResponse());
        }

        [Authorize(AuthConstants.TrustedMemberPolicyName)]
        [HttpPut(ApiEndpoints.Cars.Update)]
        public async Task<IActionResult> Update([FromRoute] Guid id, 
            [FromBody] CreateOrUpdateCarRequest request, 
            CancellationToken token) 
        {
            await _createCarRequestValidator.ValidateAndThrowAsync(request, token);
            var car = request.MapToCar(id);
            var result = await _carsService.UpdateAsync(car, token);
            return result is not null ? Ok(result.MapToCarResponse()) : NotFound();
        }

        [AllowAnonymous]
        [HttpGet(ApiEndpoints.Cars.GetAll)]
        public async Task<IActionResult> GetAll([FromQuery] GetAllCarsRequest request,
            CancellationToken token)
        {
            await _getAllCarsRequestValidator.ValidateAndThrowAsync(request,token);
            var options = request.MapToGetAllCarsOptions();
            var result = await _carsService.GetAllAsync(options, token);
            var carsCount = await _carsService.GetCountAsync(options, token);
            return Ok(result.MapToCarsResponse(options.Page, options.PageSize, carsCount));
        }

        [HttpGet(ApiEndpoints.Cars.GetById)]
        public async Task<IActionResult> GetById([FromRoute] Guid id,
            CancellationToken token)
        {
            var car = await _carsService.GetById(id, token);
            return car is null ? NotFound() : Ok(car.MapToCarResponse());
        }

        [Authorize(AuthConstants.AdminUserPolicyName)]
        [HttpDelete(ApiEndpoints.Cars.Delete)]
        public async Task<IActionResult> Delete([FromRoute] Guid id,
            CancellationToken token)
        {
            var result = await _carsService.DeleteByIdAsync(id, token);
            return result ? Ok(result) : NotFound();
        }

    }
}
