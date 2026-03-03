using Photinizer.Messaging;
using Photinizer.Template.Default.Backend.Entities;
using System.Text.Json;

namespace Photinizer.Template.Default.Backend.DataLayer;

internal class CrudRepository<T> : ICrudRepository<T, int>
    where T : BaseEntity
{
    private readonly string _db = "data.dat";

    public async Task<int> Create(T entity) 
    {
        entity.Id = 1024;
        await File.WriteAllTextAsync(_db, JsonSerializer.Serialize(entity));
        return entity.Id;
    }

    public Task Delete(int id)
    {
        File.Delete(_db);
        return Task.CompletedTask;
    }

    public async Task<T> Read(int id)
    {
        var json = await File.ReadAllTextAsync(_db);
        return JsonSerializer.Deserialize<T>(json)!;
    }

    public async Task<IReadOnlyCollection<T>> ReadAll()
        => [await Read(0)];
    
    public Task Update(T entity) => Create(entity);
}