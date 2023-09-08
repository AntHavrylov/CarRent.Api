using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRent.Contracts.Requests
{
    public class GetAllRequest
    {
        public required int Page { get; init; } = 1;

        public required int PageSize { get; init; } = 10;

        public required string? SortBy { get; init; }
    }
}
