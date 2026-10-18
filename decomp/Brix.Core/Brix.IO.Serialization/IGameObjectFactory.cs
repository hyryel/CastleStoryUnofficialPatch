using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Brix.IO.Serialization;

public interface IGameObjectFactory
{
	GameObject Spawn(object assetKeyResolver, JObject jObject);
}
