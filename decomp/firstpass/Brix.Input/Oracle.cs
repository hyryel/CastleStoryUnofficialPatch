using System;
using System.Collections;
using Brix.Engine;
using Brix.Game.AI;
using Brix.Legacy;
using UnityEngine;

namespace Brix.Input;

public class Oracle
{
	public static float defaultDistance = 1000f;

	public static bool Match(string desc, string tag)
	{
		return desc.Contains(tag);
	}

	public static object TrouverTag(object obj, string tag)
	{
		return TrouverTag(obj, tag, 0);
	}

	public static object TrouverTag(object obj, string tag, int depth)
	{
		if (obj == null || (obj is int && (int)obj == 0))
		{
			return null;
		}
		if (obj is string)
		{
			return (!Match(obj as string, tag)) ? null : obj;
		}
		if (obj is int scalaire)
		{
			if (Match("Voxel", tag) && Voxel.IsNonEmpty(XYZ.FromScalaire(scalaire)))
			{
				return obj;
			}
			if (Match("Arbre", tag) && TreeRegistry.Instance.HasTree(XYZ.FromScalaire(scalaire)))
			{
				return obj;
			}
		}
		if (obj as GameObject != null && ((obj as GameObject).GetComponent<Project>() != null || (obj as GameObject).GetComponent<Project>() != null))
		{
			try
			{
				Project component = (obj as GameObject).GetComponent<Project>();
				if (Match("Groupe", tag))
				{
					return obj;
				}
				object obj2 = TrouverTag(component, tag, depth + 1);
				if (obj2 != null)
				{
					return obj2;
				}
			}
			catch (Exception o)
			{
				Donnees.LogWarning(o);
			}
		}
		if (obj is PieMenuable)
		{
			if (Match("Selectionnable", tag))
			{
				return obj;
			}
			if ((obj as PieMenuable).CheckTag(tag))
			{
				return (!(obj is Selectionnable)) ? obj : (obj as Selectionnable).GetSelection();
			}
		}
		if (obj is GameObject)
		{
			if (Match("GameObject", tag))
			{
				return obj;
			}
			GameObject gameObject = (GameObject)obj;
			if (gameObject != null)
			{
				Component[] components = gameObject.GetComponents((depth > 0) ? typeof(SelectionnableRecursivement) : typeof(PieMenuable));
				if (components.Length > 0)
				{
					for (int i = 0; i < components.Length; i++)
					{
						object obj2 = TrouverTag(components[i], tag, depth + 1);
						if (obj2 != null)
						{
							return obj2;
						}
					}
				}
				foreach (Transform item in gameObject.transform)
				{
					object obj2 = TrouverTag(item.gameObject, tag, depth + 1);
					if (obj2 != null)
					{
						return obj2;
					}
				}
			}
		}
		if (obj is IEnumerable)
		{
			IEnumerable enumerable = (IEnumerable)obj;
			foreach (object item2 in enumerable)
			{
				object obj2 = TrouverTag(item2, tag, depth);
				if (obj2 != null)
				{
					return obj2;
				}
			}
		}
		string name = obj.GetType().Name;
		if (name.Contains(tag))
		{
			return obj;
		}
		return null;
	}
}
