﻿using CarRent.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRent.Application.Services
{
    public interface ICarsService
    {
        Task<bool> CreateAsync(Car car, CancellationToken token = default);

        Task<Car?> GetByIdAsync(Guid id, CancellationToken token = default);

        Task<Car?> UpdateAsync(Car car, CancellationToken token = default);

        Task<IEnumerable<Car>> GetAllAsync(GetAllCarsOptions options, CancellationToken token = default);

        Task<bool> DeleteByIdAsync(Guid id, CancellationToken token = default);

        Task<int> GetCountAsync(GetAllCarsOptions options, CancellationToken token = default);
    }
}
