using System.Text.Json;

namespace Photinizer.Messaging;

internal class CrudController<T, TId>(Crud operations, ICrudRepository<T, TId> repository) : INeedMessenger
{
    public void Incorporate(Messenger messenger) {
        var entityName = typeof(T).Name;
        var _ = operations switch
        {
            var _ when operations.HasFlag(Crud.Create) => messenger.OnQueryAsync<T>($"{entityName}.create", async entity => await repository.Create(entity)),
            var _ when operations.HasFlag(Crud.Read) => messenger.OnQueryAsync<TId>($"{entityName}.read", async id => await repository.Read(id)),
            var _ when operations.HasFlag(Crud.ReadAll) => messenger.OnQueryAsync($"{entityName}.readAll", async id => await repository.ReadAll()),
            var _ when operations.HasFlag(Crud.Update) => messenger.OnTaskAsync<T>($"{entityName}.update", repository.Update),
            var _ when operations.HasFlag(Crud.Delete) => messenger.OnTaskAsync<TId>($"{entityName}.delete", repository.Delete),
            _ => messenger
        };
    }
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