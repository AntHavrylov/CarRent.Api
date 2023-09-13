using CarRent.Application.Models;
using CarRent.Contracts.Requests;
using CarRent.Contracts.Responses;
using Mapster;

namespace CarRent.Api.Mapping;

public static class MapsterConfiguration
{
    public static TypeAdapterConfig CreateUpdateCarConfig =>
        GetCreateOrUpdateCarConfig();

    public static TypeAdapterConfig CarsResponseConfig =>
        GetCarsResponseConfig();

    public static TypeAdapterConfig CarRatingConfig =>
        GetCarRatingConfig();

    public static TypeAdapterConfig OrderConfig =>
        GetOrderConfig();

    public static TypeAdapterConfig UserConfig =>
        GetUserConfig();

    private static TypeAdapterConfig GetCreateOrUpdateCarConfig()
    {
        var config = new TypeAdapterConfig();
        config.NewConfig<(CreateOrUpdateCarRequest, Guid), Car>()
            .Map(dest => dest, source => source.Item1)
            .Map(dest => dest.Id, source => source.Item2);
        return config;
    }

    private static TypeAdapterConfig GetCarsResponseConfig()
    {
        var config = new TypeAdapterConfig();
        config.NewConfig<(IEnumerable<Car>, int, int, int), CarsResponse>()
            .Map(dest => dest.Items, source => source.Item1
                .Adapt<IEnumerable<CarResponse>>())
            .Map(dest => dest.Page, source => source.Item2)
            .Map(dest => dest.PageSize, source => source.Item3)
            .Map(dest => dest.Total, source => source.Item4);
        return config;
    }

    private static TypeAdapterConfig GetCarRatingConfig()
    {
        var config = new TypeAdapterConfig();
        config.NewConfig<(RateCarRequest, Guid, Guid), CarRating>()
            .Map(dest => dest.Rating, source => source.Item1.Rating)
            .Map(dest => dest.UserId, source => source.Item2)
            .Map(dest => dest.CarId, source => source.Item3);
        return config;
    }

    private static TypeAdapterConfig GetOrderConfig()
    {
        var config = new TypeAdapterConfig();
        config.NewConfig<(CreateOrUpdateOrderRequest, Guid, Guid), Order>()
            .Map(dest => dest, source => source.Item1)
            .Map(dest => dest, source => source.Item2)
            .Map(dest => dest.UserId, source => source.Item3);
        return config;
    }

    private static TypeAdapterConfig GetUserConfig()
    {
        var config = new TypeAdapterConfig();
        config.NewConfig<(CreateOrUpdateUserRequest, Guid), User>()
            .Map(dest => dest, source => source.Item1)
            .Map(dest => dest.Id, source => source.Item2);
        return config;
    }
}
