using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Brix.Engine;
using Brix.External.Factories;
using Brix.Game;
using Brix.Game.Components;
using Brix.Game.Network;
using Brix.Lifecycle.Pooling;
using Brix.Utils.Listeners;
using Brix.Utils.Signals;
using Motus.Behavior;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Transactions;

public class Transactor : MonoBehaviour
{
	private class ImmediateTransaction : Transaction
	{
		public ImmediateTransaction(TransactionMonitor monitor, TransactionPayload payload)
			: base(monitor, payload)
		{
			ReadyForTransaction();
			TryInvokePayload();
			CompleteTransaction();
		}
	}

	private class TransactionReseau : Transaction
	{
		private const double DefaultOperationDelay = 0.2;

		private readonly Transactor _transactor;

		private bool _manualFail;

		private static double Time => UnityEngine.Time.time;

		public TransactionReseau(Transactor transactor, TransactionMonitor monitor, TransactionPayload payload)
			: base(monitor, payload)
		{
			_transactor = transactor;
			SendTransactionPayload(transactor, payload, monitor, Time, Time + 0.2);
		}

		private TransactionReseau(Transactor transactor, TransactionMonitor monitor, TransactionPayload payload, double operationEndTime)
			: base(monitor, payload)
		{
			_transactor = transactor;
			if (transactor == null)
			{
				FailTransaction(new NullReferenceException());
				return;
			}
			GameSignals.Once(GameSignals.LevelUnloaded, OnSceneChanged).DisconnectWhen(monitor.OnFinished);
			transactor.GetOrAddComponent<OnDestroyListener>().Signal.ConnectOnce(OnTransactorDestroyed).DisconnectWhen(monitor.OnFinished);
			TransactionWithCoroutine.StartCoroutine(WaitForOperation(operationEndTime));
		}

		private void SendTransactionPayload(Transactor transactor, TransactionPayload payload, TransactionMonitor monitor, double operationStartTime, double operationEndTime)
		{
			if (transactor == null)
			{
				FailTransaction(new NullReferenceException());
				return;
			}
			GameSignals.Once(GameSignals.LevelUnloaded, OnSceneChanged).DisconnectWhen(monitor.OnFinished);
			transactor.GetOrAddComponent<OnDestroyListener>().Signal.ConnectOnce(OnTransactorDestroyed).DisconnectWhen(monitor.OnFinished);
			TransactionWithCoroutine.StartCoroutine(WaitForOperation(operationEndTime));
		}

		public static void OnOperationSynced(double operationEndTime, Transactor transactor, TransactionPayload payload)
		{
			TransactionMonitor monitor = payload.CreateMonitor();
			new TransactionReseau(transactor, monitor, payload, operationEndTime);
		}

		private IEnumerator WaitForOperation(double operationTime)
		{
			while (Time < operationTime)
			{
				yield return GameClock.WaitForEndOfFrame;
			}
			if (!_manualFail)
			{
				ReadyForTransaction();
				TryInvokePayload();
				CompleteTransaction();
			}
		}

		private void OnSceneChanged()
		{
			_manualFail = true;
			FailTransaction("Scene was unloaded before end of Transaction. Failing. (id=" + _transactor.Id + ")");
		}

		private void OnTransactorDestroyed()
		{
			_manualFail = true;
			FailTransaction("Transactor was destroyed before end of Transaction. Failing. (id=" + _transactor.Id + ")");
		}
	}

	public TransactionMode mode;

	private CompCache<IdentityHolder> _identity;

	public bool IsMine => true;

	public string Id => _identity.Get.Id;

	protected void Awake()
	{
		_identity = new CompCache<IdentityHolder>(base.gameObject);
	}

	public NetworkHandle<T> CreateHandle<T>(T t)
	{
		return NetworkLocator<T>.GetOrCreateLocator<CreationTimeRegistry<T>>(this).CreateHandle(t);
	}

	private TMonitor CreateMonitor<TMonitor, TResult>(TransactionPayload<TResult, TMonitor> payload) where TMonitor : TransactionMonitor<TResult>, new() where TResult : TransactionReturnValue, new()
	{
		TMonitor val = payload.CreateSpecializedMonitor();
		CreateTransaction(val, payload);
		return val;
	}

