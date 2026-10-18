using Brix.External.Factories;

namespace Brix.Lifecycle.Pooling;

public interface IPoolableComponent : IInitializable, IRequestable, IResetable
{
}
