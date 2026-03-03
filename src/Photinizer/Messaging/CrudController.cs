namespace Photinizer.Messaging;

public class CrudController<T, TId>(Crud operations, ICrudRepository<T, TId> repository) : INeedMessenger
{
    public void IncorporateMessenger(Messenger messenger) {
        var entityName = typeof(T).Name;
        if (operations.HasFlag(Crud.Create)) 
            messenger.OnQueryAsync<T>($"{entityName}.create", async entity => (await repository.Create(entity!))!);
        if (operations.HasFlag(Crud.ReadAll)) 
            messenger.OnQueryAsync<TId>($"{entityName}.read", async id => (await repository.Read(id!))!);
        if (operations.HasFlag(Crud.ReadAll)) 
            messenger.OnQueryAsync($"{entityName}.readAll", async id => await repository.ReadAll());
        if (operations.HasFlag(Crud.Update)) 
            messenger.OnTaskAsync<T>($"{entityName}.update", repository.Update!);
        if (operations.HasFlag(Crud.Delete))
            messenger.OnTaskAsync<TId>($"{entityName}.delete", repository.Delete!);
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