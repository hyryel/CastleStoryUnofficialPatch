using Brix.UI.Builder.Menu;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.UI.Builder.Widget;

public class CaptureProgressLayoutElementWidgetComponent : LayoutElementWidgetComponent
{
	[SerializeField]
	protected RawImage _rawImage;

	public Material SourceMaterial;

	private Material _material;

	public override bool Refresh(IWidget w)
	{
		if (!base.Refresh(w))
		{
			return false;
		}
		return true;
	}
}
