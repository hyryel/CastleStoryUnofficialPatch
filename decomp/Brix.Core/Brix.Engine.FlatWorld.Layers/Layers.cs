using System.Collections.Generic;

namespace Brix.Engine.FlatWorld.Layers;

public class Layers
{
	public List<Layer> layers = new List<Layer>();

	public void AddLayer(Layer layer)
	{
		layers.Add(layer);
	}

	public T GetLayer<T>() where T : Layer
	{
		for (int i = 0; i < layers.Count; i++)
		{
			if (layers[i] is T)
			{
				return (T)layers[i];
			}
		}
		return null;
	}

	public Layer GetLayer(int i)
	{
		return layers[i];
	}

	public void Clear()
	{
		foreach (Layer layer in layers)
		{
			layer?.Destroy();
		}
		layers.Clear();
	}

	public static bool Equals(Layers a, Layers b)
	{
		if (a == b)
		{
			return true;
		}
		if (a.layers.Count != b.layers.Count)
		{
			return false;
		}
		for (int i = 0; i < a.layers.Count; i++)
		{
			if (!Layer.Equals(a.layers[i], b.layers[i]))
			{
				return false;
			}
		}
		return true;
	}
}
