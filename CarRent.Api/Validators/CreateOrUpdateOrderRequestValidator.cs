using CarRent.Application.Repositories;
using CarRent.Contracts.Requests;
using FluentValidation;

namespace CarRent.Api.Validators;

public class CreateOrUpdateOrderRequestValidator : AbstractValidator<CreateOrUpdateOrderRequest>
{

    private readonly IOrdersRepository _ordersRepository;
    private readonly ICarsRepository _carsRepository;

    public CreateOrUpdateOrderRequestValidator(IOrdersRepository ordersRepository, 
        ICarsRepository carsRepository,
        CancellationToken token = default)
    {
        _ordersRepository = ordersRepository;

        RuleFor(x => x.DateFrom)
            .GreaterThanOrEqualTo(DateTime.Now);
        RuleFor(x => x.DateTo)
            .GreaterThan(x => x.DateFrom)
            .WithMessage($"Date to has to be greated than date from.");
        RuleFor(x => x)
            .MustAsync(async (x, token) =>
            {
                var result = await IsOrderOverlapped(x, token);
                return !result;
            })
            .WithMessage($"Unable to create order it's overlapped with exists one.");
        _carsRepository = carsRepository;

        RuleFor(x => x.CarId)
            .MustAsync(CarExists)
            .WithMessage("Unable to find car");
    }

    private async Task<bool> CarExists(Guid carId,  CancellationToken token) => 
        await _carsRepository.ExistsByIdAsync(carId, token);

    private async Task<bool> IsOrderOverlapped(CreateOrUpdateOrderRequest request, CancellationToken token = default) =>
        await _ordersRepository.ExistsByCarIdAndDateAsync(request.CarId, request.DateFrom, request.DateTo, token);
}
