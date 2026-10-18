using MoonSharp.Interpreter;

namespace Brix.UI.Builder.Lui;

public interface IBaseLui
{
	Script OwnerScript { get; }

	Table T { get; }

	DynValue D { get; }

	int Id { get; }
}
