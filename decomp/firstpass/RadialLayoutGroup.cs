using System;
using System.Collections.Generic;
using Brix.Audio;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Game.AI;
using Brix.Input;
using Brix.UI.Builder.Menu;
using Brix.Utils.Logging;
using UnityEngine;
using UnityEngine.UI;

public class RadialLayoutGroup : MonoBehaviour
{
	public enum MouseButton
	{
		Left,
		Right,
		Middle
	}

	public enum InputEvent
	{
		Press,
		Release
	}

	[Flags]
	public enum Inputs
	{
		None = 0,
		LeftDown = 1,
		RightDown = 2,
		LeftPressed = 4,
		RightPressed = 8,
		LeftUp = 0x10,
		RightUp = 0x20,
		OutsideScreen = 0x40,
		SpaceDown = 0x80
	}

	private static BrixLogger logger = new BrixLogger(typeof(RadialLayoutGroup).FullName);

	public static bool isDebug = false;

	public static bool isDebugB = false;

	public static object pieMenuTarget = null;

	[HideInInspector]
	public bool isCtrlSchemeHold = true;

	[HideInInspector]
	public Color defaultColor = new Color(1f, 1f, 1f, 0.25f);

	[HideInInspector]
	public Color hoverColor = new Color(1f, 1f, 1f, 1f);

	[HideInInspector]
	public Color selectionColor = new Color(1f, 1f, 1f, 1f);

	[HideInInspector]
	public Color progressColor = new Color(0f, 0f, 0f, 0.25f);

	[HideInInspector]
	public Color toggleColor = new Color(0f, 0f, 0f, 0.25f);

