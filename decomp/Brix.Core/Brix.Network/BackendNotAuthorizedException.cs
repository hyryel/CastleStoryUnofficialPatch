using System;
using System.Runtime.Serialization;

namespace Brix.Network;

[Serializable]
public class BackendNotAuthorizedException : Exception
{
	public BackendNotAuthorizedException()
	{
	}

	public BackendNotAuthorizedException(string message)
		: base(message)
	{
	}

	public BackendNotAuthorizedException(string message, Exception inner)
		: base(message, inner)
	{
	}

	protected BackendNotAuthorizedException(SerializationInfo info, StreamingContext context)
	{
	}
}
