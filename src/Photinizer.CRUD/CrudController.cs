using Photinizer.Messaging;

namespace Photinizer.CRUD;

public abstract class CrudController<T, TId, TFilter>(IMessenger messenger)
{
    private readonly string _entityName = typeof(T).Name;

    protected void AddCreate(Func<T, Task<TId>> onCreate)
        => messenger.OnQueryAsync<T>(Endpoint("create"), async (_, entity) => await onCreate(entity));

    protected void AddReadAll(Func<TFilter, Task<IReadOnlyCollection<T>>> onReadAll)
        => messenger.OnQueryAsync<TFilter>(Endpoint("readAll"), async (_, filter) => await onReadAll(filter));

    protected void AddRead(Func<TId, Task<T>> onRead)
        => messenger.OnQueryAsync<TId>(Endpoint("read"), async (_, id) => await onRead(id));

    protected void AddUpdate(Func<T, Task> update)
        => messenger.OnTaskAsync<T>(Endpoint("update"), async (_, entity) => await update(entity));

    protected void AddDelete(Func<TId, Task> delete)
        => messenger.OnTaskAsync<TId>(Endpoint("delete"), async (_, id) => await delete(id));

    protected string Endpoint(string endpoint) => $"{_entityName}.{endpoint}";
}

[Flags]
public enum Crud
{
    None = 0,
    Create = 1,
    Read = 2,
    ReadAll = 4,
    Update = 8,
    Delete = 16
}