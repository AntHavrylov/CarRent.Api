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

namespace CarRent.Api.Controllers;

[ApiController]
public class CarRatingsController: ControllerBase
{
    private readonly IRatingsService _ratingService;
    private readonly IValidator<RateCarRequest> _rateCarRequestValidator;

    public CarRatingsController(IRatingsService ratingService,
        IValidator<RateCarRequest> rateCarRequestValidator)
    {
        _rateCarRequestValidator = rateCarRequestValidator;
        _ratingService = ratingService;
    }

    [Authorize]
    [HttpPost(ApiEndpoints.Cars.Rate)]
    public async Task<IActionResult> RateCar([FromRoute] Guid id,
        [FromBody] RateCarRequest request, CancellationToken token)
    {
        await _rateCarRequestValidator.ValidateAndThrowAsync(request, token);
        var userId = HttpContext.GetUserId();
        var carRating = (request, userId, id).Adapt<CarRating>(MapsterConfiguration.CarRatingConfig);
        var result = await _ratingService.RateCarAsync(carRating, token);
        return result ? Ok(result) : NotFound();
    }

    [Authorize]
    [HttpDelete(ApiEndpoints.Cars.DeleteRating)]
    public async Task<IActionResult> DeleteRating([FromRoute] Guid id,
        CancellationToken token)
    {
        var userId = HttpContext.GetUserId();
        var result = await _ratingService.DeleteRatingAsync(userId!.Value, id, token);
        return result ? Ok(result) : NotFound();
    }

    [Authorize]
    [HttpGet(ApiEndpoints.Ratings.GetUserRatings)]
    public async Task<IActionResult> GetUserRatings(CancellationToken token = default)
    {
        var userId = HttpContext.GetUserId();
        var ratings = await _ratingService.GetRatingsForUserAsync(userId!.Value, token);
        return Ok(ratings.Adapt<IEnumerable<CarRatingResponse>>());
    }
}
