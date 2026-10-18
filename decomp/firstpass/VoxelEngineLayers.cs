using Brix.Engine.FlatWorld.Layers;

public class VoxelEngineLayers : LayersPopulator
{
	public override void Populate(Layers layers)
	{
		layers.AddLayer(new HeightmapLayer());
		layers.AddLayer(new TerrainSurfaceLayer());
		layers.AddLayer(new NegativeLayer());
		layers.AddLayer(new Brix.Engine.FlatWorld.Layers.DeepLayer());
	}
}
