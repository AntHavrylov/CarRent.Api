using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRent.Contracts.Responses
{
    public class CarRatingResponse
    {
        public required Guid UserId { get; init; }
        
        public required Guid CarId { get; init; }

        public required int Rating { get; set; }
    }
}
