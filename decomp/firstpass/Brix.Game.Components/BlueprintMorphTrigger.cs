using Brix.Engine;
using Brix.External.Signals;
using UnityEngine;

namespace Brix.Game.Components;

[RequireComponent(typeof(Blueprint))]
public class BlueprintMorphTrigger : MonoBehaviour
{
	private SignalConnection _levelLoadedConnection = SignalConnection.Empty;

	private void Awake()
	{
		_levelLoadedConnection = GameSignals.Once(GameSignals.LevelLoaded, delegate(BlueprintMorphTrigger self)
		{
			self.Morph();
		}, this);
	}

	private void Morph()
	{
		if (GameParam.Gamemode != null && GameParam.Gamemode.codename != "world_editor")
		{
			GetComponent<Blueprint>().MorphToObject();
		}
	}

	private void OnDestroy()
	{
		_levelLoadedConnection.Disconnect();
	}
}
