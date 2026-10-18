using System;
using Brix.Utils;
using UnityEngine;

namespace Brix.Input;

[Serializable]
public class LayerBundle : IPoolable, IResetable, IDisposable
{
	[SerializeField]
	private int _layer;

	public LayerBundle()
	{
	}

	public LayerBundle(params int[] layers)
	{
		Add(layers);
	}

	public static implicit operator int(LayerBundle layerBundle)
	{
		return layerBundle?._layer ?? 0;
	}

	public static implicit operator LayerBundle(int layer)
	{
		LayerBundle layerBundle = ObjectPool<LayerBundle>.Reserve();
		layerBundle.Set(layer);
		return layerBundle;
	}

	public void Set(int layer)
	{
		_layer = layer;
	}

	public void Add(int layer)
	{
		_layer |= 1 << layer;
	}

	public void AddAsIs(int layer)
	{
		_layer |= layer;
	}

	public void Add(params int[] layers)
	{
		foreach (int num in layers)
		{
			_layer |= 1 << num;
		}
	}

	public void Remove(int layer)
	{
		_layer &= ~(1 << layer);
	}

	public void RemoveAsIs(int layer)
	{
		_layer &= ~layer;
	}

	public void Remove(params int[] layers)
	{
		foreach (int num in layers)
		{
			_layer &= ~(1 << num);
		}
	}

	public int GetLayers()
	{
		return _layer;
	}

	public bool ContainsAsIs(int layer)
	{
		return (~_layer & layer) == 0;
	}

	public bool ContainsAny(int layer)
	{
		if ((_layer & layer) > 0)
		{
			return true;
		}
		return false;
	}

	public void Dispose()
	{
	}

	public void ResetObject()
	{
		_layer = 0;
	}
}
