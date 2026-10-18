using System.Collections.Generic;
using System.Threading.Tasks;
using CivicConnect.Domain.Entities;

namespace CivicConnect.Application.Interfaces.Repositories;

public interface IRequestRepository
{
    Task<Request?> GetByIdAsync(int id);
    Task<IEnumerable<Request>> GetAllAsync();
    Task<Request> AddAsync(Request request);
    Task UpdateAsync(Request request);
}
