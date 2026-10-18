using System;
using Brix.Network.Util;
using Brix.Utils.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Brix.IO.Serialization.Json.Converters;

public class IListDTOConverterInternal : JsonConverter
{
	private static BrixLogger logger = new BrixLogger();

	public override bool CanWrite => false;

	public override bool CanConvert(Type objectType)
	{
		if (objectType == typeof(IListDTONetworkArray))
		{
			return true;
		}
		return false;
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		logger.Info("IListDTOConverterNetworkArray.ReadJson() objectType: " + objectType);
		JObject jObject = JObject.Load(reader);
		logger.Info(string.Concat("IListDTOConverterNetworkArray.ReadJson() objectType: ", objectType, " jObject: ", jObject));
		object obj = jObject.ToObject(objectType);
		logger.Debug(string.Concat("IListDTOConverterNetworkArray.ReadJson() objectType: ", objectType, " target: ", obj));
		serializer.Populate(jObject.CreateReader(), obj);
		IListDTONetworkArray listDTONetworkArray = obj as IListDTONetworkArray;
		NetworkArray<int> networkArray = new NetworkArray<int>();
		foreach (int inner in listDTONetworkArray.InnerList)
		{
			networkArray.Add(inner);
		}
		return networkArray;
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		throw new NotImplementedException();
	}
}
