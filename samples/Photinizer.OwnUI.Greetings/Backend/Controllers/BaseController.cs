using Photinizer.Messaging;
using Photinizer.Template.Default.Backend.DataLayer;
using Photinizer.Template.Default.Backend.Entities;

namespace Photinizer.Template.Default.Backend.Controllers;

internal class BaseController<T>(Crud operations) : CrudController<T, int>(operations, new CrudRepository<T>()) where T : BaseEntity;