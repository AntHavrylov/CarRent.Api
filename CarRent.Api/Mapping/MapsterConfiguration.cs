using CarRent.Application.Models;
using CarRent.Contracts.Requests;
using CarRent.Contracts.Responses;
using Mapster;

namespace CarRent.Api.Mapping;

public class MapsterConfiguration
{
    public MapsterConfiguration()
    {
        TypeAdapterConfig.GlobalSettings.NewConfig<(CreateOrUpdateCarRequest, Guid), Car>()
            .Map(dest => dest, source => source.Item1)
            .Map(dest => dest.Id, source => source.Item2);

        TypeAdapterConfig.GlobalSettings.NewConfig<(IEnumerable<Car> cars, int page, int pageSize, int total), CarsResponse>()
            .Map(dest => dest.Items, source => source.cars.Adapt<IEnumerable<CarResponse>>())
            .Map(dest => dest.Page, source => source.page)
            .Map(dest => dest.PageSize, source => source.pageSize)
            .Map(dest => dest.Total, source => source.total);

        TypeAdapterConfig.GlobalSettings.NewConfig<(RateCarRequest request, Guid userId, Guid carId), CarRating>()
            .Map(dest => dest.Rating, source => source.request.Rating)
            .Map(dest => dest.UserId, source => source.userId)
            .Map(dest => dest.CarId, source => source.carId);

        TypeAdapterConfig.GlobalSettings.NewConfig<(CreateOrUpdateOrderRequest request, Guid carId, Guid userId), Order>()
            .Map(dest => dest, source => source.request)
            .Map(dest => dest.CarId, source => source.carId)
            .Map(dest => dest.UserId, source => source.userId);

        TypeAdapterConfig.GlobalSettings.NewConfig<(CreateOrUpdateUserRequest request, Guid id), User>()
            .Map(dest => dest, source => source.request)
            .Map(dest => dest.Id, source => source.id);

    }
}
