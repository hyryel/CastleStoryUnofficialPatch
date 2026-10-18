using System;
using System.Collections.Generic;
using Brix.External.Factories;
using Brix.Game.Components;
using Brix.Lifecycle.Pooling;
using Brix.Lua;
using Brix.Util;
using Smooth.Slinq;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.NewUI.OverheadDisplay;

public class OverheadDisplay : UpdatableGameComponent<OverheadDisplay>
{
	public class OverheadSpawner<T> where T : Overhead
	{
		private T instance;

		private Factory.AssetKey key;

		private OverheadDisplay owner;

		public T Get => instance;

		public bool IsSpawned => instance != null;

		public OverheadSpawner(Factory.AssetKey key, OverheadDisplay owner)
		{
			this.key = key;
			this.owner = owner;
		}

		public void Spawn()
		{
			ObjectPoolSingleton.RequestComponent<T>(out instance, key);
			instance.transform.SetParent(owner.EnabledContent, worldPositionStays: true);
			owner.RequestedLines.Add(instance);
			instance.overhead = owner;
			instance.gameObject.SetActive(value: true);
			instance.transform.SetSiblingIndex(instance.PreferedIndex);
		}

		public void Release()
		{
			if (instance != null)
			{
				ObjectPoolSingleton.Release(instance.gameObject);
				owner.RequestedLines.Remove(instance);
				instance = (T)null;
			}
		}
	}

	private enum ShowMode
	{
		hidden,
		showAll,
		showForcing
	}

	public static readonly Color allyColor = new Color(0f, 25f / 51f, 1f);

	public static readonly Color enemyColor = new Color(1f, 0f, 0.2509804f);

	public static readonly Color ownedColor = new Color(1f, 0.77254903f, 2f / 51f);

	[SerializeField]
	private BehindCameraDetector cameraDetector;

	public OverheadSpawner<FreeAgentsIssues> Issues;

	public OverheadSpawner<BigIcon> BigIcon;

	public OverheadSpawner<ThoughtBubble> ThoughtBubble;

	public OverheadSpawner<ItemCounts> ItemCounts;

	public OverheadSpawner<OrderQueue> OrderQueue;

	public OverheadSpawner<NameTag> NameTag;

	private List<Overhead> RequestedLines = new List<Overhead>();

	[SerializeField]
	private Transform EnabledContent;

	[SerializeField]
	private Transform Content;

	[SerializeField]
	private Transform disabledContent;

	[SerializeField]
	private float offsetFromArrow;

	[SerializeField]
	private float lineHeight;

	[SerializeField]
	private float lineSpacing;

	[SerializeField]
	private Image Arrow;

	[SerializeField]
	private Canvas ArrowCanvas;

	[SerializeField]
	private Canvas MainCanvas;

	[SerializeField]
	private TransformConstraint transformConstraint;

	private List<Overhead> EnabledOverheadLines = new List<Overhead>();

	private bool initialized;

	public bool SortBasedOnCamera;

	public bool hideIfFar = true;

	public bool hideForCinematics = true;

	public bool hideInFog = true;

	public bool allowChildrenForceShow = true;

	public bool showOnHover = true;

	public bool showOnSelect = true;

	public bool showByDefault;

	public bool showOnButton = true;

	[LuaDrivable("Cinematic")]
	public static bool Cinematic = false;

	public bool Hovered;

	public bool Selected;

	public bool InFog;

	public bool ShowAllButton;

	public Transform _pivot;

	private bool IsCloseEnough;

	public Transform pivot
	{
		get
		{
			return _pivot;
		}
		set
		{
			_pivot = value;
			cameraDetector.pivot = value;
		}
	}

	private bool ChildrenForceShow => RequestedLines.Slinq().Any((Overhead l) => l.ForceShow && l.Show);

	public Color Color
	{
		set
		{
			if (BigIcon.IsSpawned)
			{
				BigIcon.Get.Color = value;
			}
			if ((bool)Arrow)
			{
				Arrow.color = value;
			}
		}
	}

	public override void Initialize()
	{
		base.Initialize();
		Issues = new OverheadSpawner<FreeAgentsIssues>(Brix.External.Factories.UI.IssuesFreeAgents, this);
		BigIcon = new OverheadSpawner<BigIcon>(Brix.External.Factories.UI.BigIcon, this);
		ThoughtBubble = new OverheadSpawner<ThoughtBubble>(Brix.External.Factories.UI.ThoughtBubble, this);
		ItemCounts = new OverheadSpawner<ItemCounts>(Brix.External.Factories.UI.ItemCount, this);
		OrderQueue = new OverheadSpawner<OrderQueue>(Brix.External.Factories.UI.OrderQueue, this);
		NameTag = new OverheadSpawner<NameTag>(Brix.External.Factories.UI.NameTagOverhead, this);
	}

	public void ShowBigIcon(bool value)
	{
		if (BigIcon != null)
		{
			BigIcon.Get.gameObject.SetActive(value);
		}
	}

