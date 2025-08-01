using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Data.ResponseModel;
using IKARUSWEB.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using DevExtreme.AspNet.Mvc;

namespace IKARUSWEB.Infrastructure.Persistence.Repository
{
    public class EfRepository<T> : IRepository<T> where T : class
    {
        private readonly IKARUSWEBDbContext _context;
        private readonly DbSet<T> _dbSet;

        public EfRepository(IKARUSWEBDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public IQueryable<T> Query()
            => _dbSet.AsNoTracking();

        public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id, cancellationToken);

        public async Task<IReadOnlyList<T>> ListAsync(CancellationToken cancellationToken = default)
            => await _dbSet
                .AsNoTracking()
                .ToListAsync(cancellationToken);

        public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
            => await _dbSet.AddAsync(entity, cancellationToken);

        public void Update(T entity)
            => _dbSet.Update(entity);

        public void Delete(T entity)
            => _dbSet.Remove(entity);

        public async Task<LoadResult> LoadDataAsync(DataSourceLoadOptions loadOptions)
            => await DataSourceLoader.LoadAsync(_dbSet.AsNoTracking(), loadOptions);
    }
}
