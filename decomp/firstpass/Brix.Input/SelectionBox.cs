using System.Collections.Generic;
using Brix.Common.Collections;
using Brix.Engine;
using Brix.Game;
using Brix.Game.Components;
using Brix.UI.GameSelector;
using Brix.Utils;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Input;

public class SelectionBox : MonoBehaviour
{
	private bool rectEnabled;

	private Vector3 rectSelectA = default(Vector3);

	private Vector3 rectSelectB = default(Vector3);

	private List<UISelectable> currentContent = new List<UISelectable>();

	private RectTransform _selectionBox;

	private SelectionTrackID selectionTrack;

	private SelectionType _exclusiveType;

	private bool exlusiveIn;

	public static SelectionBox instance { get; set; }

	public static bool active => instance.RectEnabled;

	public bool RectEnabled
	{
		get
		{
			return rectEnabled;
		}
		set
		{
			rectEnabled = value;
			CursorPicker.RectEnabledChanged.Invoke();
		}
	}

	private static Rect CurrentRect => new Rect(Mathf.Min(instance.rectSelectA.x, instance.rectSelectB.x), Mathf.Min(instance.rectSelectA.y, instance.rectSelectB.y), Mathf.Abs(instance.rectSelectA.x - instance.rectSelectB.x), Mathf.Abs(instance.rectSelectA.y - instance.rectSelectB.y));

	private static bool MovedEnough()
	{
		return Vector3.SqrMagnitude(instance.rectSelectA - instance.rectSelectB) > 10f;
	}

	public void Awake()
	{
		instance = this;
		_selectionBox = UIGameSelector.instance.SelectionBox.GetComponent<RectTransform>();
	}

	public static void BeginSelection(Vector3 position, SelectionTrackID track, SelectionType exclusiveType = SelectionType.None)
	{
		instance.rectSelectA = position;
		instance.rectSelectB = position;
		instance.selectionTrack = track;
		instance._exclusiveType = exclusiveType;
		instance.exlusiveIn = false;
		instance.RectEnabled = true;
		instance.RedrawSelectionBox();
	}

	public static void EndSelection()
	{
		instance.RectEnabled = false;
		instance.RedrawSelectionBox(reset: true);
		if (MovedEnough())
		{
			UIGameSelector.SelectWhatIsHovered();
		}
	}

	public static void EndSelectionRaw()
	{
		instance.RectEnabled = false;
		instance.RedrawSelectionBox(reset: true);
	}

	public static void CancelSelection()
	{
		UIGameSelector.UnhoverEverything();
		instance.RectEnabled = false;
		instance.RedrawSelectionBox(reset: true);
	}

	public static void UpdateAction(Vector3 nextPosition)
	{
		if (instance.RectEnabled)
		{
			instance.rectSelectB = nextPosition;
			List<UISelectable> selectables = CollectionPool.RequestList<UISelectable>();
			instance.CurrentBoxContent(ref selectables, instance.selectionTrack);
			HoverWhatWasAddedToBox(selectables);
			UnhoverWhatWentOutOfBox(instance.currentContent, selectables);
			UnhoverExludedStuff(selectables);
			instance.currentContent.Clear();
			selectables.Slinq().AddTo(instance.currentContent);
			selectables.Dispose();
			instance.RedrawSelectionBox();
		}
	}

	private static void HoverableBoxContent(ref List<UISelectable> toHover, List<UISelectable> nextBoxContent)
	{
		foreach (UISelectable item in (from selectable in nextBoxContent.Slinq()
			where (bool)selectable && !UIGameSelector.IsHovered(selectable)
			select selectable).NoLeak())
		{
			if (IsExcludedBasedOnCurrentSelection(item))
			{
				break;
			}
			if (item.IsHoverable())
			{
				toHover.Add(item);
			}
		}
	}

	private static bool IsExcludedBasedOnCurrentSelection(UISelectable selectable)
	{
		return instance.exlusiveIn && selectable.SelectionType != instance._exclusiveType;
	}

	private static bool EncounteredExclusive(List<UISelectable> currentContent)
	{
		if (currentContent.Slinq().Any((UISelectable s, SelectionType p) => s.SelectionType == p, instance._exclusiveType))
		{
			return true;
		}
		if (UIGameSelector.SomeIsSelected(instance._exclusiveType))
		{
			return true;
		}
		return false;
	}

