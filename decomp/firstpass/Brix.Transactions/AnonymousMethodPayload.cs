using System;

namespace Brix.Transactions;

public class AnonymousMethodPayload : TransactionPayload
{
	private class AnonymousMethodPayloadException : Exception
	{
		public AnonymousMethodPayloadException(AnonymousMethodPayload p, Exception e)
			: base(string.Concat("Invocation failed for processor '", p._method.methodInfo, "': ", e))
		{
		}
	}

	private MethodWrapper _method;

	public AnonymousMethodPayload(MethodWrapper methodWrapper)
	{
		_method = methodWrapper;
	}

	public override void Invoke()
	{
		try
		{
			_method.Invoke();
		}
		catch (Exception e)
		{
			throw new AnonymousMethodPayloadException(this, e);
		}
	}
}
public class AnonymousMethodPayload<T> : TransactionPayload<AnonymousReturnValue<T>>
{
	private class AnonymousMethodPayloadException : Exception
	{
		public AnonymousMethodPayloadException(AnonymousMethodPayload<T> p, Exception e)
			: base(string.Concat("Invocation failed for processor '", p._method.methodInfo, "': ", e))
		{
		}
	}

	private MethodWrapper _method;

	public AnonymousMethodPayload(MethodWrapper methodWrapper)
	{
		_method = methodWrapper;
	}

	public override void Invoke(AnonymousReturnValue<T> returnValue)
	{
		try
		{
			returnValue.result = (T)_method.Invoke();
		}
		catch (Exception e)
		{
			throw new AnonymousMethodPayloadException(this, e);
		}
	}
}
