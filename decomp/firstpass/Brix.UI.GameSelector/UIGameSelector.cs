using System;
using System.Collections.Generic;
using Brix.External.Signals;
using Brix.Game;
using Brix.Game.Components;
using Brix.Game.Network;
using Brix.Input;
using Brix.Utils;
using Brix.Utils.Collections;
using Brix.Utils.Signals;
using Brix.Utils.Unity;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.UI.GameSelector;

public class UIGameSelector : MonoBehaviour
{
	[SerializeField]
	public GameObject SelectionBox;

	[SerializeField]
	private UIGameSelectorConfig Config;

	public static UIGameSelector instance;

	[SerializeField]
	[DisplayRules(DisplayFlags.ReadOnly | DisplayFlags.HideInEditMode)]
	private List<UISelectable> RegisteredList = new List<UISelectable>();

	public int MaximumHoveredOrSelectedCount = 500;

	private readonly Dictionary<SelectionType, Selection> selection = new Dictionary<SelectionType, Selection>();

	public readonly Dictionary<SelectionType, Signal<UISelectable>> OnSelectedDico = new Dictionary<SelectionType, Signal<UISelectable>>();

	public readonly Dictionary<SelectionType, Signal<UISelectable>> OnUnselectedDico = new Dictionary<SelectionType, Signal<UISelectable>>();

	public readonly Dictionary<SelectionType, Signal<UISelectable>> OnHoveredDico = new Dictionary<SelectionType, Signal<UISelectable>>();

	public readonly Dictionary<SelectionType, Signal<UISelectable>> OnUnhoveredDico = new Dictionary<SelectionType, Signal<UISelectable>>();

	private void InitializeSelectionData()
	{
		foreach (SelectionType value in Enum.GetValues(typeof(SelectionType)))
		{
			selection.Add(value, new Selection(value));
			OnSelectedDico.Add(value, new Signal<UISelectable>());
			OnUnselectedDico.Add(value, new Signal<UISelectable>());
			OnHoveredDico.Add(value, new Signal<UISelectable>());
			OnUnhoveredDico.Add(value, new Signal<UISelectable>());
		}
	}

	private void Awake()
	{
		instance = this;
		InitializeSelectionData();
	}

	public static void AddOnSelectedListener(SelectionType selectionType, Action<UISelectable> callback, Signal disconnectWhen = null)
	{
		AddListener(instance.OnSelectedDico, selectionType, callback, disconnectWhen);
	}

	public static void AddOnUnselectedListener(SelectionType selectionType, Action<UISelectable> callback, Signal disconnectWhen = null)
	{
		AddListener(instance.OnUnselectedDico, selectionType, callback, disconnectWhen);
	}

	public static void AddOnHoveredListener(SelectionType selectionType, Action<UISelectable> callback, Signal disconnectWhen = null)
	{
		AddListener(instance.OnHoveredDico, selectionType, callback, disconnectWhen);
	}

	public static void AddOnUnhoveredListener(SelectionType selectionType, Action<UISelectable> callback, Signal disconnectWhen = null)
	{
		AddListener(instance.OnUnhoveredDico, selectionType, callback, disconnectWhen);
	}

	public static SignalConnection AddOnSelectedListener(SelectionLayer selectionLayer, Action<UISelectable> callback)
	{
		return instance.OnSelectedDico[selectionLayer.ToType()].Connect(callback);
	}

	public static SignalConnection AddOnUnselectedListener(SelectionLayer selectionLayer, Action<UISelectable> callback)
	{
		return instance.OnUnselectedDico[selectionLayer.ToType()].Connect(callback);
	}

	public static SignalConnection AddOnHoveredListener(SelectionLayer selectionLayer, Action<UISelectable> callback)
	{
		return instance.OnHoveredDico[selectionLayer.ToType()].Connect(callback);
	}

	public static SignalConnection AddOnUnhoveredListener(SelectionLayer selectionLayer, Action<UISelectable> callback)
	{
		return instance.OnUnhoveredDico[selectionLayer.ToType()].Connect(callback);
	}

	private static void AddListener(Dictionary<SelectionType, Signal<UISelectable>> signal, SelectionType selectionType, Action<UISelectable> callback, Signal disconnectWhen = null)
	{
		int num = (int)selectionType;
		while (num != 0)
		{
			int num2 = num & (num - 1);
			int key = num & ~num2;
			num = num2;
			SignalConnection c = signal[(SelectionType)key].Connect(callback);
			if (disconnectWhen != null)
			{
				c.DisconnectWhen(disconnectWhen);
			}
		}
	}