	private static void UnhoverExludedStuff(List<UISelectable> content)
	{
		if (EncounteredExclusive(content))
		{
			UIGameSelector.UnhoverEverythingExcept(instance._exclusiveType);
		}
	}

	private static void HoverWhatWasAddedToBox(List<UISelectable> nextBoxContent)
	{
		List<UISelectable> toHover = CollectionPool.RequestList<UISelectable>();
		HoverableBoxContent(ref toHover, nextBoxContent);
		UIGameSelector.HoverMany(toHover);
		toHover.Dispose();
	}

	private static void UnhoverWhatWentOutOfBox(List<UISelectable> previousBoxContent, List<UISelectable> nextBoxContent)
	{
		List<UISelectable> list = CollectionPool.RequestList<UISelectable>();
		previousBoxContent.Slinq().Where((UISelectable s, List<UISelectable> p) => s != null && !p.Contains(s), nextBoxContent).ForEach(delegate(UISelectable s, List<UISelectable> p)
		{
			p.Add(s);
		}, list);
		UIGameSelector.UnhoverMany(list);
		list.Dispose();
	}

	private void CurrentBoxContent(ref List<UISelectable> selectables, SelectionTrackID track)
	{
		UIGameSelector.GetAllSelectables().Slinq().Where((UISelectable selectable, SelectionTrackID aTrack) => !selectable.IsNullOrReleased() && IsInsideSelectionBox(CurrentRect, selectable) && IsBoxSelectable(selectable) && IsInSelectableFaction(selectable) && UIGameSelector.IsInTrack(aTrack, selectable.SelectionType), track)
			.Take(UIGameSelector.instance.MaximumHoveredOrSelectedCount)
			.AddTo(selectables);
	}

	private static bool IsInSelectableFaction(UISelectable selectable)
	{
		if (selectable.faction == Faction.DefaultNeutral)
		{
			return true;
		}
		if (InputModeController.GetCurrentMode() != InputMode.storage)
		{
			return selectable.faction == User.LocalUser.faction;
		}
		return Affiliation.IsAllied(User.LocalUser.DefaultAlliedFactionGO, selectable.gameObject);
	}

	private static bool IsInsideSelectionBox(Rect rect, UISelectable selectable)
	{
		return rect.Contains(Camera.main.WorldToScreenPoint(selectable.transform.position), allowInverse: true);
	}

	private static bool IsBoxSelectable(UISelectable selectable)
	{
		if (!selectable.IsSelectable())
		{
			return false;
		}
		if (!selectable.IsHoverable())
		{
			return false;
		}
		return true;
	}

	public void RedrawSelectionBox(bool reset = false)
	{
		if (!(_selectionBox != null))
		{
			return;
		}
		if (reset)
		{
			_selectionBox.position = Vector3.zero;
			_selectionBox.sizeDelta = Vector2.zero;
			float num = 1f / Neo.canvasScale;
			Vector3 localScale = _selectionBox.localScale;
			localScale.x = num;
			localScale.y = num;
			localScale.z = num;
			_selectionBox.localScale = localScale;
			if (_selectionBox.gameObject.activeSelf)
			{
				_selectionBox.gameObject.SetActive(value: false);
			}
		}
		else if (RectEnabled)
		{
			if (!_selectionBox.gameObject.activeSelf)
			{
				_selectionBox.gameObject.SetActive(value: true);
			}
			_selectionBox.position = new Vector3(Mathf.Min(rectSelectA.x, rectSelectB.x), Mathf.Min(rectSelectA.y, rectSelectB.y), 0f);
			_selectionBox.sizeDelta = new Vector2(Mathf.Abs(rectSelectA.x - rectSelectB.x), Mathf.Abs(rectSelectA.y - rectSelectB.y));
			float num2 = 1f / Neo.canvasScale;
			Vector3 localScale2 = _selectionBox.localScale;
			localScale2.x = num2;
			localScale2.y = num2;
			localScale2.z = num2;
			_selectionBox.localScale = localScale2;
		}
		else if (_selectionBox.gameObject.activeSelf)
		{
			_selectionBox.gameObject.SetActive(value: false);
		}
	}
}
