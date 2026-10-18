using System;
using Brix.External.Signals;

namespace Brix.Transactions;

public class TransactionMonitor<TReturn> : TransactionMonitor where TReturn : TransactionReturnValue, new()
{
	public new Signal<TReturn> OnSuccess = new Signal<TReturn>();

	public TReturn ReturnValue { get; internal set; }

	internal override void CompleteTransaction()
	{
		base.CompleteTransaction();
		OnSuccess.Invoke(ReturnValue);
	}
}
public class TransactionMonitor
{
	public static TransactionMonitor Dummy = new TransactionMonitor
	{
		_state = TransactionState.Success
	};

	private TransactionState _state = TransactionState.Running;

	public Signal OnFinished = new Signal();

	public Signal OnSuccess = new Signal();

	public Signal<Exception> OnFail = new Signal<Exception>();

	public Signal OnReadyForTransaction = new Signal();

	public Signal OnTransactionDenied = new Signal();

	public TransactionState State => _state;

	internal void ReadyForTransaction()
	{
		OnReadyForTransaction.Invoke();
	}

	internal void FailTransaction(Exception e)
	{
		_state = TransactionState.Fail;
		OnFail.Invoke(e);
		OnFinished.Invoke();
	}

	internal virtual void CompleteTransaction()
	{
		_state = TransactionState.Success;
		OnSuccess.Invoke();
		OnFinished.Invoke();
	}
}
