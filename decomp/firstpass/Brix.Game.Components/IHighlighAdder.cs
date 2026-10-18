using Brix.Engine;
using Brix.External.Signals;
using UnityEngine;

namespace Brix.Game.Components;

public interface IHighlighAdder
{
	int At(XYZ pos, Color color, IReadOnlySignal<GameObject> removeWhen = null);

	int Object(GameObject go, Color color, IReadOnlySignal<GameObject> removeWhen = null);

	int Terrain(XYZ pos, Color color, Signal<GameObject> removeWhen);

	int BlockAndTerrain(XYZ pos, Color color, Signal<GameObject> removeWhen);
}
