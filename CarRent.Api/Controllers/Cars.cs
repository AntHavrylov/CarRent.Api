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
    public class Cars : ControllerBase
    {
        private readonly ICarsService _carsService;
        private readonly IValidator<CreateOrUpdateCarRequest> _createCarRequestValidator;
        private readonly IValidator<GetAllCarsRequest> _getAllCarsRequestValidator;

        public Cars(ICarsService carsService,
            IValidator<CreateOrUpdateCarRequest> createCarRequestValidator,
            IValidator<GetAllCarsRequest> getAllCarsRequestValidator)
        {
            _carsService = carsService;
            _createCarRequestValidator = createCarRequestValidator;
            _getAllCarsRequestValidator = getAllCarsRequestValidator;
        }

        //[Authorize(AuthConstants.TrustedMemberPolicyName)]
        [HttpPost(ApiEndpoints.Cars.Create)]
        public async Task<IActionResult> CreateAsync([FromBody] CreateOrUpdateCarRequest request,
            CancellationToken token)
        {
            await _createCarRequestValidator.ValidateAndThrowAsync(request,token);
            var car = request.MapToCar();
            var result = await _carsService.CreateAsync(car, token);
            if (!result)
            {
                return Conflict();
            }
            return CreatedAtAction(nameof(Get), new { id = car.Id }, car.MapToCarResponse());
        }

        [HttpPut(ApiEndpoints.Cars.Update)]
        public async Task<IActionResult> UpdateAsync([FromRoute] Guid id, [FromBody] CreateOrUpdateCarRequest request, CancellationToken token) 
        {
            var car = request.MapToCar(id);
            var result = await _carsService.UpdateAsync(car, token);
            return result is not null ? Ok(result.MapToCarResponse()) : NotFound();
        }


        [HttpGet(ApiEndpoints.Cars.GetAll)]
        public async Task<IActionResult> GetAll([FromQuery] GetAllCarsRequest request,
            CancellationToken token)
        {
            await _getAllCarsRequestValidator.ValidateAndThrowAsync(request,token);
            var options = request.MapToGetAllCarsOptions();
            var result = await _carsService.GetAllAsync(options, token);
            var carsCount = await _carsService.GetCountAsync(options, token);
            return Ok(result.MapToCarResponses(options.Page, options.PageSize, carsCount));
        }

        //[Authorize]
        [HttpGet(ApiEndpoints.Cars.Get)]
        public async Task<IActionResult> Get([FromRoute] Guid id,
            CancellationToken token)
        {
            var car = await _carsService.GetByIdAsync(id, token);
            return car is null ? NotFound() : Ok(car.MapToCarResponse());
        }

        [HttpDelete(ApiEndpoints.Cars.Delete)]
        public async Task<IActionResult> Delete([FromRoute] Guid id,
            CancellationToken token)
        {
            var result = await _carsService.DeleteByIdAsync(id, token);
            return result ? Ok(result) : NotFound();
        }

    }
}
