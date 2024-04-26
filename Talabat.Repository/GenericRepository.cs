using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entities;
using Talabat.Core.Repository.Contract;
using Talabat.Core.Specifications;
using Talabat.Repository.Data;

namespace Talabat.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntitiy
    {
        private readonly StoreContext _Context;

        public GenericRepository(StoreContext Context)
        {
            _Context = Context;
        }
        public async Task<IReadOnlyList<T>> GetAllAsync()
        {
            //if(typeof(T)==typeof(Product))
            //    return (IEnumerable < T >) await _Context.Set<Product>().Include(p => p.Brand).Include(p=>p.Category).ToListAsync();
            return await _Context.Set<T>().ToListAsync();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            //if(typeof(T)==typeof(Product))
            //    return await _Context.Set<Product>().Where(p=>p.Id == id).Include(p => p.Brand).Include(p => p.Category).FirstOrDefaultAsync() as T;
            return await _Context.Set<T?>().FindAsync(id);
        }

        public async Task<IReadOnlyList<T>> GetAllWithSpecAsync(ISpecification<T> spec)
        {
            return await ApplySpecifications(spec).ToListAsync();
        }
        public async Task<T?> GetEntityWithSpecAsync(ISpecification<T> spec)
        {
            return await ApplySpecifications(spec).FirstOrDefaultAsync();
        }

        public async Task<int> GetCountAsync(ISpecification<T> spec)
        {
            return await ApplySpecifications(spec).CountAsync();
        }

        private IQueryable<T> ApplySpecifications(ISpecification<T> spec)
        {
            return SpecificationsEvaluator<T>.GetQuary(_Context.Set<T>(), spec);
        }

        public async Task AddAsync(T entity)
           => await _Context.AddAsync(entity);
       

        public void Update(T entity)
            => _Context.Update(entity);

        public void Delete(T entity)
            => _Context.Remove(entity);
    } 
}
