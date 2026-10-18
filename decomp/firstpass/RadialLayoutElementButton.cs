using UnityEngine;
using UnityEngine.UI;

public class RadialLayoutElementButton : RadialLayoutElement
{
	[HideInInspector]
	public Button button;

	public override void Init(RadialLayoutGroup radialLayoutGroup)
	{
		base.Init(radialLayoutGroup);
		InitSignals();
	}

	public override void InitBgRawImage()
	{
		base.InitBgRawImage();
		bgRawImage.material = new Material(radialLayoutGroup.segmentMaterial);
	}

	public override void OnMouseEnter(bool flag)
	{
		base.OnMouseEnter(flag);
		radialLayoutGroup.OnUpdateTooltipText(this, showFlag: true);
	}

	public void OnClick()
	{
	}
}