	[HideInInspector]
	public Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);

	public Material segmentMaterial;

	public Material coreBotBgMaterial;

	public Material coreMidBgMaterial;

	public Material coreTopBgMaterial;

	public Material arrowMaterial;

	[HideInInspector]
	public float baseSizeFactorAO = 1f;

	[HideInInspector]
	public float m_BaseSize = 400f;

	[Disabled]
	[HideInInspector]
	public Vector2 baseOffsetPos = default(Vector2);

	[HideInInspector]
	public float baseAngleOffsetAO;

	[HideInInspector]
	public float m_BaseAngleOffset;

	[HideInInspector]
	public Vector2 baseAngleSpreadRange = new Vector2(0f, 1f);

	[HideInInspector]
	[Disabled]
	public Vector2 mousePos = default(Vector2);

	[Disabled]
	[HideInInspector]
	public Vector2 mouseDownPos = default(Vector2);

	[Disabled]
	[HideInInspector]
	public Vector2 mouseDragPos = default(Vector2);

	[HideInInspector]
	public float defaultWeight = 1f;

	[HideInInspector]
	public Vector2 defaultRadiusRange = new Vector2(0.1f, 0.9f);

	[HideInInspector]
	public float hoverWeight = 1f;

	[HideInInspector]
	public float selectionWeight = 1.5f;

	[HideInInspector]
	public bool applyInnerRadiusHover = true;

	[HideInInspector]
	public bool applyOuterRadiusHover = true;

	[HideInInspector]
	public bool isMouseDown;

	[HideInInspector]
	public bool isLeftMouseDown;

	[HideInInspector]
	public bool isMouseDownPreventUnhover = true;

	[HideInInspector]
	public bool isUsingSelectionLock = true;

	public InputEvent actionInputEvent = InputEvent.Release;

	[HideInInspector]
	public bool isFrozen;

	[HideInInspector]
	public bool wasFrozen;

	[HideInInspector]
	public bool isUsingFrozen = true;

	[Disabled]
	public bool isHidden = true;

	public MouseButton actionMouseButton;

	public MouseButton altActionMouseButton = MouseButton.Right;

	[HideInInspector]
	public int hoverIdx = -1;

	[HideInInspector]
	public int selectionIdx = -1;

	[HideInInspector]
	private List<RadialLayoutElement> radialLayoutElements = new List<RadialLayoutElement>();

	[HideInInspector]
	public RadialLayoutElementCore coreRadialLayoutElement;

	public ContainerPiePopulator containerPiePopulator;

	[HideInInspector]
	public int testElemCount = 8;

	[HideInInspector]
	public bool testSpreadRangeB;

	private Material defaultUIMaterial;

	public RectTransform pickingPlane;

	public static Signal OpenProgressChanged = new Signal();

	public static float OpenProcessProgression = 0f;

	public static bool PieMenuIsOpen = false;

	public static Signal PieMenuOnShow = new Signal();

	public static Signal PieMenuOnHide = new Signal();

	private bool HasContent;

	[HideInInspector]
	public float creationInitTime = 0.3f;

	[HideInInspector]
	public float creationTime = -1f;

	[HideInInspector]
	public bool outIsCreationWaiting;

	[HideInInspector]
	public bool outIsCreationDue;

	[HideInInspector]
	public float actionInitTime = 0.5f;

	[HideInInspector]
	public float actionTime = -1f;

	[HideInInspector]
	public bool outIsActionWaiting;

	[HideInInspector]
	public bool outIsActionDue;

	[HideInInspector]
	public float heldActionInitTime = 0.6f;

	[HideInInspector]
	public float heldActionTime = -1f;

	[HideInInspector]
	public bool outIsHeldActionWaiting;

	private int heldItemIdx = -1;

	public RawImage arrowRawImage;

	[HideInInspector]
	public float arrowMagicFactor = 1f;

	private Material pieArrowMat;

	public float BaseSizeFactor => Neo.canvasScale;

	public float rawBaseSize => m_BaseSize;

	public float baseSize => rawBaseSize * baseSizeFactorAO;

	public float rawBaseAngleOffset
	{
		get
		{
			return Mathf.Repeat(m_BaseAngleOffset, 1f);
		}
		set
		{
			m_BaseAngleOffset = Mathf.Repeat(value, 1f);
		}
	}

	public float baseAngleOffset => Mathf.Repeat(m_BaseAngleOffset + baseAngleOffsetAO, 1f);

	public float defaultRadius => (defaultRadiusRange.y - defaultRadiusRange.x) * 0.5f + defaultRadiusRange.x;

	public Vector2 defaultVisualRadiusRange
	{
		get
		{
			return defaultRadiusRange;
		}
		set
		{
			defaultRadiusRange = value;
		}
	}

	public float hoverRadius => defaultRadius;

	public Vector2 hoverRadiusRange
	{
		get
		{
			return defaultRadiusRange;
		}
		set
		{
			defaultRadiusRange = value;
		}
	}

	public Vector2 hoverVisualRadiusRange
	{
		get
		{
			return defaultRadiusRange;
		}
		set
		{
			defaultRadiusRange = value;
		}
	}

	public float selectionRadius => defaultRadius;

	public Vector2 selectionRadiusRange
	{
		get
		{
			return defaultRadiusRange;
		}
		set
		{
			defaultRadiusRange = value;
		}
	}

	public Vector2 selectionVisualRadiusRange
	{
		get
		{
			return defaultRadiusRange;
		}
		set
		{
			defaultRadiusRange = value;
		}
	}

	public float pixelRadius => baseSize * 0.5f;

	public float pixelDefaultRadius => defaultRadius * pixelRadius;

	public Vector2 pixelDefaultRadiusRange
	{
		get
		{
			return new Vector2(defaultRadiusRange.x * pixelRadius, defaultRadiusRange.y * pixelRadius);
		}
		set
		{
			defaultRadiusRange = new Vector2(value.x / pixelRadius, value.y / pixelRadius);
		}
	}

	public Vector2 pixelDefaultVisualRadiusRange
	{
		get
		{
			return new Vector2(defaultVisualRadiusRange.x * pixelRadius, defaultVisualRadiusRange.y * pixelRadius);
		}
		set
		{
			defaultVisualRadiusRange = new Vector2(value.x / pixelRadius, value.y / pixelRadius);
		}
	}

	public float pixelHoverRadius => hoverRadius * pixelRadius;

	public Vector2 pixelHoverRadiusRange
	{
		get
		{
			return new Vector2(hoverRadiusRange.x * pixelRadius, hoverRadiusRange.y * pixelRadius);
		}
		set
		{
			hoverRadiusRange = new Vector2(value.x / pixelRadius, value.y / pixelRadius);
		}
	}

	public Vector2 pixelHoverVisualRadiusRange
	{
		get
		{
			return new Vector2(hoverVisualRadiusRange.x * pixelRadius, hoverVisualRadiusRange.y * pixelRadius);
		}
		set
		{
			hoverVisualRadiusRange = new Vector2(value.x / pixelRadius, value.y / pixelRadius);
		}
	}

	public float pixelSelectionRadius => selectionRadius * pixelRadius;

	public Vector2 pixelSelectionRadiusRange
	{
		get
		{
			return new Vector2(selectionRadiusRange.x * pixelRadius, selectionRadiusRange.y * pixelRadius);
		}
		set
		{
			selectionRadiusRange = new Vector2(value.x / pixelRadius, value.y / pixelRadius);
		}
	}

	public Vector2 pixelSelectionVisualRadiusRange
	{
		get
		{
			return new Vector2(selectionVisualRadiusRange.x * pixelRadius, selectionVisualRadiusRange.y * pixelRadius);
		}
		set
		{
			selectionVisualRadiusRange = new Vector2(value.x / pixelRadius, value.y / pixelRadius);
		}
	}

	private bool spawnsSubMenu => containerPiePopulator != null && containerPiePopulator.spawnsSubMenu;

	public bool IsCreationWaiting
	{
		get
		{
			bool result = false;
			if (actionInputEvent == InputEvent.Release)
			{
				result = isMouseDown && creationTime != -1f;
			}
			return result;
		}
	}

	public bool IsCreationDue => outIsCreationDue = IsCreationWaiting && Time.unscaledTime >= creationTime;

	public bool isActionWaiting
	{
		get
		{
			bool result = false;
			if (actionInputEvent == InputEvent.Release)
			{
				result = isMouseDown && actionTime != -1f;
			}
			return result;
		}
	}

	public bool isActionDue => outIsActionDue = isActionWaiting && Time.unscaledTime >= actionTime;

	public bool isHeldActionWaiting
	{
		get
		{
			bool result = false;
			if (heldItemIdx != -1 && heldActionTime != -1f)
			{
				result = true;
			}
			return result;
		}
	}

	public bool IsHeldActionDue => isHeldActionWaiting && Time.unscaledTime >= heldActionTime;

	public bool isDebugMenu => false;

	[ContextMenu("BuildAndPopulate")]
	public bool BuildAndPopulate()
	{
		if (containerPiePopulator != null)
		{
			return containerPiePopulator.BuildAndPopulate();
		}
		return false;
	}

	[ContextMenu("Apply")]
	public void Apply()
	{
		if (containerPiePopulator != null)
		{
			containerPiePopulator.Apply();
		}
	}

	[ContextMenu("Init")]
	public void Init()
	{
		if (isDebug)
		{
			logger.Info(base.name + ".Init()");
		}
		radialLayoutElements.Clear();
		foreach (Transform item in base.transform)
		{
			if (!item.gameObject.activeSelf)
			{
				continue;
			}
			RadialLayoutElement component = item.gameObject.GetComponent<RadialLayoutElement>();
			if (!(component == null))
			{
				if (component.GetType() == typeof(RadialLayoutElementCore))
				{
					coreRadialLayoutElement = component as RadialLayoutElementCore;
				}
				radialLayoutElements.Add(component);
				component.Init(this);
			}
		}
		InitStates();
		UpdateAll();
	}

	private void InitStates()
	{
		if (isDebug)
		{
			logger.Info(base.name + ".InitStates()");
		}
		for (int i = 0; i < radialLayoutElements.Count; i++)
		{
			RadialLayoutElement radialLayoutElement = radialLayoutElements[i];
			if (!(radialLayoutElement == null))
			{
				radialLayoutElement.isHover = false;
				radialLayoutElement.isTrueHover = false;
				radialLayoutElement.isSelection = false;
				radialLayoutElement.isDown = false;
				radialLayoutElement.isDrag = false;
				radialLayoutElement.isDragIn = false;
				radialLayoutElement.isDragOut = false;
			}
		}
	}

	[ContextMenu("UpdateTransforms")]
	public void UpdateTransforms()
	{
		UpdateTransforms(null);
	}

	public void UpdateTransforms(RadialLayoutElement targetRadialLayoutElement)
	{
		if (isDebug)
		{
			logger.Info(base.name + ".UpdateTransforms()");
		}
		for (int i = 0; i < radialLayoutElements.Count; i++)
		{
			RadialLayoutElement radialLayoutElement = radialLayoutElements[i];
			if (!(radialLayoutElement == null) && (!(targetRadialLayoutElement != null) || !(radialLayoutElement != targetRadialLayoutElement)))
			{
				radialLayoutElement.UpdateTransform();
				radialLayoutElement.UpdateBgTransform();
			}
		}
	}

	[ContextMenu("UpdateSettings")]
	public void UpdateSettings()
	{
		UpdateSettings(null);
	}

	public void UpdateSettings(RadialLayoutElement targetRadialLayoutElement)
	{
		if (isDebug)
		{
			logger.Info(base.name + ".UpdateSettings()");
		}
		if (radialLayoutElements.Count == 0)
		{
			return;
		}
		float num = 3f;
		float num2 = 2f;
		float num3 = 0f;
		int num4 = 0;
		for (int i = 0; i < radialLayoutElements.Count; i++)
		{
			RadialLayoutElement radialLayoutElement = radialLayoutElements[i];
			if (!(radialLayoutElement == null) && !(radialLayoutElement == coreRadialLayoutElement))
			{
				float num5 = radialLayoutElement.weight;
				if (testSpreadRangeB)
				{
					num5 = (((float)i != num) ? num5 : num2);
				}
				num3 += num5;
				num4++;
			}
		}
		float num6 = 0f;
		float num7 = baseAngleSpreadRange.y - baseAngleSpreadRange.x;
		float num8 = num7 / (float)num4;
		if (containerPiePopulator.isSubMenu && containerPiePopulator.superMenu != null && containerPiePopulator.superMenu.subMenuGameItemIdx != -1 && containerPiePopulator.superMenu.subMenuGameItemIdx > 0 && containerPiePopulator.superMenu.subMenuGameItemIdx < containerPiePopulator.superMenu.radialLayoutGroup.radialLayoutElements.Count - 1)
		{
			rawBaseAngleOffset = containerPiePopulator.superMenu.radialLayoutGroup.radialLayoutElements[containerPiePopulator.superMenu.subMenuGameItemIdx].angleOffsetRaw - 1f / 64f + containerPiePopulator.superMenu.radialLayoutGroup.rawBaseAngleOffset;
		}
		float num9 = GetLowestBaseSize() * 0.5f;
		float num10 = num9 * 2f * (float)Math.PI;
		float num11 = num10 / (float)testElemCount;
		float num12 = rawBaseSize * defaultRadiusRange.y * 0.5f;
		float num13 = num12 * 2f * (float)Math.PI;
		float num14 = num13 / (float)num4;
		float num15 = num11 / num14;
		num15 = 1f;
		for (int j = 0; j < radialLayoutElements.Count; j++)
		{
			RadialLayoutElement radialLayoutElement2 = radialLayoutElements[j];
			if (radialLayoutElement2 == null)
			{
				continue;
			}
			if (radialLayoutElement2 == coreRadialLayoutElement)
			{
				coreRadialLayoutElement.UpdateCoreRadius();
				coreRadialLayoutElement.UpdateCoreThickness();
				coreRadialLayoutElement.UpdateCoreWidth();
				continue;
			}
			float num16 = radialLayoutElement2.weight;
			if (testSpreadRangeB)
			{
				num16 = (((float)j != num) ? num16 : num2);
			}
			float num17 = num16 / num3;
			float num18 = num17 * (float)num4;
			float num19 = num8 * num18;
			float num20 = num19;
			float num21 = num20 * 0.5f;
			num6 += num21;
			float num22 = num6;
			num6 += num21;
			num20 *= num15;
			num22 += rawBaseAngleOffset;
			if ((!(targetRadialLayoutElement != null) || !(radialLayoutElement2 != targetRadialLayoutElement)) && !(radialLayoutElement2.bgMat == null) && !(radialLayoutElement2.bgMat == Graphic.defaultGraphicMaterial))
			{
				radialLayoutElement2.baseAngleOffset = baseAngleOffset + baseAngleSpreadRange.x;
				radialLayoutElement2.angleSpread = num20;
				radialLayoutElement2.angleOffset = num22;
				radialLayoutElement2.radiusInner = defaultVisualRadiusRange.x;
				radialLayoutElement2.radiusOuter = defaultVisualRadiusRange.y;
				radialLayoutElement2.alphaFactor = ((!isFrozen || !isUsingFrozen || isDebugMenu) ? 1f : 0.5f);
			}
		}
	}

	[ContextMenu("UpdateHovers")]
	public void UpdateHovers()
	{
		if (isDebug)
		{
			logger.Info(base.name + ".UpdateHovers()");
		}
		if (isHidden)
		{
			hoverIdx = -1;
			SetHeldItemIdx(hoverIdx);
			return;
		}
		if (radialLayoutElements.Count == 0)
		{
			hoverIdx = -1;
			SetHeldItemIdx(hoverIdx);
			return;
		}
		bool flag = false;
		if (isMouseDown && isMouseDownPreventUnhover)
		{
			flag = true;
		}
		Vector2 pos = mousePos - baseOffsetPos;
		float d = Mathf.Sqrt(Mathf.Pow(pos.x, 2f) + Mathf.Pow(pos.y, 2f));
		int num = (isMouseDown ? hoverIdx : (-1));
		int num2 = num;
		for (int i = 0; i < radialLayoutElements.Count; i++)
		{
			RadialLayoutElement radialLayoutElement = radialLayoutElements[i];
			if (!(radialLayoutElement == null))
			{
				bool flag2 = i == num;
				bool flag3 = radialLayoutElement.HitTest(pos, d);
				if (flag3 != flag2 && flag3)
				{
					num2 = i;
					break;
				}
			}
		}
		hoverIdx = num2;
		SetHeldItemIdx(hoverIdx);
		for (int j = 0; j < radialLayoutElements.Count; j++)
		{
			RadialLayoutElement radialLayoutElement2 = radialLayoutElements[j];
			if (radialLayoutElement2 == null)
			{
				continue;
			}
			bool flag4 = j == num;
			bool flag5 = (radialLayoutElement2.isHover = j == num2);
			if (flag5 != flag4)
			{
				if (flag5 && (!isFrozen || !radialLayoutElement2.isFrozen))
				{
					radialLayoutElement2.OnMouseEnter(flag);
				}
				else
				{
					radialLayoutElement2.OnMouseExit(flag);
				}
			}
		}
	}

	public void ClearHover()
	{
		hoverIdx = -1;
		SetHeldItemIdx(hoverIdx);
	}

	public void SetHovers(bool flag)
	{
		for (int i = 0; i < radialLayoutElements.Count; i++)
		{
			RadialLayoutElement radialLayoutElement = radialLayoutElements[i];
			if (!(radialLayoutElement == null))
			{
				radialLayoutElement.isHover = flag;
			}
		}
	}

	public void SetSelections(bool flag)
	{
		for (int i = 0; i < radialLayoutElements.Count; i++)
		{
			RadialLayoutElement radialLayoutElement = radialLayoutElements[i];
			if (!(radialLayoutElement == null))
			{
				radialLayoutElement.InitBgRawImage();
				radialLayoutElement.ToggleSelection(flag);
			}
		}
	}

	public bool ApplyInterpolators()
	{
		bool flag = true;
		foreach (RadialLayoutElement radialLayoutElement in radialLayoutElements)
		{
			int num = ApplyInterpolator(radialLayoutElement);
			flag = num == -1 && flag;
		}
		return flag;
	}

	public int ApplyInterpolator(RadialLayoutElement targetRadialLayoutElement)
	{
		if (targetRadialLayoutElement == null)
		{
			return 0;
		}
		ContaineePie containeePie = targetRadialLayoutElement.containee as ContaineePie;
		if (containeePie == null)
		{
			return 1;
		}
		InterpolatorTargetable interpolatorTargetable = containeePie.interpolatorTargetable;
		if (interpolatorTargetable == null)
		{
			return 2;
		}
		PropertyDriveByInterpolator component = interpolatorTargetable.gameObject.GetComponent<PropertyDriveByInterpolator>();
		if (component == null)
		{
			return 4;
		}
		component.Update();
		return -1;
	}

	public void SetBaseOffsetPos(Vector2 pos)
	{
		pos = GetSafeBaseOffsetPos(pos);
		if (baseOffsetPos != pos)
		{
			baseOffsetPos = pos;
			UpdateAll();
		}
	}

	public float GetGreatestBaseSize()
	{
		float num = rawBaseSize * defaultRadiusRange.y;
		if (spawnsSubMenu && containerPiePopulator.subMenu != null)
		{
			float greatestBaseSize = containerPiePopulator.subMenu.radialLayoutGroup.GetGreatestBaseSize();
			num = ((!(greatestBaseSize > num)) ? num : greatestBaseSize);
		}
		return num;
	}

	public float GetLowestBaseSize()
	{
		float num = rawBaseSize * defaultRadiusRange.y;
		if (containerPiePopulator.superMenu != null)
		{
			float lowestBaseSize = containerPiePopulator.superMenu.radialLayoutGroup.GetLowestBaseSize();
			num = ((!(lowestBaseSize < num)) ? num : lowestBaseSize);
		}
		return num;
	}

	public float GetLowestInnerBaseSize()
	{
		float num = rawBaseSize * defaultRadiusRange.x;
		if (containerPiePopulator.superMenu != null)
		{
			float lowestInnerBaseSize = containerPiePopulator.superMenu.radialLayoutGroup.GetLowestInnerBaseSize();
			num = ((!(lowestInnerBaseSize < num)) ? num : lowestInnerBaseSize);
		}
		return num;
	}

	public Vector2 GetSafeBaseOffsetPos(Vector2 pos)
	{
		float num = Screen.width;
		float num2 = Screen.height;
		float greatestBaseSize = GetGreatestBaseSize();
		float num3 = pos.x - greatestBaseSize;
		float num4 = pos.y - greatestBaseSize;
		float num5 = pos.x + greatestBaseSize;
		float num6 = pos.y + greatestBaseSize;
		float num7 = 0f;
		num7 = ((!(num3 < 0f)) ? num7 : (0f - num3));
		num7 = ((!(num5 > num)) ? num7 : (num - num5));
		float num8 = 0f;
		num8 = ((!(num4 < 0f)) ? num8 : (0f - num4));
		num8 = ((!(num6 > num2)) ? num8 : (num2 - num6));
		pos.x += num7;
		pos.y += num8;
		return pos;
	}

	private void RemoveWhiteButton()
	{
		for (int i = 0; i < radialLayoutElements.Count; i++)
		{
			RadialLayoutElement radialLayoutElement = radialLayoutElements[i];
			if (!(radialLayoutElement == null) && radialLayoutElement.bgMat.color == Color.white)
			{
				radialLayoutElement.bgMat.color = defaultColor;
			}
		}
	}

	public void UpdateAll()
	{
		if (isDebugB)
		{
			logger.Info(Mathf.Round(Time.unscaledTime * 100f) / 100f + " :: " + base.name + "[RadialLayoutGroup].UpdateAll()\n");
		}
		UpdateSettings();
		UpdateHovers();
		UpdateTransforms();
		UpdateArrow();
		RemoveWhiteButton();
	}

	public void UpdateAt(RadialLayoutElement targetRadialLayoutElement)
	{
		UpdateSettings(targetRadialLayoutElement);
		UpdateTransforms(targetRadialLayoutElement);
	}

	public void UpdatePickingPlane()
	{
		if (!(pickingPlane == null))
		{
			if (!isHidden)
			{
				pickingPlane.gameObject.SetActive(value: true);
				ClickCatch(flag: true);
			}
			else
			{
				pickingPlane.gameObject.SetActive(value: false);
				ClickCatch(flag: false);
			}
		}
	}

	private void OnRectTransformDimensionsChange()
	{
		UpdateSettings();
	}

	private void Start()
	{
		isMouseDown = false;
		UpdateAll();
		GameSignals.Connect(InputModeController.InputModeChanged, OnInputModeChanged);
	}

	private void OnInputModeChanged(InputMode mode)
	{
		ToggleSelf(flag: false, force: true);
	}

	private void OnEnable()
	{
		isMouseDown = false;
		UpdateAll();
	}

	private void UpdateCoreMaterials()
	{
		if (!(coreRadialLayoutElement == null))
		{
			coreRadialLayoutElement.GetDefaultMaterialValues();
		}
	}

	private void Update()
	{
		HandleInput();
		if (Time.timeScale == 0f)
		{
			return;
		}
		if (isDebugMenu)
		{
			UpdateCoreMaterials();
		}
		if (isUsingFrozen && isFrozen)
		{
			UpdateAll();
			return;
		}
		wasFrozen = false;
		UpdateAll();
		if (!IsCreationWaiting && !IsCreationDue && OpenProcessProgression != -1f && !containerPiePopulator.isSubMenu)
		{
			OpenProcessProgression = -1f;
			OpenProgressChanged.Invoke();
		}
		if (IsCreationWaiting && !IsCreationDue && !containerPiePopulator.isSubMenu)
		{
			OpenProcessProgression = ComputeOpenProgress();
			OpenProgressChanged.Invoke();
		}
		if (IsCreationDue)
		{
			Create();
			if (!containerPiePopulator.isSubMenu)
			{
				OpenProcessProgression = -1f;
				OpenProgressChanged.Invoke();
			}
		}
		else if (isCtrlSchemeHold && IsHeldActionDue)
		{
			radialLayoutElements[heldItemIdx].OnPointerHold();
			ResetHeldActionTimeAndIndex();
		}
	}

	private float ComputeOpenProgress()
	{
		float result = 0f;
		if (creationTime != -1f)
		{
			result = creationInitTime - (creationTime - Time.unscaledTime);
			result /= creationInitTime;
			result = Mathf.Clamp(result, 0f, 1f);
		}
		return result;
	}

	public void ShowSelf()
	{
		ToggleSelf(flag: true);
	}

	public void HideSelf()
	{
		ToggleSelf(flag: false);
	}

	public void ToggleSelf(bool flag, bool force = false)
	{
		if (flag)
		{
			TryOnOpen();
		}
		else
		{
			TryOnClose(force);
		}
		if (force)
		{
			isFrozen = false;
			wasFrozen = false;
		}
		isHidden = !flag;
		for (int i = 0; i < radialLayoutElements.Count; i++)
		{
			RadialLayoutElement radialLayoutElement = radialLayoutElements[i];
			if (!(radialLayoutElement == null))
			{
				radialLayoutElement.ToggleSelf(flag);
			}
		}
		UpdateAll();
	}

	public void ToggleFreeze(bool flag)
	{
		isFrozen = flag;
		wasFrozen = ((!flag) ? wasFrozen : flag);
		for (int i = 0; i < radialLayoutElements.Count; i++)
		{
			RadialLayoutElement radialLayoutElement = radialLayoutElements[i];
			if (!(radialLayoutElement == null))
			{
				radialLayoutElement.ToggleFreeze(flag);
			}
		}
	}

	private void TryOnOpen()
	{
		if (isUsingFrozen && !isFrozen && isHidden && !ClockConfig.Paused)
		{
			if (!containerPiePopulator.isSubMenu)
			{
				OnOpen();
			}
			else
			{
				OnSubOpen();
			}
		}
	}

	private void TryOnClose(bool force = false)
	{
		if (force || (!isFrozen && isUsingFrozen && !isHidden && !ClockConfig.Paused))
		{
			if (!containerPiePopulator.isSubMenu)
			{
				OnClose();
			}
			else
			{
				OnSubClose();
			}
		}
	}

	private void OnSubClose()
	{
		SoundEngine.Play(MenuSound.GetSoundLabel(MenuSound.Prefix.Pie, MenuSound.Suffix.Collapse));
	}

	private void OnOpen()
	{
		Curseur.instance.disableNudgeByPieMenu = true;
		pickingPlane.gameObject.SetActive(value: true);
		ClickCatch(flag: true);
		PieMenuIsOpen = true;
		PieMenuOnShow.Invoke();
		SoundEngine.Play(MenuSound.GetSoundLabel(MenuSound.Prefix.Pie, MenuSound.Suffix.Open));
	}

	private void OnSubOpen()
	{
		SoundEngine.Play(MenuSound.GetSoundLabel(MenuSound.Prefix.Pie, MenuSound.Suffix.Expand));
	}

	private void OnClose()
	{
		Curseur.instance.disableNudgeByPieMenu = false;
		pickingPlane.gameObject.SetActive(value: false);
		ClickCatch(flag: false);
		PieMenuIsOpen = false;
		PieMenuOnHide.Invoke();
		SoundEngine.Play(MenuSound.GetSoundLabel(MenuSound.Prefix.Pie, MenuSound.Suffix.Close));
	}

	private void ClickCatch(bool flag)
	{
		containerPiePopulator.canvasGroup.blocksRaycasts = flag;
		containerPiePopulator.canvasGroup.interactable = flag;
	}

	private void Create()
	{
		HasContent = BuildAndPopulate();
		if (HasContent)
		{
			ResetCreationTime();
			containerPiePopulator.ResetToggleCache();
			Init();
			ApplyInterpolators();
			UpdateAll();
			ShowSelf();
		}
	}

	public void SetCreationTime()
	{
		if (!containerPiePopulator.isSubMenu)
		{
			creationTime = Time.unscaledTime + creationInitTime;
			SetBaseOffsetPos(mousePos);
		}
	}

	public void ResetCreationTime()
	{
		creationTime = -1f;
	}

	public void SetActionTime()
	{
		actionTime = Time.unscaledTime + actionInitTime;
	}

	public void ResetActionTime()
	{
		actionTime = -1f;
	}

	public void SetHeldActionTime()
	{
		heldActionTime = Time.unscaledTime + heldActionInitTime;
	}

	public void ResetHeldActionTimeAndIndex()
	{
		heldItemIdx = -1;
		heldActionTime = -1f;
	}

	private void SetHeldItemIdx(int dstHeldItemIdx)
	{
		if (heldItemIdx != dstHeldItemIdx)
		{
			ResetHeldActionTimeAndIndex();
			if (dstHeldItemIdx != -1)
			{
				SetHeldActionTime();
			}
			heldItemIdx = dstHeldItemIdx;
		}
	}

	private Inputs GetInput()
	{
		Inputs inputs = Inputs.None;
		if (Time.timeScale == 0f)
		{
			return Inputs.None;
		}
		if (InputModeController.GetCurrentMode() == InputMode.menu)
		{
			return Inputs.None;
		}
		if (Input.GetMouseButtonDown((int)actionMouseButton))
		{
			inputs |= Inputs.RightDown;
		}
		if (Input.GetMouseButtonDown((int)altActionMouseButton))
		{
			inputs |= Inputs.LeftDown;
		}
		if (Input.GetMouseButton((int)actionMouseButton))
		{
			inputs |= Inputs.RightPressed;
		}
		if (Input.GetMouseButton((int)altActionMouseButton))
		{
			inputs |= Inputs.LeftPressed;
		}
		if (Input.GetMouseButtonUp((int)actionMouseButton))
		{
			inputs |= Inputs.RightUp;
		}
		if (Input.GetMouseButtonUp((int)altActionMouseButton))
		{
			inputs |= Inputs.LeftUp;
		}
		if (!Input.GetMouseButton((int)actionMouseButton) && isMouseDown)
		{
			inputs |= Inputs.OutsideScreen;
		}
		if (Input.GetKey(KeyCode.Space))
		{
			inputs |= Inputs.SpaceDown;
		}
		return inputs;
	}

	private void FakeInputsClose()
	{
		mousePos = baseOffsetPos;
		UpdateAll();
		MouseRightUp(InverseMousePos(baseOffsetPos));
		UpdateAll();
	}

	private void HandleInput()
	{
		Inputs input = GetInput();
		if (input == Inputs.None)
		{
			return;
		}
		if (input.HasFlag(Inputs.SpaceDown))
		{
			SpacebarDown();
			return;
		}
		mousePos = Input.mousePosition;
		if (input.HasFlag(Inputs.RightDown))
		{
			MouseRightDown(mousePos);
		}
		if (input.HasFlag(Inputs.RightUp))
		{
			MouseRightUp(mousePos);
		}
		if (input.HasFlag(Inputs.RightPressed))
		{
			MouseRightDrag(mousePos);
		}
		if (input.HasFlag(Inputs.LeftUp))
		{
			MouseLeftUp(mousePos);
		}
		if (input.HasFlag(Inputs.OutsideScreen))
		{
			FakeInputsClose();
		}
	}

	private void SpacebarDown()
	{
		ResetCreationTime();
	}

	private void GetTarget()
	{
		Project currentHovered = UIGameObserver.projects.CurrentHovered;
		if (currentHovered != null)
		{
			pieMenuTarget = currentHovered.gameObject;
		}
		else
		{
			pieMenuTarget = Picking.Instance.PieMenuTarget;
		}
	}

	private void MouseRightDown(Vector2 pos)
	{
		GetTarget();
		SetCreationTime();
		mouseDownPos = pos;
		isMouseDown = true;
		ApplyToHoveredElement(delegate(RadialLayoutElement r, float f)
		{
			r.OnMouseDown(f);
		}, pos);
	}

	private void MouseRightUp(Vector2 pos)
	{
		isMouseDown = false;
		ApplyToHoveredElement(delegate(RadialLayoutElement r, float f)
		{
			r.OnMouseUp(f);
		}, pos);
	}

	private void MouseLeftUp(Vector2 pos)
	{
		isMouseDown = false;
		ApplyToHoveredElement(delegate(RadialLayoutElement r, float f)
		{
			r.OnLeftMouseUp(f);
		}, pos);
	}

	private void MouseRightDrag(Vector2 pos)
	{
		mouseDragPos = pos;
		isMouseDown = true;
		if (IsCreationWaiting && Time.timeScale != 0f)
		{
			Vector2 vector = mouseDragPos - mouseDownPos;
			float num = Mathf.Sqrt(Mathf.Pow(vector.x, 2f) + Mathf.Pow(vector.y, 2f));
			if (num > defaultRadiusRange.x * baseSize)
			{
				creationTime = -1f;
				Create();
				bool force = true;
				OnWarpToHovered(force);
			}
		}
		ApplyToHoveredElement(delegate(RadialLayoutElement r, float f)
		{
			r.OnMouseDrag(f);
		}, pos);
	}

	private void ApplyToHoveredElement(Action<RadialLayoutElement, float> action, Vector2 position)
	{
		if (isHidden)
		{
			return;
		}
		float arg = AngleFromPosition(position);
		for (int i = 0; i < radialLayoutElements.Count; i++)
		{
			RadialLayoutElement radialLayoutElement = radialLayoutElements[i];
			if (!(radialLayoutElement == null) && !radialLayoutElement.isFrozen && (radialLayoutElement.isTrueHover || radialLayoutElement.isHover))
			{
				action(radialLayoutElement, arg);
			}
		}
	}

	private float GetAngle(Vector2 pos, bool usingOffsets)
	{
		float num = Mathf.Atan2(pos.y, pos.x) / ((float)Math.PI * 2f);
		if (usingOffsets)
		{
			num += baseAngleOffset;
			num += baseAngleSpreadRange.x;
		}
		num = (10000f + num) % 1f;
		num = 1f - num;
		num += 0.5f;
		return (10000f + num) % 1f;
	}

	private float AngleFromPosition(Vector2 pos)
	{
		return GetAngle(pos - baseOffsetPos, usingOffsets: true);
	}

	private Vector2 InverseMousePos(Vector2 pos)
	{
		pos.y = (float)Screen.height - pos.y;
		return pos;
	}

	public void OnWarpToHovered(bool force)
	{
		if (isDebug)
		{
			logger.Info(base.name + ".OnWarpToHovered()\n");
		}
		if (isHidden && !force)
		{
			return;
		}
		Vector2 pos = mousePos - baseOffsetPos;
		float d = Mathf.Sqrt(Mathf.Pow(pos.x, 2f) + Mathf.Pow(pos.y, 2f));
		for (int i = 0; i < radialLayoutElements.Count; i++)
		{
			RadialLayoutElement radialLayoutElement = radialLayoutElements[i];
			if (!(radialLayoutElement == null) && !(radialLayoutElement == coreRadialLayoutElement) && radialLayoutElement.HitTest(pos, d))
			{
				radialLayoutElement.OnWarpToHovered();
			}
		}
	}

	public void OnSelectionExclusive(RadialLayoutElement targetRadialLayoutElement, bool flag)
	{
		if (isDebug)
		{
			logger.Info(string.Concat(base.name, ".OnSelectionLock(", targetRadialLayoutElement, ", ", flag, ")\n", mousePos));
		}
		if (isHidden)
		{
			return;
		}
		if (!flag)
		{
			targetRadialLayoutElement.OnSelectionExclusive(flag: false);
			return;
		}
		for (int i = 0; i < radialLayoutElements.Count; i++)
		{
			RadialLayoutElement radialLayoutElement = radialLayoutElements[i];
			if (!(radialLayoutElement == null))
			{
				radialLayoutElement.OnSelectionExclusive(radialLayoutElement == targetRadialLayoutElement);
			}
		}
	}

	public void OnToggle(RadialLayoutElementToggle targetRadialLayoutElementToggle, bool flag)
	{
		if (isDebug)
		{
			logger.Info(string.Concat(base.name, ".OnToggle(", targetRadialLayoutElementToggle, ", ", flag, ")\n", mousePos));
		}
		if (isHidden)
		{
			return;
		}
		for (int i = 0; i < radialLayoutElements.Count; i++)
		{
			RadialLayoutElement radialLayoutElement = radialLayoutElements[i];
			if (!(radialLayoutElement == null))
			{
				RadialLayoutElementToggle radialLayoutElementToggle = radialLayoutElement as RadialLayoutElementToggle;
				if (!(radialLayoutElementToggle == null))
				{
					bool flag2 = radialLayoutElementToggle == targetRadialLayoutElementToggle;
					radialLayoutElementToggle.OnToggle(flag2 && flag);
				}
			}
		}
	}

	public void OnUpdateTooltipText(RadialLayoutElement radialLayoutElement, bool showFlag)
	{
		containerPiePopulator.UpdateTooltipText((!showFlag) ? null : radialLayoutElement);
	}

	public void UpdateArrow()
	{
		if (arrowRawImage == null || arrowMaterial == null)
		{
			return;
		}
		Vector2 pos = mousePos - baseOffsetPos;
		float num = Mathf.Sqrt(Mathf.Pow(pos.x, 2f) + Mathf.Pow(pos.y, 2f));
		bool flag = num <= defaultRadiusRange.x * baseSize;
		if (isCtrlSchemeHold)
		{
			flag = num <= GetLowestInnerBaseSize();
		}
		bool flag2 = isFrozen && isUsingFrozen;
		bool flag3 = isHidden || flag || flag2;
		GameObject gameObject = arrowRawImage.gameObject;
		gameObject.SetActive(!flag3);
		if (!isHidden)
		{
			float size = baseSize * 2f * arrowMagicFactor;
			RectTransform rectTransform = gameObject.transform as RectTransform;
			rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
			rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
			rectTransform.position = baseOffsetPos;
			float angle = GetAngle(pos, usingOffsets: false);
			Vector3 localEulerAngles = rectTransform.localEulerAngles;
			localEulerAngles.z = (0f - angle) * 360f;
			rectTransform.localEulerAngles = localEulerAngles;
			Material material = arrowRawImage.material;
			if (material == null || material != pieArrowMat)
			{
				material = (pieArrowMat = new Material(arrowMaterial));
				arrowRawImage.material = material;
			}
		}
	}
}
