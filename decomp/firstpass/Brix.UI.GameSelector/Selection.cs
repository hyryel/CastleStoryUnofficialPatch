using System.Collections.Generic;
using Brix.Game;
using Brix.Game.Components;
using Brix.Utils;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.UI.GameSelector;

public class Selection
{
	public SelectionType type;

	private List<UISelectable> selecteds = new List<UISelectable>();

	private List<UISelectable> hovereds = new List<UISelectable>();

	public Selection(SelectionType type)
	{
		this.type = type;
	}

	public void Selecteds(ref List<GameObject> gos)
	{
		selecteds.RemoveAll(GameComponentExtender.IsNullOrReleased);
		selecteds.Slinq().ForEach(delegate(UISelectable s, List<GameObject> p)
		{
			p.Add(s.gameObject);
		}, gos);
	}

	public void Hovereds(ref List<GameObject> gos)
	{
		hovereds.RemoveAll(GameComponentExtender.IsNullOrReleased);
		(from s in hovereds.Slinq()
			select s.gameObject).AddTo(gos);
	}

	public int GetHoveredOrSelectedCount()
	{
		return hovereds.Count + selecteds.Count;
	}

	public bool IsSelected(UISelectable s)
	{
		return selecteds.Contains(s);
	}

	public bool IsHovered(UISelectable s)
	{
		return hovereds.Contains(s);
	}

	public bool SelectionBelongsTo(User user)
	{
		return selecteds.Slinq().All((UISelectable s) => Affiliation.IsOwnedBy(s.gameObject, user.gameObject));
	}

	public bool HoversBelongsTo(User user)
	{
		return hovereds.Slinq().All((UISelectable s) => Affiliation.IsOwnedBy(s.gameObject, user.gameObject));
	}

	public bool AnySelected()
	{
		return selecteds.Slinq().Any();
	}

	public bool AnyHovered()
	{
		return hovereds.Slinq().Any();
	}

	public void Select(UISelectable s)
	{
		try
		{
			if (!selecteds.Contains(s))
			{
				selecteds.Add(s);
				s.OnSelect();
			}
		}
		finally
		{
		}
	}

	public void Unselect(UISelectable s)
	{
		selecteds.Remove(s);
		s.OnUnselect();
	}

	public void Hover(UISelectable s)
	{
		if (!hovereds.Contains(s))
		{
			hovereds.Add(s);
			s.OnHover();
		}
	}

	public void Unhover(UISelectable s)
	{
		hovereds.Remove(s);
		s.OnUnhover();
	}

	public void SelectWhatIsHovered()
	{
		List<UISelectable> list = CollectionPool.RequestList<UISelectable>();
		hovereds.Slinq().AddTo(list);
		(from s in list.Slinq()
			where s.IsSelectable()
			select s).ForEach(delegate(UISelectable b, Selection self)
		{
			self.Select(b);
		}, this);
		list.Slinq().ForEach(delegate(UISelectable s, Selection self)
		{
			self.Unhover(s);
		}, this);
		if (list.Count > 0)
		{
			list[0].OnMultiSelect(list);
		}
		list.Dispose();
	}

	public void UnselectAll(UISelectable except = null)
	{
		List<UISelectable> list = CollectionPool.RequestList<UISelectable>();
		(from s in selecteds.Slinq()
			where s != except
			select s).AddTo(list);
		list.Slinq().ForEach(delegate(UISelectable b, Selection self)
		{
			self.Unselect(b);
		}, this);
		list.Dispose();
	}

	public void UnhoverAll()
	{
		List<UISelectable> list = CollectionPool.RequestList<UISelectable>();
		hovereds.Slinq().AddTo(list);
		list.Slinq().ForEach(delegate(UISelectable b, Selection self)
		{
			self.Unhover(b);
		}, this);
		list.Dispose();
	}
}
