using System;
using Brix.Network.Util;
using Brix.Utils.Logging;
using Newtonsoft.Json;

namespace Brix.IO.Serialization.Json.Converters;

public class IListDTOConverterNetworkArray : JsonConverter
{
	private static BrixLogger logger = new BrixLogger();

	public override bool CanConvert(Type objectType)
	{
		if (objectType == typeof(NetworkArray<int>))
		{
			return true;
		}
		return false;
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		throw new NotImplementedException();
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		logger.Info("IListDTOConverterNetworkArray.WriteJson()  typeof(value): " + value.GetType());
		IListDTONetworkArray listDTONetworkArray = new IListDTONetworkArray(value as NetworkArray<int>);
		logger.Info("IListDTOConverterNetworkArray.WriteJson()  InnerList.Count: " + listDTONetworkArray.InnerList.Count);
		serializer.Serialize(writer, listDTONetworkArray);
	}
}
