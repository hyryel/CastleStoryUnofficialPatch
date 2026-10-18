using Brix.Engine;
using Brix.External.Factories;
using Brix.Game.AI;
using Brix.Game.Components;

public struct SpawnOrder
{
	public XYZ pos;

	public XYZ direction;

	public Factory.AssetKey key;

	public Goal instance;

	public bool alwaysReplace;

	public static SpawnOrder None = new SpawnOrder(XYZ.zero, XYZ.zero, null);

	public SpawnOrder(XYZ _pos, XYZ _dir, Factory.AssetKey _key, bool _alwaysReplace = false)
	{
		pos = _pos;
		direction = _dir;
		key = _key;
		instance = null;
		alwaysReplace = _alwaysReplace;
	}

	public bool IsValid()
	{
		return key == null == instance.IsNullOrReleased();
	}
}
