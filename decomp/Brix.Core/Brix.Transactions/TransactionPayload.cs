using Newtonsoft.Json;

namespace Brix.Transactions;

[JsonObject(MemberSerialization.Fields)]
public abstract class TransactionPayload
{
	public abstract void Invoke();

	public virtual bool Validate()
	{
		return true;
	}

	public virtual TransactionMonitor CreateMonitor()
	{
		return new TransactionMonitor();
	}
}
public abstract class TransactionPayload<TReturn> : TransactionPayload where TReturn : TransactionReturnValue, new()
{
	[JsonIgnore]
	private TReturn _returnValue;

	protected TReturn ReturnValue => _returnValue ?? (_returnValue = new TReturn());

	public abstract void Invoke(TReturn returnValue);

	public sealed override void Invoke()
	{
		Invoke(ReturnValue);
	}

	public TransactionMonitor<TReturn> CreateSpecializedMonitor()
	{
		return new TransactionMonitor<TReturn>
		{
			ReturnValue = ReturnValue
		};
	}

	public override TransactionMonitor CreateMonitor()
	{
		return CreateSpecializedMonitor();
	}
}
public abstract class TransactionPayload<TReturn, TMonitor> : TransactionPayload<TReturn> where TReturn : TransactionReturnValue, new() where TMonitor : TransactionMonitor<TReturn>, new()
{
	public new TMonitor CreateSpecializedMonitor()
	{
		return new TMonitor
		{
			ReturnValue = base.ReturnValue
		};
	}

	public override TransactionMonitor CreateMonitor()
	{
		return CreateSpecializedMonitor();
	}
}
