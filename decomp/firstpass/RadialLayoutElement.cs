using System;
using Brix.Audio;
using Brix.UI.Builder.Menu;
using Brix.Utils.Logging;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class RadialLayoutElement : MonoBehaviour
{
	[Flags]
	public enum EventsUsed
	{
		OnPointerEnter = 1,
		OnPointerExit = 2,
		OnPointerUp = 4,
		OnLeftPointerUp = 8,
		OnPointerDown = 0x10,
		OnPointerHold = 0x20
	}

	public enum OutHitTestCase
	{
		None,
		Spread,
		RadiusIn,
		RadiusOut
	}

	protected enum SoundEventEnum
	{
		HoverTrue,
		HoverFalse,
		HoverDisabledTrue,
		HoverDisabledFalse,
		Action,
		ActionNegative,
		AltAction,
		AltActionNegative,
		ToggleTrue,
		ToggleFalse,
		ScrollPositive,
		ScrollNegative,
		Text,
		Open,
		Close
	}

	private static BrixLogger logger = new BrixLogger(typeof(RadialLayoutElement).FullName);

	public static bool isDebug = false;

	[HideInInspector]
	[SerializeField]
	private UnityEvent m_OnPointerEnter = new UnityEvent();

	[SerializeField]
	[HideInInspector]
	private UnityEvent m_OnPointerExit = new UnityEvent();

	[SerializeField]
	[HideInInspector]
	private UnityEvent m_OnPointerUp = new UnityEvent();

	[HideInInspector]
	[SerializeField]
	private UnityEvent m_OnLeftPointerUp = new UnityEvent();

	[HideInInspector]
	[SerializeField]
	private UnityEvent m_OnPointerDown = new UnityEvent();

	[SerializeField]
	[HideInInspector]
	private UnityEvent m_OnPointerHold = new UnityEvent();

	public static string[] eventsUsedLabels = new string[6] { "On Pointer Enter", "On Pointer Exit", "On Pointer Up", "On Left Pointer Up", "On Pointer Down", "On Pointer Hold" };

	[SerializeField]
	[HideInInspector]
	private EventsUsed m_EventsUsed;

	[HideInInspector]
	[Disabled]
	public bool isHover;

	[HideInInspector]
	[Disabled]
	public bool isTrueHover;

	public bool isSelectable;

	[Disabled]
	[HideInInspector]
	public bool isSelection;

	[Disabled]
	[HideInInspector]
	public bool isDown;

	[HideInInspector]
	[Disabled]
	public bool isDrag;

	[HideInInspector]
	[Disabled]
	public bool isDragIn;

	[HideInInspector]
	[Disabled]
	public bool isDragOut;

	[HideInInspector]
	[Disabled]
	public bool isFrozen;

	[HideInInspector]
	public float angleOffsetAO;

	[HideInInspector]
	public float radiusInnerAO;

	[HideInInspector]
	public float radiusOuterAO;

	[HideInInspector]
	public bool usePreferredSpreadOffset;

	[HideInInspector]
	public float preferredSpreadOffset;

	[HideInInspector]
	public bool usePreferredSpreadRangeSize;

	[HideInInspector]
	public float preferredSpreadRangeSize;

	[HideInInspector]
	public bool usePreferredArcLength;

	[HideInInspector]
	public float preferredArcLength;

	public RawImage bgRawImage;

	[Disabled]
	[HideInInspector]
	public Containee containee;

	[HideInInspector]
	[Disabled]
	public RadialLayoutGroup radialLayoutGroup;

	[HideInInspector]
	[Disabled]
	public float outWeight;

	[HideInInspector]
	[Disabled]
	public float outRelWeight;

	[HideInInspector]
	[Disabled]
	public float outAngleSpread;

	[Disabled]
	[HideInInspector]
	public float outRawAngleSpread;

	[Disabled]
	[HideInInspector]
	public float outRawSumSpread;

	[Disabled]
	[HideInInspector]
	public float outAngleOffset;

	[Disabled]
	[HideInInspector]
	public float outUserAngleOffset;

	[Disabled]
	[HideInInspector]
	public float outOtherAngleOffset;

	[Disabled]
	[HideInInspector]
	public float outTmpAngleOffset;

	[HideInInspector]
	[Disabled]
	public float outTmpTraverse;

	[HideInInspector]
	[Disabled]
	public float outTmpRangedTraverse;

	[Disabled]
	[HideInInspector]
	public float outTmpAngleDistance;

	[Disabled]
	[HideInInspector]
	public float outRadius;

	[Disabled]
	[HideInInspector]
	public float outRadiusInner;

	[Disabled]
	[HideInInspector]
	public float outRadiusOuter;

	[Disabled]
	[HideInInspector]
	public float outRadiusInnerAO;

	[Disabled]
	[HideInInspector]
	public float outRadiusOuterAO;

	[Disabled]
	[HideInInspector]
	public float outRadiusInnerRaw;

	[Disabled]
	[HideInInspector]
	public float outRadiusOuterRaw;

	[Disabled]
	[HideInInspector]
	public float outRadiusBase;

	[Disabled]
	[HideInInspector]
	public float outRadiusAverage;

	[Disabled]
	[HideInInspector]
	public OutHitTestCase outHitTestCase;

	private bool wasToggleFlagged;

	public virtual UnityEvent onPointerEnter
	{
		get
		{
			return m_OnPointerEnter;
		}
		set
		{
			m_OnPointerEnter = value;
		}
	}

	public virtual UnityEvent onPointerExit
	{
		get
		{
			return m_OnPointerExit;
		}
		set
		{
			m_OnPointerExit = value;
		}
	}

	public virtual UnityEvent onPointerUp
	{
		get
		{
			return m_OnPointerUp;
		}
		set
		{
			m_OnPointerUp = value;
		}
	}

	public virtual UnityEvent onLeftPointerUp
	{
		get
		{
			return m_OnLeftPointerUp;
		}
		set
		{
			m_OnLeftPointerUp = value;
		}
	}

	public virtual UnityEvent onPointerDown
	{
		get
		{
			return m_OnPointerDown;
		}
		set
		{
			m_OnPointerDown = value;
		}
	}

	public virtual UnityEvent onPointerHold
	{
		get
		{
			return m_OnPointerHold;
		}
		set
		{
			m_OnPointerHold = value;
		}
	}

	public virtual EventsUsed eventsUsed
	{
		get
		{
			return m_EventsUsed;
		}
		set
		{
			m_EventsUsed = value;
		}
	}

	public bool isIconEnabled => !(containeePie == null) && containeePie.isIconEnabled;

	public bool isActionNegative => !(containeePie == null) && containeePie.isActionNegative;

	public bool isActivatedOnClick => !(containeePie == null) && containeePie.isActivatedOnClick;

	public float radiusAverage => (radiusInner + radiusOuter) * 0.5f;

	private ContaineePie containeePie => (!(containee == null)) ? (containee as ContaineePie) : null;

	public float baseSize => radialLayoutGroup.baseSize;

	public bool applyInnerRadiusHover => radialLayoutGroup.applyInnerRadiusHover;

	public bool applyOuterRadiusHover => radialLayoutGroup.applyOuterRadiusHover;

	public bool isMouseDownPreventUnhover => radialLayoutGroup.isMouseDownPreventUnhover;

	public bool isUsingSelectionLock => radialLayoutGroup.isUsingSelectionLock;

	public RadialLayoutGroup.InputEvent actionInputEvent => radialLayoutGroup.actionInputEvent;

	public float weight => 1f;

	public virtual Material bgMat
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

	public virtual Color color
	{
		get
		{
			return bgMat.GetColor("_Color");
		}
		set
		{
			bgMat.SetColor("_Color", value);
		}
	}

	public float colorMask
	{
		get
		{
			return bgMat.GetFloat("_ColorMask");
		}
		set
		{
			bgMat.SetFloat("_ColorMask", value);
		}
	}

	public float angleOffset
	{
		get
		{
			return bgMat.GetFloat("_AngleOffset") - angleOffsetAO;
		}
		set
		{
			bgMat.SetFloat("_AngleOffset", value + angleOffsetAO);
		}
	}

	public float angleOffsetRaw => bgMat.GetFloat("_AngleOffset");

	public float angleSpread
	{
		get
		{
			return bgMat.GetFloat("_AngleSpread");
		}
		set
		{
			bgMat.SetFloat("_AngleSpread", value);
		}
	}

	public float radiusInner
	{
		get
		{
			return bgMat.GetFloat("_RadiusInner") - radiusInnerAO;
		}
		set
		{
			bgMat.SetFloat("_RadiusInner", value + radiusInnerAO);
		}
	}

	public float radiusOuter
	{
		get
		{
			return bgMat.GetFloat("_RadiusOuter") - radiusOuterAO;
		}
		set
		{
			bgMat.SetFloat("_RadiusOuter", value + radiusOuterAO);
		}
	}

	public float radiusInnerRaw => bgMat.GetFloat("_RadiusInner");

	public float radiusOuterRaw => bgMat.GetFloat("_RadiusOuter");

	public float baseAngleOffset
	{
		get
		{
			return bgMat.GetFloat("_BaseAngleOffset");
		}
		set
		{
			bgMat.SetFloat("_BaseAngleOffset", value);
		}
	}

	public bool hasProgress
	{
		get
		{
			return bgMat.GetFloat("_HasProgress") == 1f;
		}
		set
		{
			bgMat.SetFloat("_HasProgress", (!value) ? 0f : 1f);
		}
	}

	public Color progressColor
	{
		get
		{
			return bgMat.GetColor("_ProgressColor");
		}
		set
		{
			bgMat.SetColor("_ProgressColor", value);
		}
	}

	public float progress
	{
		get
		{
			return bgMat.GetFloat("_Progress");
		}
		set
		{
			bgMat.SetFloat("_Progress", value);
		}
	}

	public bool isToggable
	{
		get
		{
			return bgMat.GetFloat("_IsToggable") == 1f;
		}
		set
		{
			bgMat.SetFloat("_IsToggable", (!value) ? 0f : 1f);
		}
	}

	public Color toggleColor
	{
		get
		{
			return bgMat.GetColor("_ToggleColor");
		}
		set
		{
			bgMat.SetColor("_ToggleColor", value);
		}
	}

	public float toggleRadius
	{
		get
		{
			return bgMat.GetFloat("_ToggleRadius");
		}
		set
		{
			bgMat.SetFloat("_ToggleRadius", value);
		}
	}

	public bool isToggle
	{
		get
		{
			return bgMat.GetFloat("_IsToggle") == 1f;
		}
		set
		{
			bgMat.SetFloat("_IsToggle", (!value) ? 0f : 1f);
		}
	}

	public bool hasLineLimitMin
	{
		get
		{
			return bgMat.GetFloat("_HasLineLimitMin") == 1f;
		}
		set
		{
			bgMat.SetFloat("_HasLineLimitMin", (!value) ? 0f : 1f);
		}
	}

	public float lineLimitMin
	{
		get
		{
			return bgMat.GetFloat("_LineLimitMin");
		}
		set
		{
			bgMat.SetFloat("_LineLimitMin", value);
		}
	}

	public bool hasLineLimitMax
	{
		get
		{
			return bgMat.GetFloat("_HasLineLimitMax") == 1f;
		}
		set
		{
			bgMat.SetFloat("_HasLineLimitMax", (!value) ? 0f : 1f);
		}
	}

	public float lineLimitMax
	{
		get
		{
			return bgMat.GetFloat("_LineLimitMax");
		}
		set
		{
			bgMat.SetFloat("_LineLimitMax", value);
		}
	}

	public float lineAngleSpread
	{
		get
		{
			return bgMat.GetFloat("_LineAngleSpread");
		}
		set
		{
			bgMat.SetFloat("_LineAngleSpread", value);
		}
	}

	public float alphaFactor
	{
		get
		{
			return bgMat.GetFloat("_AlphaFactor");
		}
		set
		{
			bgMat.SetFloat("_AlphaFactor", value);
		}
	}

	public float disabledFactor
	{
		set
		{
			bgMat.SetFloat("_DisabledFactor", value);
		}
	}

	public virtual bool IsEventUsed(EventsUsed eventType)
	{
		return (eventsUsed & eventType) == eventType;
	}

	public void ShowSelf()
	{
		ToggleSelf(flag: true);
	}

	public void HideSelf()
	{
		ToggleSelf(flag: false);
	}

	public void ToggleSelf(bool flag)
	{
		if (containee.interpolatorTargetable != null)
		{
			string text = ((!flag) ? "Closed" : "Opened");
			string empty = string.Empty;
			empty = ((containee.interpolatorTargetable.nextTargetStateNames.Count != 0) ? containee.interpolatorTargetable.nextTargetStateNames[0] : empty);
			if (empty != text && containee.interpolatorTargetable.currentTargetStateName != text)
			{
				containee.interpolatorTargetable.PlayTo(text);
			}
		}
		else
		{
			base.gameObject.SetActive(flag);
		}
	}

	public void ToggleSelection()
	{
		bool flag = !isSelection;
		ToggleSelection(flag);
	}

	public void ToggleSelection(bool flag)
	{
		if (!isSelectable)
		{
			return;
		}
		bool flag2 = isSelection;
		bool flag3 = (isSelection = flag);
		if (flag3 != flag2 && !flag3)
		{
			if (!isHover)
			{
				ToggleSelf(flag: true);
			}
			else if (containee.interpolatorTargetable != null)
			{
				containee.interpolatorTargetable.nextTargetStateNames.Clear();
				containee.interpolatorTargetable.currentTargetStateName = string.Empty;
				containee.interpolatorTargetable.PlayTo("Hovered");
			}
		}
	}

	public void ToggleFreeze(bool flag)
	{
		isFrozen = flag;
	}

	public bool HitTest(Vector2 pos)
	{
		float d = Mathf.Sqrt(Mathf.Pow(pos.x, 2f) + Mathf.Pow(pos.y, 2f));
		return HitTest(pos, d);
	}

	public virtual bool HitTest(Vector2 pos, float d)
	{
		bool result = true;
		outHitTestCase = OutHitTestCase.None;
		float num = angleSpread;
		float num2 = angleOffset;
		num2 += radialLayoutGroup.rawBaseAngleOffset;
		num2 = (10000f + num2) % 1f;
		float num3 = (outTmpAngleOffset = num2 + num / 2f) - (outTmpRangedTraverse = (10000f + (outTmpTraverse = Mathf.Atan2(pos.y, pos.x) / ((float)Math.PI * 2f))) % 1f);
		if (num3 > 1f)
		{
			num3 -= 1f;
		}
		if (num3 < 0f)
		{
			num3 += 1f;
		}
		num3 = (outTmpAngleDistance = Mathf.Abs(num3));
		float num4 = radiusInner;
		float num5 = radiusOuter;
		num4 *= baseSize;
		num5 *= baseSize;
		if (radialLayoutGroup.isCtrlSchemeHold)
		{
			num4 = radialLayoutGroup.GetLowestInnerBaseSize();
		}
		if (num3 > num)
		{
			outHitTestCase = OutHitTestCase.Spread;
			result = false;
		}
		else if (applyInnerRadiusHover && d < num4)
		{
			outHitTestCase = OutHitTestCase.RadiusIn;
			result = false;
		}
		else if (applyOuterRadiusHover && d > num5)
		{
			outHitTestCase = OutHitTestCase.RadiusOut;
			result = false;
		}
		return result;
	}

	public virtual void UpdateBgTransform()
	{
		RectTransform rectTransform = bgRawImage.transform as RectTransform;
		Vector3 position = rectTransform.position;
		position.x = radialLayoutGroup.baseOffsetPos.x;
		position.y = radialLayoutGroup.baseOffsetPos.y;
		rectTransform.position = position;
		float size = radialLayoutGroup.baseSize * 2f;
		rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
		rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
	}

	public virtual void UpdateTransform()
	{
		float num = radiusAverage;
		num += (radiusInnerAO + radiusOuterAO) * 0.5f;
		num *= radialLayoutGroup.baseSize;
		num = (outRadius = num * radialLayoutGroup.BaseSizeFactor);
		outRadiusInner = radiusInner;
		outRadiusOuter = radiusOuter;
		outRadiusInnerAO = radiusInnerAO;
		outRadiusOuterAO = radiusOuterAO;
		outRadiusInnerRaw = radiusInnerRaw;
		outRadiusOuterRaw = radiusOuterRaw;
		outRadiusBase = radialLayoutGroup.baseSize;
		outRadiusAverage = radiusAverage;
		float num2 = angleOffset;
		num2 += radialLayoutGroup.baseAngleOffset;
		num2 += radialLayoutGroup.baseAngleSpreadRange.x;
		num2 *= (float)Math.PI * 2f;
		Vector2 vector = base.transform.position;
		vector.x = num * Mathf.Cos(num2) + radialLayoutGroup.baseOffsetPos.x;
		vector.y = num * Mathf.Sin(num2) + radialLayoutGroup.baseOffsetPos.y;
		if (!float.IsNaN(vector.x) && !float.IsNaN(vector.y))
		{
			base.transform.position = vector;
		}
		Color buttonColor = GetButtonColor();
		if (containeePie.pieRawImage != null)
		{
			containeePie.pieRawImage.color = buttonColor;
		}
		if (containeePie.pieImage != null)
		{
			containeePie.pieImage.color = buttonColor;
		}
	}

	private Color GetButtonColor()
	{
		if ((radialLayoutGroup.isFrozen && radialLayoutGroup.isUsingFrozen && !isHover) || isFrozen)
		{
			return new Color(1f, 1f, 1f, 0.25f);
		}
		return new Color(1f, 1f, 1f, 1f);
	}

	public virtual void Init(RadialLayoutGroup radialLayoutGroup)
	{
		if (isDebug)
		{
			logger.Info(Mathf.Round(Time.time * 100f) / 100f + " :: " + base.name + "[RadialLayoutElement].Init()\n");
		}
		this.radialLayoutGroup = radialLayoutGroup;
		InitBgRawImage();
		InitContainee();
	}

	public virtual void InitSignals()
	{
	}

	private void InitContainee()
	{
		containee = ((!(containee == null)) ? containee : base.gameObject.GetComponent<Containee>());
	}

	public virtual void InitBgRawImage()
	{
		bgRawImage = DoInitBgRawImage(bgRawImage, "Bot", 0f);
	}

	protected RawImage DoInitBgRawImage(RawImage targetRawImage, string prefixName, float rotationDeg)
	{
		if (targetRawImage != null)
		{
			return targetRawImage;
		}
		if (isDebug)
		{
			logger.Info(base.name + ".InitBg" + prefixName + "RawImage()");
		}
		GameObject gameObject = new GameObject("[RadialLayoutElement] Bg" + prefixName + "RawImage", typeof(RectTransform));
		gameObject.transform.SetParent(base.transform);
		gameObject.transform.SetSiblingIndex(0);
		gameObject.transform.localScale = Vector3.one;
		RectTransform rectTransform = gameObject.transform as RectTransform;
		rectTransform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, rotationDeg));
		targetRawImage = gameObject.AddComponent<RawImage>();
		targetRawImage.color = Color.white;
		return targetRawImage;
	}

	public virtual void UpdateBackground()
	{
	}

	public virtual void OnEnable()
	{
		InitSignals();
	}

	public virtual void Start()
	{
	}

	public virtual void Update()
	{
	}

	public void AltClick()
	{
		PlaySound((!isActionNegative) ? SoundEventEnum.AltAction : SoundEventEnum.AltActionNegative);
		TryContaineeAltClick();
	}

	public void TryContaineeAltClick()
	{
		if (isIconEnabled)
		{
			containee.AltClick();
		}
	}

	public void Click()
	{
		PlaySound((!isActionNegative) ? SoundEventEnum.Action : SoundEventEnum.ActionNegative);
		TryContaineeClick();
	}

	public void TryContaineeClick()
	{
		if (isIconEnabled)
		{
			containee.Click();
		}
	}

	public void TryShowSubMenu()
	{
		if (radialLayoutGroup.containerPiePopulator.spawnsSubMenu && radialLayoutGroup.containerPiePopulator.subMenu != null)
		{
			radialLayoutGroup.containerPiePopulator.subMenu.radialLayoutGroup.ShowSelf();
		}
	}

	public void TryHideSuperMenu()
	{
		if (radialLayoutGroup.containerPiePopulator.isSubMenu && radialLayoutGroup.containerPiePopulator.superMenu != null)
		{
			radialLayoutGroup.containerPiePopulator.superMenu.radialLayoutGroup.HideSelf();
		}
	}

	public void TryHideMenu()
	{
		if (radialLayoutGroup != null)
		{
			radialLayoutGroup.HideSelf();
		}
	}

	public void FreezeMenu()
	{
		radialLayoutGroup.ToggleFreeze(flag: true);
	}

	public void UnfreezeMenu()
	{
		radialLayoutGroup.ToggleFreeze(flag: false);
	}

	public void TryFreezeSuperMenu()
	{
		if (radialLayoutGroup.containerPiePopulator.isSubMenu && radialLayoutGroup.containerPiePopulator.superMenu != null)
		{
			radialLayoutGroup.containerPiePopulator.superMenu.radialLayoutGroup.ToggleFreeze(flag: true);
		}
	}

	public void TryUnfreezeSuperMenu()
	{
		if (radialLayoutGroup.containerPiePopulator.isSubMenu && radialLayoutGroup.containerPiePopulator.superMenu != null)
		{
			radialLayoutGroup.containerPiePopulator.superMenu.radialLayoutGroup.ToggleFreeze(flag: false);
		}
	}

	public void TryCloseSubMenu()
	{
		if (this.radialLayoutGroup.containerPiePopulator.isSubMenu && this.radialLayoutGroup.containerPiePopulator.superMenu != null)
		{
			ContainerPiePopulator containerPiePopulator = this.radialLayoutGroup.containerPiePopulator;
			RadialLayoutGroup radialLayoutGroup = this.radialLayoutGroup;
			ContainerPiePopulator superMenu = this.radialLayoutGroup.containerPiePopulator.superMenu;
			RadialLayoutGroup radialLayoutGroup2 = superMenu.radialLayoutGroup;
			radialLayoutGroup.HideSelf();
			radialLayoutGroup2.ToggleFreeze(flag: false);
			superMenu.ShowCore();
			radialLayoutGroup2.ShowSelf();
			containerPiePopulator.HideCore();
			radialLayoutGroup2.SetSelections(flag: false);
		}
	}

	public void ToggleFlag()
	{
		wasToggleFlagged = true;
	}

	public void TryToggleFlag()
	{
		if (!wasToggleFlagged)
		{
			ToggleFlag();
		}
	}

	public virtual void OnMouseEnter(bool flag)
	{
		if (actionInputEvent == RadialLayoutGroup.InputEvent.Release)
		{
			isDragIn = flag && !isDown;
			isDragOut = isDragOut && !isTrueHover;
			isTrueHover = isDragIn;
			isDrag = isDown;
		}
		else
		{
			isDrag = false;
			isDragIn = false;
			isDragOut = false;
			isTrueHover = true;
			isDrag = false;
		}
		bool flag2 = true;
		isHover = flag2;
		SoundEngine.Play(MenuSound.GetSoundLabel(MenuSound.Prefix.Pie, MenuSound.Suffix.RolloverOn));
		m_OnPointerEnter.Invoke();
	}

	public virtual void OnMouseExit(bool flag)
	{
		isDragIn = false;
		isDragOut = false;
		isTrueHover = false;
		bool flag2 = false;
		isHover = flag2;
		if (actionInputEvent == RadialLayoutGroup.InputEvent.Release)
		{
			isDrag = isDragOut;
		}
		else
		{
			isDrag = false;
		}
		SoundEngine.Play(MenuSound.GetSoundLabel(MenuSound.Prefix.Pie, MenuSound.Suffix.RolloverOff));
		m_OnPointerExit.Invoke();
	}

	public virtual void OnMouseDown(float angle)
	{
		isDown = true;
		isDrag = false;
		if (actionInputEvent == RadialLayoutGroup.InputEvent.Press)
		{
			PlaySound(SoundEventEnum.Action);
		}
		m_OnPointerDown.Invoke();
	}

	public virtual void OnMouseUp(float angle)
	{
		isDown = false;
		isDrag = false;
		isDragIn = false;
		isDragOut = false;
		isTrueHover = false;
		m_OnPointerUp.Invoke();
	}

	public virtual void OnLeftMouseUp(float angle)
	{
		if (isSelectable && !radialLayoutGroup.isCtrlSchemeHold)
		{
			radialLayoutGroup.OnSelectionExclusive(this, !isSelection);
			m_OnLeftPointerUp.Invoke();
		}
		else
		{
			m_OnLeftPointerUp.Invoke();
		}
	}

	public virtual void OnMouseDrag(float angle)
	{
		isDrag = isDown;
	}

	public virtual void OnScrollWheel(Vector2 delta)
	{
		PlaySound((!(delta.y > 0f)) ? SoundEventEnum.ScrollNegative : SoundEventEnum.ScrollPositive);
	}

	public virtual void OnPointerHold()
	{
		if (isSelectable && !isSelection)
		{
			radialLayoutGroup.OnSelectionExclusive(this, !isSelection);
			m_OnPointerHold.Invoke();
		}
		else
		{
			m_OnPointerHold.Invoke();
		}
	}

	public virtual void OnWarpToHovered()
	{
		isHover = true;
		isDown = true;
		isDrag = true;
		if (containee.interpolatorTargetable != null)
		{
			string stateName = "Hovered";
			containee.interpolatorTargetable.WarpTo(stateName);
		}
		else
		{
			base.gameObject.SetActive(value: true);
		}
	}

	public virtual void OnSelectionExclusive(bool flag)
	{
		ToggleSelection(flag);
	}

	protected void PlaySound(SoundEventEnum soundEventEnum)
	{
		if (!(containee == null))
		{
			MenuSound.Suffix suffix = MenuSound.Suffix._None;
			switch (soundEventEnum)
			{
			case SoundEventEnum.HoverTrue:
				suffix = containee.soundHoverTrue;
				break;
			case SoundEventEnum.HoverFalse:
				suffix = containee.soundHoverFalse;
				break;
			case SoundEventEnum.HoverDisabledTrue:
				suffix = containee.soundHoverDisabledTrue;
				break;
			case SoundEventEnum.HoverDisabledFalse:
				suffix = containee.soundHoverDisabledFalse;
				break;
			case SoundEventEnum.Action:
				suffix = containee.soundAction;
				break;
			case SoundEventEnum.ActionNegative:
				suffix = containee.soundActionNegative;
				break;
			case SoundEventEnum.AltAction:
				suffix = containee.soundAltAction;
				break;
			case SoundEventEnum.AltActionNegative:
				suffix = containee.soundAltActionNegative;
				break;
			case SoundEventEnum.ToggleTrue:
				suffix = containee.soundToggleTrue;
				break;
			case SoundEventEnum.ToggleFalse:
				suffix = containee.soundToggleFalse;
				break;
			case SoundEventEnum.ScrollPositive:
				suffix = containee.soundScrollPositive;
				break;
			case SoundEventEnum.ScrollNegative:
				suffix = containee.soundScrollNegative;
				break;
			case SoundEventEnum.Text:
				suffix = containee.soundText;
				break;
			case SoundEventEnum.Open:
				suffix = containee.soundOpen;
				break;
			case SoundEventEnum.Close:
				suffix = containee.soundClose;
				break;
			}
			string soundLabel = MenuSound.GetSoundLabel(MenuSound.Prefix.Pie, suffix);
			if (!(soundLabel == string.Empty))
			{
				SoundEngine.Play(soundLabel);
			}
		}
	}
}
