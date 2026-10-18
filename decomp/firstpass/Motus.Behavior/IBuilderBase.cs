using Brix.Utils.Closures;
using UnityEngine;

namespace Motus.Behavior;

public interface IBuilderBase
{
	StaticNode ToNode { get; }

	void SetFinalize(IClosure<IAction> a);

	void Label(string label, Object context);
}
