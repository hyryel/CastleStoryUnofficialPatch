using Brix.Network.Util;
using Newtonsoft.Json;
using UnityEngine;

namespace Brix.IO.Serialization;

[SerializerTarget(new string[] { "Excluded" })]
[JsonObject(MemberSerialization.OptIn)]
public class TestComponentExcludedBecauseOfSerializationTarget : MonoBehaviour
{
	[JsonProperty]
	public int dummyField = 777;

	[JsonProperty]
	[SerializerTarget(new string[] { "Excluded" })]
	public GameObject go;
}
