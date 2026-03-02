using Photinizer.Messaging;
using Photinizer.Template.Default.Backend.Entities;

namespace Photinizer.Template.Default.Backend.Controllers;

internal class UserController() : BaseController<User>(Crud.Create | Crud.Read | Crud.ReadAll | Crud.Delete | Crud.Update);