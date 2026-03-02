namespace Photinizer.Messaging;

internal interface ICrudRepository<T, TId>
{
    Task<TId> Create(T entity);
    Task<T> Read(TId id);
    Task<T> ReadAll();
    Task Update(T entity);
    Task Delete(TId id);
}