using CarRent.Application.Models;
using CarRent.Contracts.Requests;
using CarRent.Contracts.Responses;
using System.Reflection.Metadata.Ecma335;

namespace CarRent.Api.Mapping
{
    public static class ContractMapping
    {
        public static Car MapToCar(this CreateOrUpdateCarRequest request) =>
            new()
            {
                Id = Guid.NewGuid(),
                Brand = request.Brand,
                Model = request.Model,
                EngineType = (EngineType)Enum.Parse(typeof(EngineType), request.EngineType),
                BodyType = (BodyType)Enum.Parse(typeof(BodyType), request.BodyType),
                YearOfProduction = request.YearOfProduction,
            };

        public static Car MapToCar(this CreateOrUpdateCarRequest request, Guid id) =>
           new()
           {
               Id = id,
               Brand = request.Brand,
               Model = request.Model,
               EngineType = (EngineType)Enum.Parse(typeof(EngineType), request.EngineType),
               BodyType = (BodyType)Enum.Parse(typeof(BodyType), request.BodyType),
               YearOfProduction = request.YearOfProduction,
           };

        public static CarResponse MapToCarResponse(this Car car) =>
            new()
            {
                Id = car.Id,
                Brand = car.Brand,
                Model = car.Model,
                Slug = car.Slug,
                EngineType = car.EngineType.ToString(),
                BodyType = car.BodyType.ToString(),
                YearOfProduction = car.YearOfProduction,
            };

        public static GetAllCarsOptions MapToGetAllCarsOptions(this GetAllCarsRequest request) =>
            new()
            {
                Slug = request.Slug,
                YearOfProduction = request?.YearOfProduction,
                Page = request.Page,
                PageSize = request.PageSize,
                SortField = request.SortBy?.Trim('+', '-'),
                SortOrder = request.SortBy is null ?
                    SortOrder.Unsorted :
                    request.SortBy.StartsWith('-') ?
                        SortOrder.Descending :
                        SortOrder.Ascending,
            };
        public static CarsResponse MapToCarsResponse(
            this IEnumerable<Car> cars,
            int page,
            int pageSize,
            int carsCount
            ) =>
            new()
            {
                Items = cars.Select(MapToCarResponse),
                Page = page,
                PageSize = pageSize,
                Total = carsCount
            };


        public static User MapToUser(this CreateOrUpdateUserRequest request) =>
            new()
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Email = request.Email
            };

        public static User MapToUser(this CreateOrUpdateUserRequest request, Guid id) =>
            new()
            {
                Id = id,
                Name = request.Name,
                Email = request.Email
            };

        public static UserResponse MapToResponse(this User user) =>
            new()
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email
            };

        public static UsersResponse MapToUsersResponse(
            this IEnumerable<User> users) => new()
            {
                Items = users.Select(MapToResponse)
            };

        public static Order MapToOrder(this CreateOrUpdateOrderRequest request,
            Guid userId,
            Guid id = default) =>
            new()
            {
                Id = id == default ? Guid.NewGuid() : id,
                UserId = userId,
                CarId = request.CarId,
                DateFrom = request.DateFrom,
                DateTo = request.DateTo,
            };

        public static OrderResponse MapToResponse(this Order order) =>
            new()
            {
                Id = order.Id,
                UserId = order.UserId,
                CarId = order.CarId,
                DateFrom = order.DateFrom,
                DateTo = order.DateTo,
            };

        public static OrdersResponse MapToResponse(this IEnumerable<Order> orders) =>
            new()
            {
                Items = orders.Select(MapToResponse)
            };

    }
}