	public static void Register(UISelectable selectable)
	{
		if ((bool)selectable && !instance.RegisteredList.Contains(selectable))
		{
			instance.RegisteredList.Add(selectable);
		}
	}

	public static void Unregister(UISelectable selectable)
	{
		Unhover(selectable);
		Unselect(selectable);
		if (!instance.RegisteredList.Contains(selectable))
		{
			instance.RegisteredList.Remove(selectable);
		}
	}

	public static List<UISelectable> GetAllSelectables()
	{
		return instance.RegisteredList;
	}

	public static void GetSelectedOfType(ref List<GameObject> gos, SelectionType type)
	{
		instance.selection[type].Selecteds(ref gos);
	}

	public static void GetSelectedInTrack(ref List<GameObject> gos, SelectionTrackID track)
	{
		List<SelectionType> list = instance.Config.TypesIn(track);
		foreach (SelectionType item in list)
		{
			instance.selection[item].Selecteds(ref gos);
		}
	}

	public static bool HasSelectionInTrack(SelectionTrackID track)
	{
		List<SelectionType> list = instance.Config.TypesIn(track);
		foreach (SelectionType item in list)
		{
			if (instance.selection[item].AnySelected())
			{
				return true;
			}
		}
		return false;
	}

	public static void GetSelectedEverything(ref List<GameObject> gos)
	{
		foreach (KeyValuePair<SelectionType, Selection> item in instance.selection)
		{
			item.Value.Selecteds(ref gos);
		}
	}

	public static void GetHoveredOfType(SelectionType type, ref List<GameObject> gos)
	{
		instance.selection[type].Hovereds(ref gos);
	}

	public static void GetHoveredInTrack(ref List<GameObject> gos, SelectionTrackID track)
	{
		List<SelectionType> list = instance.Config.TypesIn(track);
		foreach (SelectionType item in list)
		{
			instance.selection[item].Hovereds(ref gos);
		}
	}

	public static void GetHoveredEverything(ref List<GameObject> gos)
	{
		foreach (KeyValuePair<SelectionType, Selection> item in instance.selection)
		{
			item.Value.Hovereds(ref gos);
		}
	}

	public static bool IsInTrack(SelectionTrackID track, SelectionType type)
	{
		return instance.Config.TypesIn(track).Contains(type);
	}

	public static bool IsSelected(UISelectable selectable)
	{
		return instance.selection[selectable.SelectionType].IsSelected(selectable);
	}

	public static bool IsSelected(GameObject obj)
	{
		UISelectable uISelectable = FindSelectable(obj);
		if (uISelectable.IsNullOrReleased())
		{
			return false;
		}
		return IsSelected(uISelectable);
	}

	public static bool IsFocused(GameObject obj)
	{
		UISelectable uISelectable = FindSelectable(obj);
		if (uISelectable.IsNullOrReleased())
		{
			return false;
		}
		return IsFocused(uISelectable);
	}

	public static bool IsFocused(UISelectable selectable)
	{
		return IsSelected(selectable) || IsHovered(selectable);
	}

	public static bool SomeIsSelected(SelectionType type)
	{
		return instance.selection[type].AnySelected();
	}

	public static bool SomeIsHovered(SelectionType type)
	{
		return instance.selection[type].AnyHovered();
	}

	public static bool IsSelectable(GameObject obj)
	{
		UISelectable uISelectable = FindSelectable(obj);
		if (uISelectable == null)
		{
			return false;
		}
		return uISelectable.IsSelectable();
	}

	public static bool IsHoverable(GameObject obj)
	{
		UISelectable uISelectable = FindSelectable(obj);
		if (uISelectable == null)
		{
			return false;
		}
		return uISelectable.IsHoverable();
	}

	public static bool IsHovered(UISelectable selectable)
	{
		return instance.selection[selectable.SelectionType].IsHovered(selectable);
	}

	public static bool IsHovered(GameObject obj)
	{
		UISelectable uISelectable = FindSelectable(obj);
		if (uISelectable.IsNullOrReleased())
		{
			return false;
		}
		return IsHovered(uISelectable);
	}

