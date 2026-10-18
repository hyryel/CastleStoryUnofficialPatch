using System;
using Brix.Core;

namespace Brix.Engine.FlatWorld.Layers;

public class SelectableLayersPopulator : LayersPopulator
{
	[TypeReference.Config(typeof(ISelectableLayer))]
	public TypeReference[] LayerTypes;

	public override void Populate(Layers layers)
	{
		for (int i = 0; i < LayerTypes.Length; i++)
		{
			Layer layer = (LayerTypes[i].IsVoid ? null : ((Layer)Activator.CreateInstance(LayerTypes[i])));
			if (layer != null)
			{
				layers.AddLayer(layer);
			}
		}
	}
}
