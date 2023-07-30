using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;

namespace pos.sys.Repositories
{
    public interface IRepository<T>
    {
        IQueryable<T> GetAll();
        IQueryable<T> Query(Expression<Func<T, bool>> filter);
        Tuple<int, IQueryable<T>> QueryWithPaging(Expression<Func<T, bool>> filter, int pageIndex, int pageSize);
        void Insert(T entity);
        void InsertRange(List<T> entity);
        T InsertReturn(T entity);
        Task<T> InsertReturnAsync(T entity);
        void Delete(T entity);
        void DeleteRange(List<T> entity);
        T Update(T NewEntity);
        List<T> UpdateRange(List<T> NewEntity);
        T UpdatePartial(T NewEntity, params Expression<Func<T, object>>[] propertiesToUpdate);
        Task<T> UpdateCompleteAsync(T NewEntity);
        int Commit();
        Task<int> CommitAsync();
    }
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly DbContext _dbContext;
        public Repository(DbContext dbContext)
        {
            this._dbContext = dbContext;
        }
        public virtual IQueryable<T> GetAll()
        {
            return _dbContext.Set<T>().AsQueryable().AsNoTracking();
        }
        public virtual IQueryable<T> Query(Expression<Func<T, bool>> filter)
        {
            return _dbContext.Set<T>().Where(filter).AsQueryable().AsNoTracking();
        }

        public virtual Tuple<int, IQueryable<T>> QueryWithPaging(Expression<Func<T, bool>> filter, int pageIndex = 0, int pageSize = 10)
        {

            if (pageIndex <= 0)
            {

                return Tuple.Create(_dbContext.Set<T>().Where(filter).AsNoTracking().Count(), _dbContext.Set<T>().Where(filter).AsNoTracking());
            }
            else
            {
                return Tuple.Create(_dbContext.Set<T>().Where(filter).Count(), _dbContext.Set<T>().Where(filter).Skip((pageIndex - 1) * pageSize).Take(pageSize).AsNoTracking());
            }
        }
        public virtual void Insert(T entity)
        {
            _dbContext.Set<T>().Add(entity);
        }
        public virtual T InsertReturn(T entity)
        {
            T newEntity = _dbContext.Set<T>().Add(entity).Entity;
            return newEntity;
        }
        public virtual async Task<T> InsertReturnAsync(T entity)
        {
            T newEntity = _dbContext.Set<T>().Add(entity).Entity;
            var result = await _dbContext.SaveChangesAsync();
            if (result > 0)
            {
                return newEntity;
            }
            else
            {
                return null;
            }
        }
        public virtual void Delete(T entity)
        {
            _dbContext.Set<T>().Remove(entity);
        }
        public virtual T Update(T NewEntity)
        {
            _dbContext.Set<T>().Attach(NewEntity);
            _dbContext.Entry(NewEntity).State = EntityState.Modified;
            return NewEntity;
        }
        public virtual T UpdatePartial(T NewEntity, params Expression<Func<T, object>>[] propertiesToUpdate)
        {

            _dbContext.Set<T>().Attach(NewEntity);
            foreach (var p in propertiesToUpdate)
            {
                _dbContext.Entry(NewEntity).Property(p).IsModified = true;
            }
            return NewEntity;
        }
        public virtual async Task<T> UpdateCompleteAsync(T NewEntity)
        {
            _dbContext.Set<T>().Attach(NewEntity);
            _dbContext.Entry(NewEntity).State = EntityState.Modified;
            var result = await _dbContext.SaveChangesAsync();
            if (result > 0)
            {
                return NewEntity;
            }
            else
            {
                return null;
            }
        }

        public virtual List<T> RawSqlQuery<T>(string query, Func<DbDataReader, T> map)
        {
            var entities = new List<T>();
            using (var command = _dbContext.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = query;
                command.CommandType = CommandType.Text;

                _dbContext.Database.OpenConnection();

                using (var result = command.ExecuteReader())
                {
                    while (result.Read())
                    {
                        entities.Add(map(result));
                    }

                }
                _dbContext.Database.CloseConnection();
            }
            return entities;
        }

        public virtual int Commit()
        {
            return _dbContext.SaveChanges();
        }

        public virtual async Task<int> CommitAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }
        public List<T> UpdateRange(List<T> NewEntity)
        {
            _dbContext.Set<T>().AttachRange(NewEntity);
            foreach (var item in NewEntity) {
                _dbContext.Entry(item).State = EntityState.Modified;
            }
            return NewEntity;
        }

        public void InsertRange(List<T> entity)
        {
            _dbContext.Set<T>().AddRange(entity);
        }

        public void DeleteRange(List<T> entity)
        {
            _dbContext.Set<T>().RemoveRange(entity);
        }
    }
}