	public static bool IsHoveredOrSelectedMaxedOut(UISelectable selectable)
	{
		return instance.selection[selectable.SelectionType].GetHoveredOrSelectedCount() >= instance.MaximumHoveredOrSelectedCount;
	}

	public static void Hover(GameObject obj)
	{
		UISelectable uISelectable = FindSelectable(obj);
		if (!uISelectable.IsNullOrReleased())
		{
			Hover(uISelectable);
		}
	}

	public static void HoverMany(List<GameObject> objs)
	{
		List<UISelectable> list = (from o in objs.Slinq()
			select o.GetComponent<UISelectable>()).ToPooledList();
		if (list.Slinq().Any())
		{
			HoverMany(list);
		}
		list.Dispose();
	}

	public static void HoverMany(List<UISelectable> selectables)
	{
		selectables.Slinq().ForEach(delegate(UISelectable s)
		{
			Hover(s);
		});
	}

	public static void Hover(UISelectable selectable)
	{
		if (selectable.IsHoverable() && !IsHovered(selectable) && !IsHoveredOrSelectedMaxedOut(selectable))
		{
			instance.selection[selectable.SelectionType].Hover(selectable);
		}
	}

	public static void Unhover(GameObject obj)
	{
		UISelectable uISelectable = FindSelectable(obj);
		if (!uISelectable.IsNullOrReleased())
		{
			Unhover(uISelectable);
		}
	}

	public static void Unhover(UISelectable selectable)
	{
		if (IsHovered(selectable))
		{
			instance.selection[selectable.SelectionType].Unhover(selectable);
		}
	}

	public static void UnhoverMany(List<GameObject> objs)
	{
		List<UISelectable> list = (from o in objs.Slinq()
			select o.GetComponent<UISelectable>()).ToPooledList();
		if (list.Slinq().Any())
		{
			UnhoverMany(list);
		}
		list.Dispose();
	}

	public static void UnhoverMany(List<UISelectable> selectables)
	{
		selectables.Slinq().ForEach(delegate(UISelectable s)
		{
			Unhover(s);
		});
	}

	public static void UnhoverEverythingExcept(SelectionType type)
	{
		_UnhoverEverything(type);
	}

	public static void UnhoverEverything()
	{
		_UnhoverEverything();
	}

	private static void _UnhoverEverything(SelectionType except = SelectionType.None)
	{
		foreach (KeyValuePair<SelectionType, Selection> item in instance.selection)
		{
			if (item.Key != except)
			{
				item.Value.UnhoverAll();
			}
		}
	}

	public static void SelectWhatIsHovered()
	{
		foreach (KeyValuePair<SelectionType, Selection> item in instance.selection)
		{
			item.Value.SelectWhatIsHovered();
		}
	}

	public static void Select(GameObject obj)
	{
		UISelectable uISelectable = FindSelectable(obj);
		if ((bool)uISelectable)
		{
			instance.selection[uISelectable.SelectionType].UnselectAll();
			_Select(uISelectable);
		}
	}

	private static void _Select(UISelectable selectable)
	{
		if (!InputModeController.IsPaused() && selectable.IsSelectable() && !IsSelected(selectable) && (IsYourOwn(selectable) || IsAPlacedBlock(selectable)) && !IsHoveredOrSelectedMaxedOut(selectable))
		{
			instance.selection[selectable.SelectionType].Select(selectable);
			UnhoverEverything();
		}
	}

	public static void Select(List<GameObject> objs)
	{
		_SelectMany(objs, clearSelection: true);
	}

	public static void AddToSelection(GameObject obj)
	{
		UISelectable uISelectable = FindSelectable(obj);
		if ((bool)uISelectable && CurrentSelectionIsCompatibleWith(uISelectable.SelectionType))
		{
			_Select(uISelectable);
		}
	}

	public static void AddToSelection(List<GameObject> objs)
	{
		_SelectMany(objs, clearSelection: false);
	}

	public static void ToggleSelection(GameObject obj)
	{
		if (IsSelected(obj))
		{
			Unselect(obj);
		}
		else
		{
			AddToSelection(obj);
		}
	}

	private static void _SelectMany(List<GameObject> gos, bool clearSelection)
	{
		List<UISelectable> list = (from o in gos.Slinq()
			select o.GetComponent<UISelectable>()).ToPooledList();
		if (list.Slinq().Any())
		{
			_SelectMany(list, clearSelection);
		}
		list.Dispose();
	}

