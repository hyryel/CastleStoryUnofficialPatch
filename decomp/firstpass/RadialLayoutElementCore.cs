using Brix.Utils.Logging;
using UnityEngine;
using UnityEngine.UI;

public class RadialLayoutElementCore : RadialLayoutElement
{
	private static BrixLogger logger = new BrixLogger(typeof(RadialLayoutElementCore).FullName);

	private float defaultCoreBotRadius = 0.3f;

	private float defaultCoreMidRadius = 0.3f;

	private float defaultCoreTopRadius = 0.3f;

	[HideInInspector]
	public float coreRadiusAO;

	private float defaultCoreBotThickness = 0.225f;

	private float defaultCoreMidThickness = 0.141f;

	private float defaultCoreTopThickness = 0.5f;

	[HideInInspector]
	public float coreThicknessAO;

	private float defaultCoreBotWidth = 0.42f;

	private float defaultCoreMidWidth = 1f;

	private float defaultCoreTopWidth = 0.42f;

	[HideInInspector]
	public float coreWidthAO;

	[HideInInspector]
	public Button button;

	[HideInInspector]
	public RawImage bgMidRawImage;

	[HideInInspector]
	public RawImage bgTopRawImage;

	[HideInInspector]
	public RawImage bgBotRawImage => bgRawImage;

	public override Material bgMat
	{
		get
		{
			return bgRawImage.material;
		}
		set
		{
			bgRawImage.material = value;
		}
	}

	public override void OnEnable()
	{
		base.OnEnable();
		GetDefaultMaterialValues();
	}

	public void GetDefaultMaterialValues()
	{
		if (!(radialLayoutGroup == null))
		{
			defaultCoreBotRadius = radialLayoutGroup.coreBotBgMaterial.GetFloat("_Radius");
			defaultCoreMidRadius = radialLayoutGroup.coreMidBgMaterial.GetFloat("_Radius");
			defaultCoreTopRadius = radialLayoutGroup.coreTopBgMaterial.GetFloat("_Radius");
			defaultCoreBotThickness = radialLayoutGroup.coreBotBgMaterial.GetFloat("_Thickness");
			defaultCoreMidThickness = radialLayoutGroup.coreMidBgMaterial.GetFloat("_Thickness");
			defaultCoreTopThickness = radialLayoutGroup.coreTopBgMaterial.GetFloat("_Thickness");
			defaultCoreBotWidth = radialLayoutGroup.coreBotBgMaterial.GetFloat("_Width");
			defaultCoreMidWidth = radialLayoutGroup.coreMidBgMaterial.GetFloat("_Width");
			defaultCoreTopWidth = radialLayoutGroup.coreTopBgMaterial.GetFloat("_Width");
		}
	}

	public void UpdateCoreRadius()
	{
		bgBotRawImage.material.SetFloat("_Radius", defaultCoreBotRadius + coreRadiusAO);
		bgMidRawImage.material.SetFloat("_Radius", defaultCoreMidRadius + coreRadiusAO);
		bgTopRawImage.material.SetFloat("_Radius", defaultCoreTopRadius + coreRadiusAO);
	}

	public void UpdateCoreThickness()
	{
		bgBotRawImage.material.SetFloat("_Thickness", defaultCoreBotThickness + coreThicknessAO);
		bgMidRawImage.material.SetFloat("_Thickness", defaultCoreMidThickness + coreThicknessAO);
		bgTopRawImage.material.SetFloat("_Thickness", defaultCoreTopThickness + coreThicknessAO);
	}

	public void UpdateCoreWidth()
	{
		bgBotRawImage.material.SetFloat("_Width", defaultCoreBotWidth + coreWidthAO);
		bgMidRawImage.material.SetFloat("_Width", defaultCoreMidWidth + coreWidthAO);
		bgTopRawImage.material.SetFloat("_Width", defaultCoreTopWidth + coreWidthAO);
	}

	public override void Init(RadialLayoutGroup radialLayoutGroup)
	{
		base.Init(radialLayoutGroup);
	}

	public override void UpdateBgTransform()
	{
		base.UpdateBgTransform();
		Vector2 corePos = GetCorePos();
		float coreDiameter = GetCoreDiameter();
		ApplyTransform(bgRawImage.transform as RectTransform, corePos, coreDiameter);
		ApplyTransform(bgMidRawImage.transform as RectTransform, corePos, coreDiameter);
		ApplyTransform(bgTopRawImage.transform as RectTransform, corePos, coreDiameter);
	}

	public override void UpdateTransform()
	{
		Vector2 corePos = GetCorePos();
		float coreDiameter = GetCoreDiameter();
		ApplyTransform(base.transform as RectTransform, corePos, coreDiameter);
	}

	protected Vector2 GetCorePos()
	{
		return new Vector2(radialLayoutGroup.baseOffsetPos.x, radialLayoutGroup.baseOffsetPos.y);
	}

	protected float GetCoreDiameter()
	{
		float num = radialLayoutGroup.baseSize;
		if (radialLayoutGroup.containerPiePopulator.isSubMenu)
		{
			num = ((!(radialLayoutGroup.containerPiePopulator.superMenu == null)) ? radialLayoutGroup.containerPiePopulator.superMenu.radialLayoutGroup.baseSize : num);
		}
		return num * 2f;
	}

	protected void ApplyTransform(RectTransform rectTrs, Vector2 corePos, float coreDiameter)
	{
		Vector3 position = rectTrs.position;
		position.x = corePos.x;
		position.y = corePos.y;
		if (!float.IsNaN(position.x) && !float.IsNaN(position.y))
		{
			rectTrs.position = position;
		}
		rectTrs.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, coreDiameter);
		rectTrs.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, coreDiameter);
	}

	public override void InitBgRawImage()
	{
		base.InitBgRawImage();
		bgRawImage.material = new Material(radialLayoutGroup.coreBotBgMaterial);
		bgMidRawImage = DoInitBgRawImage(bgMidRawImage, "Mid", 90f);
		bgTopRawImage = DoInitBgRawImage(bgTopRawImage, "Top", 180f);
		bgMidRawImage.material = new Material(radialLayoutGroup.coreMidBgMaterial);
		bgTopRawImage.material = new Material(radialLayoutGroup.coreTopBgMaterial);
	}

	public override bool HitTest(Vector2 pos, float d)
	{
		float num = radialLayoutGroup.baseSize * radialLayoutGroup.defaultRadiusRange.x;
		if (radialLayoutGroup.isCtrlSchemeHold)
		{
			num = radialLayoutGroup.GetLowestInnerBaseSize();
		}
		return d < num;
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

	public override void OnMouseDown(float angle)
	{
		base.OnMouseDown(angle);
		if (RadialLayoutElement.isDebug)
		{
			logger.Info(base.name + ".OnMouseDown(" + angle + ")\n");
		}
		if (radialLayoutGroup.isFrozen && radialLayoutGroup.isUsingFrozen)
		{
		}
	}
}
