using System.Collections;
using Brix.Utils.Logging;
using Newtonsoft.Json;

namespace Brix.IO.Serialization.Json.Converters;

[JsonObject(MemberSerialization.OptIn)]
public class IListDTO<C, T> where C : class, IList, new()
{
	private static BrixLogger logger = new BrixLogger();

	[JsonProperty]
	[JsonConverter(typeof(ArrayConverter))]
	public C InnerList;

	public IListDTO(C list)
	{
		logger.Info("IListDTO<C,T> list: " + list);
		InnerList = new C();
		foreach (T item in list)
		{
			logger.Info("IListDTO<C,T> item: " + item);
			InnerList.Add(item);
		}
	}
}
