namespace Brix.Input.WorldEditor;

public class GrassBrushSettings : BrushSettings, ITerrainType
{
	private int terrainType = 2;

	public int TerrainType
	{
		get
		{
			return terrainType;
		}
		set
		{
			terrainType = value;
		}
	}
}
