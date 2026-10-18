using Brix.Utils.Logging;
using UnityEngine;
using UnityEngine.UI;

public class RadialLayoutElementDivider : RadialLayoutElement
{
	private static BrixLogger logger = new BrixLogger(typeof(RadialLayoutElementDivider).FullName);

	public Button button;

	public override void Init(RadialLayoutGroup radialLayoutGroup)
	{
		base.Init(radialLayoutGroup);
		bgRawImage.gameObject.SetActive(value: false);
	}

	public override void InitBgRawImage()
	{
		base.InitBgRawImage();
		bgRawImage.material = new Material(radialLayoutGroup.segmentMaterial);
	}

	public override void OnMouseEnter(bool flag)
	{
		base.OnMouseEnter(flag);
		if (RadialLayoutElement.isDebug)
		{
			logger.Info(base.name + ".OnMouseEnter(" + flag + ")\n");
		}
		radialLayoutGroup.OnUpdateTooltipText(this, showFlag: false);
	}
}
