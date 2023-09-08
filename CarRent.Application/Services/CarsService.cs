using CarRent.Application.Models;
using CarRent.Application.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRent.Application.Services
{
    public class CarsService : ICarsService
    {
        private readonly ICarsRepository _carsRepository;

        public CarsService(ICarsRepository carsRepository)
        {
            _carsRepository = carsRepository;
        }

        public async Task<bool> CreateAsync(Car car, CancellationToken token = default)
        {
            return await _carsRepository.CreateAsync(car, token);
        }

        public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken token = default)
        {
            return await _carsRepository.DeleteByIdAsync(id, token);
        }

        public async Task<IEnumerable<Car>> GetAllAsync(GetAllCarsOptions options, CancellationToken token = default)
        { 
            return await _carsRepository.GetAllAsync(options, token);
        }

        public async Task<Car?> GetByIdAsync(Guid id, CancellationToken token = default)
        {
            return await _carsRepository.GetByIdAsync(id, token);
        }

        public Task<int> GetCountAsync(GetAllCarsOptions options, CancellationToken token = default)
        {
            return _carsRepository.GetCountAsync(options,token);
        }

        public async Task<Car?> UpdateAsync(Car car, CancellationToken token = default)
        {
            var exists = await _carsRepository.ExistsByIdAsync(car.Id, token);    
            if(!exists) 
            {
                return null;
            }
            var bRes = await _carsRepository.UpdateAsync(car, token);
            return bRes ? car : null;
        }
    }
}
