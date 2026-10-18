using System;
using System.Collections;
using Brix.Network;
using Brix.Utils.Listeners;
using Brix.Utils.Signals;
using UnityEngine;

namespace Brix.Transactions;

public class TransactionWithCoroutine : Transaction
{
	private readonly Transactor _transactor;

	private readonly float _delay;

	private bool _manualFail;

	private static MonoBehaviour _ngc;

	public TransactionWithCoroutine(Transactor transactor, TransactionMonitor monitor, TransactionPayload payload, float delay = 0f)
		: base(monitor, payload)
	{
		_transactor = transactor;
		_delay = delay;
		if (transactor == null)
		{
			FailTransaction(new NullReferenceException());
			return;
		}
		transactor.GetOrAddComponent<OnDestroyListener>().Signal.ConnectOnce(OnTransactorDestroyed).DisconnectWhen(monitor.OnFinished);
		StartCoroutine(Go());
	}

	public static void StartCoroutine(IEnumerator coroutine)
	{
		if (_ngc == null)
		{
			_ngc = UnityEngine.Object.FindObjectOfType<NetworkGameClient>();
		}
		_ngc.StartCoroutine(coroutine);
	}

	private IEnumerator Go()
	{
		yield return new WaitForSeconds(_delay);
		if (!_manualFail)
		{
			ReadyForTransaction();
			TryInvokePayload();
			CompleteTransaction();
		}
	}

	private void OnTransactorDestroyed()
	{
		_manualFail = true;
		FailTransaction("Transactor was destroyed before end of Transaction. Failing. (id=" + _transactor.Id + ")");
	}
}
