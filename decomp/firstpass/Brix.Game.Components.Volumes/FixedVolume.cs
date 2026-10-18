using Brix.External.Factories;
using Brix.Utils.Unity;
using UnityEngine.Networking;

namespace Brix.Game.Components.Volumes;

public class FixedVolume : Volume
{
	[DisplayRules(DisplayFlags.ReadOnly)]
	private Factory.AssetKey _assetKeyVolumeReference;

	public override Factory.AssetKey AssetKey => _assetKeyVolumeReference;

	public override FreeVolume InternalVolume => _internalVolume;

	public override void Initialize()
	{
		base.Initialize();
		_assetKeyVolumeReference = base.AssetKey;
	}

	protected override void Start()
	{
		base.Start();
		Register();
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		bool flag2 = default(bool);
		return flag2 || flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
	}
}
