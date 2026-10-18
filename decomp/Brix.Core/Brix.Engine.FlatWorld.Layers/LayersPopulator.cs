using UnityEngine;

namespace Brix.Engine.FlatWorld.Layers;

public abstract class LayersPopulator : MonoBehaviour
{
	public abstract void Populate(Layers layers);
}
