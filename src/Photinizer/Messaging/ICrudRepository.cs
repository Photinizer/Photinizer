namespace Photinizer.Messaging;

public interface ICrudRepository<T, TId>
{
    Task<TId> Create(T entity);
    Task<T> Read(TId id);
    Task<IReadOnlyCollection<T>> ReadAll();
    Task Update(T entity);
    Task Delete(TId id);
}