	private static void _SelectMany(List<UISelectable> selectables, bool clearSelection)
	{
		if (selectables.Count >= 1 && !selectables.Slinq().Any((UISelectable s) => !IsYourOwn(s) && !IsAPlacedBlock(s)))
		{
			if (clearSelection)
			{
				UnselectAll(selectables[0].SelectionType);
			}
			selectables.Slinq().ForEach(delegate(UISelectable s)
			{
				_Select(s);
			});
			selectables[0].OnMultiSelect(selectables);
		}
	}

	public static void Unselect(GameObject obj)
	{
		UISelectable uISelectable = FindSelectable(obj);
		if (!uISelectable.IsNullOrReleased())
		{
			Unselect(uISelectable);
		}
	}

	public static void UnselectMany(List<GameObject> objs)
	{
		List<UISelectable> list = (from o in objs.Slinq()
			select o.GetComponent<UISelectable>()).ToPooledList();
		if (list.Slinq().Any())
		{
			UnselectMany(list);
		}
		list.Dispose();
	}

	private static void UnselectMany(List<UISelectable> selectables)
	{
		selectables.Slinq().ForEach(delegate(UISelectable s)
		{
			Unselect(s);
		});
	}

	public static void Unselect(UISelectable selectable)
	{
		if (IsSelected(selectable))
		{
			instance.selection[selectable.SelectionType].Unselect(selectable);
		}
	}

	public static void UnselectAll(SelectionType selectionType, GameObject except = null)
	{
		instance.selection.Slinq().Where((KeyValuePair<SelectionType, Selection> s, SelectionType aSelectionType) => aSelectionType.HasFlag(s.Key), selectionType).ForEach(delegate(KeyValuePair<SelectionType, Selection> s, GameObject aExcept)
		{
			s.Value.UnselectAll(FindSelectable(aExcept));
		}, except);
	}

	public static void UnselectAllInTrack(SelectionTrackID track, GameObject except = null)
	{
		List<SelectionType> list = instance.Config.TypesIn(track);
		foreach (SelectionType item in list)
		{
			instance.selection[item].UnselectAll();
		}
	}

	public static void UnselectEverything(GameObject except = null)
	{
		foreach (KeyValuePair<SelectionType, Selection> item in instance.selection)
		{
			if (!instance.Config.IsIndependent(item.Value.type))
			{
				item.Value.UnselectAll(FindSelectable(except));
			}
		}
		UnhoverEverything();
	}

	public static bool CurrentSelectionIsCompatibleWith(SelectionType type)
	{
		List<SelectionType> list = instance.Config.TypesExclusiveTo(type);
		foreach (SelectionType item in list)
		{
			if (instance.selection[item].AnySelected())
			{
				return false;
			}
		}
		return true;
	}

	public static bool HasSelectableParentGameObject(GameObject go)
	{
		UISelectable component = go.GetComponent<UISelectable>();
		return !component || component != FindSelectable(go);
	}

	public static UISelectable FindSelectable(GameObject go)
	{
		if (go.IsNullOrReleased())
		{
			return null;
		}
		GenericParentTracker component = go.GetComponent<GenericParentTracker>();
		if ((bool)component && component.IsTracked())
		{
			UISelectable component2 = component.TrackingParent().GetComponent<UISelectable>();
			if ((bool)component2 && component2.SelectionType != SelectionType.Project)
			{
				return component2;
			}
		}
		UISelectable component3 = go.GetComponent<UISelectable>();
		if (!component3)
		{
			UISelectable uISelectable = SignificantParent<UISelectable>.Of(go);
			if ((bool)uISelectable)
			{
				return uISelectable;
			}
		}
		if (!component3)
		{
			GameComponent gameComponent = PickingUtility.FindComponentInParents<GameComponent>(go, layerwise: false);
			if ((bool)gameComponent)
			{
				GameObject gameObject = gameComponent.gameObject;
				component3 = gameObject.GetComponent<UISelectable>();
			}
		}
		return component3;
	}

	private static bool IsYourOwn(UISelectable selectable)
	{
		return Affiliation.IsOwnedBy(selectable.gameObject, User.LocalUser.gameObject);
	}

	private static bool IsAPlacedBlock(UISelectable selectable)
	{
		return selectable.SelectionType == SelectionType.Blocks;
	}
}
