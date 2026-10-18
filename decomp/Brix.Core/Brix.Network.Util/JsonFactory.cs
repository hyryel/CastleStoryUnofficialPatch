using System;
using Newtonsoft.Json.Linq;

namespace Brix.Network.Util;

public abstract class JsonFactory
{
	public virtual object CreateInstance(string objectType, JObject jObject)
	{
		return null;
	}

	public virtual void RegisterClass(Type objectType)
	{
	}

	public virtual bool CanCreate(Type objectType)
	{
		return false;
	}
}