	private TransactionMonitor<TResult> CreateMonitor<TResult>(TransactionPayload<TResult> payload) where TResult : TransactionReturnValue, new()
	{
		TransactionMonitor<TResult> transactionMonitor = payload.CreateSpecializedMonitor();
		CreateTransaction(transactionMonitor, payload);
		return transactionMonitor;
	}

	private TransactionMonitor CreateMonitor(TransactionPayload payload)
	{
		TransactionMonitor transactionMonitor = payload.CreateMonitor();
		CreateTransaction(transactionMonitor, payload);
		return transactionMonitor;
	}

	private Transaction CreateTransaction(TransactionMonitor monitor, TransactionPayload payload)
	{
		switch (mode)
		{
		case TransactionMode.Network:
			return new TransactionReseau(this, monitor, payload);
		case TransactionMode.Immediate:
			return new ImmediateTransaction(monitor, payload);
		case TransactionMode.Autodetect:
		case TransactionMode.WithCoroutine:
			return new TransactionWithCoroutine(this, monitor, payload);
		default:
			throw new Exception("Invalid transaction mode");
		}
	}

	public void OnOperationSynced(double operationTime, TransactionPayload payload)
	{
		TransactionReseau.OnOperationSynced(operationTime, this, payload);
	}

	public TransactionMonitor ExecuteAnonymousMethod(MethodWrapper methodWrapper)
	{
		return CreateMonitor(new AnonymousMethodPayload(methodWrapper));
	}

	public TransactionMonitor<AnonymousReturnValue<T>> ExecuteAnonymousMethod<T>(MethodWrapper methodWrapper)
	{
		return CreateMonitor(new AnonymousMethodPayload<T>(methodWrapper));
	}

	public TransactionMonitor Destroy(GameObject gameObject)
	{
		DestroyTransactionPayload destroyTransactionPayload = new DestroyTransactionPayload();
		destroyTransactionPayload.gameObject = gameObject;
		DestroyTransactionPayload payload = destroyTransactionPayload;
		return CreateMonitor(payload);
	}

	public static GameObject SpawnUNet(Factory.AssetKey key, Faction faction, Vector3 position, Quaternion rotation, GameObject UserRequestor = null, bool spawnActive = true)
	{
		if (!ObjectPoolSingleton.Request(out var go, key, position, rotation, spawnActive))
		{
			return null;
		}
		AffiliationComponent component = go.GetComponent<AffiliationComponent>();
		if (component != null)
		{
			component.faction = faction;
			component.NetworkUserCreator = UserRequestor;
		}
		if ((bool)go.GetComponent<NetworkIdentity>() && NetworkServer.active)
		{
			NetworkServer.Spawn(go, key.AssetId);
		}
		return go;
	}

	public SpawnTransactionMonitor Spawn(Factory.AssetKey assetKey, Faction faction, Vector3 position, Quaternion rotation, params SpawnPostProcessor[] postProcessors)
	{
		return CreateMonitor(new SpawnTransactionPayloadAtPosition(assetKey, faction, position, rotation, postProcessors));
	}

	public IEnumerator<ComputationProgress<T>> SpawnNode<T>(Factory.AssetKey assetKey, Faction faction, Vector3 position, Quaternion rotation, params SpawnPostProcessor[] postProcessors) where T : Component
	{
		SpawnTransactionMonitor monitor = GetComponent<Transactor>().Spawn(assetKey, faction, position, rotation, postProcessors);
		yield return ComputationProgress<T>.Sub(Node.WaitForCondition(monitor.OnFinished));
		if (monitor.State == TransactionState.Fail)
		{
			yield return ComputationProgress<T>.Fail();
		}
		yield return ComputationProgress<T>.Return(monitor.ReturnValue.SpawnedInstance.GetComponent<T>());
	}

