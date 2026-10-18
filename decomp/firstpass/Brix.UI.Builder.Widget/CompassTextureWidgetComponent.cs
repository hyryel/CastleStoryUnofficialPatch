using Brix.UI.Builder.Menu;
using UnityEngine;

namespace Brix.UI.Builder.Widget;

public class CompassTextureWidgetComponent : TextureWidgetComponent
{
	public Material SourceMaterial;

	private Material _material;

	public override bool Refresh(IWidget w)
	{
		if (!base.Refresh(w))
		{
			return false;
		}
		if (_material == null)
		{
			_material = new Material(SourceMaterial);
		}
		return true;
	}
}
