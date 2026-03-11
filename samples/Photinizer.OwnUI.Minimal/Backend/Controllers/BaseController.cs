using Photinizer.CRUD;
using Photinizer.Messaging;

namespace Photinizer.OwnUI.Minimal.Backend.Controllers;

internal class BaseController<T>(IMessenger messenger) : CrudController<T, int, NoFilter>(messenger);