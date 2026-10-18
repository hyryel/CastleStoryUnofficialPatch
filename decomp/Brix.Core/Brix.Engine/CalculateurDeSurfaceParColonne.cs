using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Brix.Engine;

internal class CalculateurDeSurfaceParColonne<Handle>
{
	public class Node
	{
		public object handle;

		public int bas;

		public int haut;

		public int basMaxVoisins;

		public int hautMinVoisins;

		public Node(object handle, int bas, int haut, int basMaxVoisins, int hautMinVoisins)
		{
			this.handle = handle;
			this.bas = bas;
			this.haut = haut;
			this.basMaxVoisins = basMaxVoisins;
			this.hautMinVoisins = hautMinVoisins;
		}
	}

	public IEnumerable<Handle> handles = null;

	public Func<Handle, int> GetXZ = null;

	public Func<Handle, int> GetBas = null;

	public Func<Handle, int> GetHaut = null;

	public const bool EliminerVoxelsSolitaires = true;

	private Dictionary<int, Node> xz2node;

	private Dictionary<int, Node> voisinsVides;

	public IEnumerable<KeyValuePair<int, int>> GetVideDessusY()
	{
		Dictionary<int, Node> colonnes = GetColonnes(handles);
		return MapNodeToY(colonnes, (Node node) => node.haut);
	}

	public IEnumerable<KeyValuePair<int, int>> GetVideDessousY()
	{
		Dictionary<int, Node> colonnes = GetColonnes(handles);
		return MapNodeToY(colonnes, (Node node) => node.bas - 1);
	}

	public IEnumerable<KeyValuePair<int, IEnumerable<int>>> GetVideMursY()
	{
		Dictionary<int, Node> colonnes = GetColonnes(handles);
		Dictionary<int, Node> dictionary = GetVoisinsVides(colonnes);
		return MapNodeToY(dictionary, GetYComplet);
	}

	private IEnumerable<KeyValuePair<int, T>> MapNodeToY<T>(IEnumerable<KeyValuePair<int, Node>> xz2node, Func<Node, T> map)
	{
		return xz2node.Select((KeyValuePair<int, Node> kv) => new KeyValuePair<int, T>(kv.Key, map(kv.Value)));
	}

	public IEnumerable<KeyValuePair<int, Node>> GetColonnes()
	{
		return GetColonnes(handles);
	}

	private Dictionary<int, Node> GetColonnes(IEnumerable<Handle> handles)
	{
		if (xz2node != null)
		{
			return xz2node;
		}
		xz2node = new Dictionary<int, Node>();
		foreach (Handle handle in handles)
		{
			int num = GetBas(handle);
			int num2 = GetHaut(handle);
			if (num2 > 0 && num > 0 && num < num2 && num != num2 - 1)
			{
				int key = GetXZ(handle);
				xz2node[key] = new Node(handle, num, num2, 0, 255);
			}
		}
		GetVoisinsVides(xz2node);
		return xz2node;
	}

	private Dictionary<int, Node> GetVoisinsVides(Dictionary<int, Node> xz2node)
	{
		if (voisinsVides != null)
		{
			return voisinsVides;
		}
		int[] array = new int[4] { 1, 1024, -1, -1024 };
		voisinsVides = new Dictionary<int, Node>();
		foreach (KeyValuePair<int, Node> item in xz2node)
		{
			int key = item.Key;
			Node value = item.Value;
			Node value2 = null;
			for (int i = 0; i < 4; i++)
			{
				int key2 = key + array[i];
				if (xz2node.TryGetValue(key2, out value2))
				{
					Donnees.Augment(ref value2.basMaxVoisins, value.bas);
					Donnees.Diminish(ref value2.hautMinVoisins, value.haut);
					continue;
				}
				if (voisinsVides.TryGetValue(key2, out value2))
				{
					Donnees.Diminish(ref value2.bas, value.bas);
					Donnees.Augment(ref value2.haut, value.haut);
				}
				else
				{
					value2 = new Node(null, value.bas, value.haut, 0, 0);
					voisinsVides.Add(key2, value2);
				}
				value.basMaxVoisins = value.haut;
				value.hautMinVoisins = value.bas;
			}
		}
		return voisinsVides;
	}

	public IEnumerable<int> GetYBasEnHaut(Node node)
	{
		if (node.basMaxVoisins >= node.hautMinVoisins)
		{
			return GetYComplet(node);
		}
		if (node.basMaxVoisins >= node.haut || node.hautMinVoisins <= node.bas)
		{
			return GetYComplet(node);
		}
		if (node.haut - node.bas <= 1)
		{
			return GetYUnique(node);
		}
		int count = Mathf.Max(1, node.basMaxVoisins - node.bas);
		int num = Mathf.Max(1, node.haut - node.hautMinVoisins);
		IEnumerable<int> first = Enumerable.Range(node.bas, count);
		IEnumerable<int> second = Enumerable.Range(node.haut - num, num);
		return first.Concat(second);
	}

	public IEnumerable<int> GetYHautEnBas(Node node)
	{
		return GetYBasEnHaut(node).Reverse();
	}

	public IEnumerable<int> GetYComplet(Node node)
	{
		return Enumerable.Range(node.bas, node.haut - node.bas);
	}

	public IEnumerable<int> GetYUnique(Node node)
	{
		yield return node.bas;
	}

	public int GetYHaut(Node node)
	{
		return node.haut - 1;
	}
}
