using System;
using System.Collections;
using Brix.Utils.Logging;
using Newtonsoft.Json;

namespace Brix.IO.Serialization.Json.Converters;

public class IListDTOConverter<C, T> : JsonConverter where C : class, IList, new()
{
	private static BrixLogger logger = new BrixLogger();

	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(C);
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		logger.Info("IListDTOConverter<C,T>.WriteJson()  typeof(value): " + value.GetType());
		IListDTO<C, T> listDTO = new IListDTO<C, T>(value as C);
		logger.Info("IListDTOConverter<C,T>.WriteJson()  InnerList.Count: " + listDTO.InnerList.Count);
		serializer.Serialize(writer, listDTO);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		logger.Info("IListDTOConverter<C,T>.ReadJson() objectType: " + objectType);
		IListDTO<C, T> listDTO = serializer.Deserialize<IListDTO<C, T>>(reader);
		return listDTO.InnerList;
	}
}
