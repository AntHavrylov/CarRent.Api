namespace CarRent.Api;

public static class ApiEndpoints
{
    private const string ApiBase = "api";

    public static class Cars
    {
        private const string Base = $"{ApiBase}/cars";

        public const string Create = Base;
        public const string Get = $"{Base}/{{id:guid}}";
        public const string GetAll = Base;
        public const string Update = $"{Base}/{{id:guid}}";
        public const string Delete = $"{Base}/{{id:guid}}";
    }

    public static class Users
    {
        private const string Base = $"{ApiBase}/users";

        public const string Create = Base;
        public const string GetById = $"{Base}/{{id:guid}}";
        public const string GetAll = Base;
        public const string Update = $"{Base}/{{id:guid}}";
        public const string Delete = $"{Base}/{{id:guid}}";
    }

    public static class Orders
    {
        private const string Base = $"{ApiBase}/orders";

        public const string Create = Base;
        public const string GetById = $"{Base}/{{id:guid}}";
        public const string GetUserOrders = $"{Base}/me";
        public const string CancelUserOrder = $"{Base}/me/{{id:guid}}";
        public const string GetAll = Base;
        public const string Update = $"{Base}/{{id:guid}}";
        public const string Delete = $"{Base}/{{id:guid}}";
    }

}
