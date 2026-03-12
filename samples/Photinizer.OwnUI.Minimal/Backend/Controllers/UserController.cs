using Photinizer.CRUD;
using Photinizer.Messaging;
using Photinizer.OwnUI.Minimal.Backend.DataLayer;
using Photinizer.OwnUI.Minimal.Backend.Entities;

namespace Photinizer.OwnUI.Minimal.Backend.Controllers;

internal class UserController(
    IMessenger messenger,
    CrudRepository<User> crudRepository) : BaseController<User>(messenger), IRunnableService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        AddCreate(crudRepository.Create);
        AddRead(crudRepository.Read);
        AddReadAll(filter => crudRepository.ReadAll());
        AddUpdate(crudRepository.Update);
        AddDelete(crudRepository.Delete);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}