using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Brix.IO.Serialization;

public interface IAssetKeyResolver
{
	string ResolveAssetKey(GameObject gameObject);

	object ResolveAssetKey(JObject gameObject);
}
