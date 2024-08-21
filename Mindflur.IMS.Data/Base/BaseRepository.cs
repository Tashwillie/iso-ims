using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mindflur.IMS.Application.Contracts.Repository;

namespace Mindflur.IMS.Data.Base
{
    public class BaseRepository<T> : IAsyncRepository<T> where T : class
    {
        protected readonly IMSDEVContext _context;
       

        public BaseRepository(IMSDEVContext dbContext, ILogger<T> logger)
        {
            _context = dbContext; 
        }

        public virtual async Task<T> GetByIdAsync(int id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public async Task<IReadOnlyList<T>> ListAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }

        public virtual async Task<IReadOnlyList<T>> GetPagedResponseAsync(int page, int size)
        {
            return await _context.Set<T>().Skip((page - 1) * size).Take(size).AsNoTracking().ToListAsync();
        }

        public async Task<T> AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
            await _context.SaveChangesAsync();

            return entity;
        }

        public async Task UpdateAsync(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(T entity)
        {
            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync();
        }

        //Following code block is for Stored Procedure
        //Using stored procedure with EF Core is not recommended
        //For Read Operation
        public async Task<IList<T>> StoredProcedureQueryAsync(string storedProcedureName, SqlParameter[]? parameters)
        {
            var parameterNames = GetParameterNames(parameters);
            return await _context.Set<T>().FromSqlRaw(string.Format("{0} {1}", storedProcedureName, string.Join(",", parameterNames)), parameters).ToListAsync();
        }

        //For Insert, Update, Delete Operations
        public async Task<int> StoredProcedureCommandAsync(string storedProcedureName, SqlParameter[] parameters = null)
        {
            var parameterNames = GetParameterNames(parameters);
            return await _context.Database.ExecuteSqlRawAsync(string.Format("{0} {1}", storedProcedureName, string.Join(",", parameterNames)), parameters);
        }

        private string[]? GetParameterNames(SqlParameter[]? parameters)
        {
            if (parameters == null)
                return null;

            var parameterNames = new string[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                parameterNames[i] = parameters[i].ParameterName;
            }
            return parameterNames;
        }

       
    }
}