using Brix.Lua;
using Brix.UI.Icons;
using UnityEngine;

namespace Brix.Game.Beacons;

public class BeaconMeta : MonoBehaviour
{
	public class Audio
	{
		public string Created = string.Empty;

		public string PositionChanged = "Beacon_Completed";

		public string RadiusChanged = "Beacon_Completed";

		public string OnHover = "Beacon_Rollover_On";

		public string BeaconSelect = "Beacon_Select";
	}

	[SerializeField]
	private IconKey.IconKeyLink iconKey;

	[LuaDrivable("displayName")]
	public string displayName;

	[LuaDrivable("beaconIcon")]
	private Sprite spriteOverride;

	public Audio sound = new Audio();

	public Sprite sprite => spriteOverride ?? iconKey.Get64;

	public void OnValidate()
	{
		iconKey.Validate();
	}

	public void Awake()
	{
		iconKey.Validate();
	}
}
