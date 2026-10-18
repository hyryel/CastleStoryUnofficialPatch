using Brix.Network.Util;
using Newtonsoft.Json;
using UnityEngine;

namespace Brix.IO.Serialization;

[SerializerTarget(new string[] { "Test" })]
[JsonObject(MemberSerialization.OptIn)]
public class TestComponentIncludedBecauseOfSerializationTarget : MonoBehaviour
{
	[JsonProperty]
	public int dummyField = 777;

	[JsonProperty]
	public GameObject go;
}
