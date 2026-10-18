using Brix.IO.Serialization.Json.Converters;
using Brix.Transactions;
using Newtonsoft.Json;
using UnityEngine;

namespace Brix.Network;

public class OperationData
{
	[JsonConverter(typeof(GameObjectReferenceConverter))]
	public GameObject Transactor;

	public TransactionPayload Payload;
}
