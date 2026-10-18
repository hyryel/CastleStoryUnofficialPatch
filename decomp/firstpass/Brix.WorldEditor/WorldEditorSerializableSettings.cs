using System.Collections.Generic;
using Brix.Game.Components;
using Newtonsoft.Json;

namespace Brix.WorldEditor;

[JsonObject(MemberSerialization.OptIn)]
public class WorldEditorSerializableSettings : GameComponent
{
	[JsonProperty]
	public Dictionary<string, int> TotalMarkerCreatedCount = new Dictionary<string, int>();

	public static WorldEditorSerializableSettings Instance { get; set; }

	public override void OnRequested()
	{
		base.OnRequested();
		Instance = this;
	}
}