	public SpawnBatchTransactionMonitor<T> SpawnBatch<T>(Factory.AssetKey assetKey, Faction faction, Vector3[] positions, params SpawnPostProcessor[] postProcessors) where T : Component
	{
		IEnumerable<TransactionPayload<SpawnTransactionReturnValue>> source = Enumerable.Range(0, positions.Length).Select((Func<int, TransactionPayload<SpawnTransactionReturnValue>>)((int i) => new SpawnTransactionPayloadAtPosition(assetKey, faction, positions[i], Quaternion.identity, postProcessors)));
		return CreateMonitor(new SpawnBatchTransactionPayload<T>(source.ToArray()));
	}

	public TransactionMonitor ExecuteSynchedMethod(Action a)
	{
		return CreateMonitor(SynchedActionPayloadBase.Create(a));
	}

	public TransactionMonitor ExecuteSynchedMethod<T1>(Action<T1> a, T1 b)
	{
		return CreateMonitor(SynchedActionPayloadBase.Create(a, b));
	}

	public TransactionMonitor ExecuteSynchedMethod<T1, T2>(Action<T1, T2> a, T1 b, T2 c)
	{
		return CreateMonitor(SynchedActionPayloadBase.Create(a, b, c));
	}

	public TransactionMonitor ExecuteSynchedMethod<T1, T2, T3>(Action<T1, T2, T3> a, T1 b, T2 c, T3 d)
	{
		return CreateMonitor(SynchedActionPayloadBase.Create(a, b, c, d));
	}

	public TransactionMonitor ExecuteSynchedMethod<T1, T2, T3, T4>(Action<T1, T2, T3, T4> a, T1 b, T2 c, T3 d, T4 e)
	{
		return CreateMonitor(SynchedActionPayloadBase.Create(a, b, c, d, e));
	}

	public TransactionMonitor ExecuteSynchedMethod<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> a, T1 b, T2 c, T3 d, T4 e, T5 f)
	{
		return CreateMonitor(SynchedActionPayloadBase.Create(a, b, c, d, e, f));
	}

	public TransactionMonitor ExecuteSynchedMethod<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> a, T1 b, T2 c, T3 d, T4 e, T5 f, T6 g)
	{
		return CreateMonitor(SynchedActionPayloadBase.Create(a, b, c, d, e, f, g));
	}

	public TransactionMonitor<GenericReturnValue<T>> ExecuteSynchedMethod<T>(Func<T> func)
	{
		return CreateMonitor(SynchedFunctionPayload<T>.Create(func));
	}

	public TransactionMonitor<GenericReturnValue<T>> ExecuteSynchedMethod<A, T>(Func<A, T> func, A a)
	{
		return CreateMonitor(SynchedFunctionPayload<T>.Create(func, a));
	}

	public TransactionMonitor<GenericReturnValue<T>> ExecuteSynchedMethod<A, B, T>(Func<A, B, T> func, A a, B b)
	{
		return CreateMonitor(SynchedFunctionPayload<T>.Create(func, a, b));
	}

	public TransactionMonitor<GenericReturnValue<T>> ExecuteSynchedMethod<A, B, C, T>(Func<A, B, C, T> func, A a, B b, C c)
	{
		return CreateMonitor(SynchedFunctionPayload<T>.Create(func, a, b, c));
	}

	public TransactionMonitor<GenericReturnValue<T>> ExecuteSynchedMethod<A, B, C, D, T>(Func<A, B, C, D, T> func, A a, B b, C c, D d)
	{
		return CreateMonitor(SynchedFunctionPayload<T>.Create(func, a, b, c, d));
	}

	public TransactionMonitor<GenericReturnValue<T>> ExecuteSynchedMethod<A, B, C, D, E, T>(Func<A, B, C, D, E, T> func, A a, B b, C c, D d, E e)
	{
		return CreateMonitor(SynchedFunctionPayload<T>.Create(func, a, b, c, d, e));
	}

	public TransactionMonitor<GenericReturnValue<T>> ExecuteSynchedMethod<A, B, C, D, E, F, T>(Func<A, B, C, D, E, T> func, A a, B b, C c, D d, E e, F f)
	{
		return CreateMonitor(SynchedFunctionPayload<T>.Create(func, a, b, c, d, e, f));
	}
}
