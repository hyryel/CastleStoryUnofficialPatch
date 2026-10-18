using System;
using Brix.Utils.Logging;

namespace Brix.Transactions;

public abstract class Transaction
{
	private class FailedTransactionException : Exception
	{
		public FailedTransactionException(string message)
			: base(message)
		{
		}
	}

	private static BrixLogger logger = new BrixLogger();

	protected readonly TransactionMonitor _monitor;

	protected readonly TransactionPayload _payload;

	protected Transaction(TransactionMonitor monitor, TransactionPayload payload)
	{
		_monitor = monitor;
		_payload = payload;
		_monitor.OnFail.Connect(LogError);
	}

	protected void ReadyForTransaction()
	{
		_monitor.ReadyForTransaction();
	}

	protected void TryInvokePayload()
	{
		try
		{
			_payload.Invoke();
		}
		catch (Exception e)
		{
			_monitor.FailTransaction(e);
		}
	}

	protected void FailTransaction(Exception e)
	{
		_monitor.FailTransaction(e);
	}

	protected void FailTransaction(string message)
	{
		_monitor.FailTransaction(new FailedTransactionException(message));
	}

	protected void CompleteTransaction()
	{
		_monitor.CompleteTransaction();
	}

	private void LogError(Exception e)
	{
		if (e is FailedTransactionException)
		{
			logger.Info(e);
		}
		else if (e != null)
		{
			logger.Error(e, "Transaction failed");
		}
	}
}
