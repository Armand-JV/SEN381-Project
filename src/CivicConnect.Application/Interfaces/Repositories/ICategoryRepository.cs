using System.Collections.Generic;
using System.Threading.Tasks;
using CivicConnect.Domain.Entities;

namespace CivicConnect.Application.Interfaces.Repositories;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllAsync();
}
