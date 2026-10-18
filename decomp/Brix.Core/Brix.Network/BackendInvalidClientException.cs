using System;
using System.Runtime.Serialization;

namespace Brix.Network;

[Serializable]
public class BackendInvalidClientException : Exception
{
	public BackendInvalidClientException()
	{
	}

	public BackendInvalidClientException(string message)
		: base(message)
	{
	}

	public BackendInvalidClientException(string message, Exception inner)
		: base(message, inner)
	{
	}

	protected BackendInvalidClientException(SerializationInfo info, StreamingContext context)
	{
	}
}
