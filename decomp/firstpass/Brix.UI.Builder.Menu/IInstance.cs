using UnityEngine;

namespace Brix.UI.Builder.Menu;

public interface IInstance
{
	bool HasInstance { get; }

	void ParentToTrs(RectTransform trs);

	void Request(bool skipIfInstanced);

	void Release();
}