	public void Init()
	{
		if (!initialized)
		{
			MainCanvas.overrideSorting = true;
			ArrowCanvas.overrideSorting = true;
			cameraDetector.CameBackIntoCamera.Connect(ShowOrHide);
			cameraDetector.WentBehindCamera.Connect(ShowOrHide);
			initialized = true;
		}
	}

	public void CheckCloseEnough()
	{
		bool flag = (Camera.main.transform.position - pivot.position).sqrMagnitude < Calcule.Sqr((float)Screen.height / 5f);
		if (flag != IsCloseEnough)
		{
			IsCloseEnough = flag;
			ShowOrHide();
		}
	}

	public void ShowOrHide()
	{
		ShowMode showMode = ShouldShow();
		Show(showMode != ShowMode.hidden);
		ShowLines(showMode);
	}

	private ShowMode ShouldShow()
	{
		return ((!hideIfFar || IsCloseEnough) && (!hideForCinematics || !Cinematic) && (!hideInFog || !InFog)) ? ((showByDefault || (showOnHover && Hovered) || (showOnSelect && Selected) || (showOnButton && ShowAllButton)) ? ShowMode.showAll : ((allowChildrenForceShow && ChildrenForceShow) ? ShowMode.showForcing : ShowMode.hidden)) : ShowMode.hidden;
	}

	private void Show(bool value)
	{
		CanvasGroup component = Content.GetComponent<CanvasGroup>();
		component.alpha = (value ? 1 : 0);
		component.interactable = value;
		transformConstraint.enabled = value;
	}

	private void ShowLines(ShowMode mode)
	{
		foreach (Overhead requestedLine in RequestedLines)
		{
			ShowLine(requestedLine, mode switch
			{
				ShowMode.showAll => requestedLine.Show, 
				ShowMode.showForcing => requestedLine.Show && requestedLine.ForceShow, 
				_ => false, 
			});
		}
	}

	public void ShowLine(Overhead line, bool wantShow)
	{
		bool flag = LineIsShowing(line);
		bool flag2 = !wantShow && flag;
		bool flag3 = wantShow && !flag;
		if (flag2)
		{
			line.MakeVisible(value: false);
			EnabledOverheadLines.Remove(line);
		}
		else if (flag3)
		{
			line.MakeVisible(value: true);
			int index = Mathf.Min(line.PreferedIndex, EnabledOverheadLines.Count);
			EnabledOverheadLines.Insert(index, line);
		}
		RepositionOverheads();
	}

	public void SetInteractible(bool value)
	{
		RequestedLines.Slinq().ForEach(delegate(Overhead l, bool v)
		{
			l.Interactible = v;
		}, value);
	}

	private void RepositionOverheads()
	{
		for (int i = 0; i < EnabledOverheadLines.Count; i++)
		{
			float height = (float)i * (lineHeight + lineSpacing) + lineSpacing + offsetFromArrow;
			PositionFromMidBottom(EnabledOverheadLines[i].transform, height);
			if (base.transform.hasChanged)
			{
				RectTransform rectTransform = (RectTransform)EnabledOverheadLines[i].transform;
				float num = Mathf.Round(rectTransform.sizeDelta.x);
				if (Math.Abs(num % 2f) > 0f)
				{
					num += 1f;
				}
				rectTransform.sizeDelta = new Vector2(num, rectTransform.sizeDelta.y);
				base.transform.hasChanged = false;
			}
		}
	}

	private void PositionFromMidBottom(Transform t, float height)
	{
		RectTransform rectTransform = t as RectTransform;
		rectTransform.anchorMin = new Vector2(0.5f, 0f);
		rectTransform.anchorMax = new Vector2(0.5f, 0f);
		rectTransform.anchoredPosition = new Vector2(0f, height);
	}

	public bool LineIsShowing(Overhead line)
	{
		CanvasGroup component = line.GetComponent<CanvasGroup>();
		return (bool)component && component.alpha > 0f;
	}

	protected override void BrixUpdate()
	{
		base.BrixUpdate();
		if (SortBasedOnCamera)
		{
			MainCanvas.sortingOrder = -(int)Mathf.Abs(Vector3.Distance(pivot.position, Camera.main.transform.position));
		}
	}

	public override void ResetComponent()
	{
		base.ResetComponent();
		CleanUp();
	}

	private void CleanUp()
	{
		if (NameTag.IsSpawned)
		{
			NameTag.Get.TextColor(ownedColor);
		}
		hideIfFar = true;
		hideForCinematics = true;
		hideInFog = true;
		allowChildrenForceShow = true;
		showOnHover = true;
		showOnSelect = true;
		showOnButton = true;
		showByDefault = false;
		Selected = false;
		Hovered = false;
		ShowAllButton = false;
		Cinematic = false;
		InFog = false;
		NameTag.Release();
		BigIcon.Release();
		ThoughtBubble.Release();
		ItemCounts.Release();
		OrderQueue.Release();
		Issues.Release();
		Color = ownedColor;
		EnabledOverheadLines.Clear();
		RequestedLines.Clear();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		CleanUp();
	}
}
