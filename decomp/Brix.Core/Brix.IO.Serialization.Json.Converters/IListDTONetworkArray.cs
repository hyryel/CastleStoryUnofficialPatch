using System.Collections.Generic;
using Brix.Network.Util;
using Brix.Utils.Logging;
using Newtonsoft.Json;

namespace Brix.IO.Serialization.Json.Converters;

[JsonObject(MemberSerialization.OptIn)]
public class IListDTONetworkArray
{
	private static BrixLogger logger = new BrixLogger();

	[JsonProperty]
	[JsonConverter(typeof(ArrayConverter))]
	public List<object> InnerList;

	public IListDTONetworkArray()
	{
		InnerList = new List<object>();
	}

	public IListDTONetworkArray(NetworkArray<int> list)
	{
		logger.Info("IListDTONetworkArray list: " + list);
		InnerList = new List<object>();
		foreach (int item in list)
		{
			logger.Info("IListDTONetworkArray item: " + item);
			InnerList.Add(item);
		}
	}
}
