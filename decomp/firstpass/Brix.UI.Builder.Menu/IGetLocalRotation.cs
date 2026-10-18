using UnityEngine;

namespace Brix.UI.Builder.Menu;

public interface IGetLocalRotation : IWidgetHandle
{
	Vector3 GetLocalRotation();
}
