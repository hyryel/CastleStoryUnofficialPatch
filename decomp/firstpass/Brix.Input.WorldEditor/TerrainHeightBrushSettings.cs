namespace Brix.Input.WorldEditor;

public class TerrainHeightBrushSettings : CreateIslandBrushSettings
{
	private bool dontCreate = true;

	public override bool DontCreate => dontCreate;
}
