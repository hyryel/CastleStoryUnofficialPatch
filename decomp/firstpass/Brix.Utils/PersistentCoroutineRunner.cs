using System;
using System.Collections;
using System.Threading;
using Brix.Engine.Abstract;
using Brix.External.Signals;
using UnityEngine;

namespace Brix.Utils;

public class PersistentCoroutineRunner : MonoBehaviour
{
	private static int _frameCount;

	private static string FrameState = string.Empty;

	public static PersistentCoroutineRunner Instance;

	private IEnumerator _onEndOfFrameCoroutine;

	private static readonly WaitForEndOfFrame TmpWaitForEndOfFrame = new WaitForEndOfFrame();

	private readonly Signal _updateSignal = new Signal();

	private readonly Signal _lateUpdateSignal = new Signal();

	private readonly Signal _lateUpdate1Signal = new Signal();

	private readonly Signal _onEndOfFrameSignalRelais = new Signal();

	private readonly Signal _onEndOfFrameSignal = new Signal();

	private static readonly WaitForSeconds TmpWaitForOneSecond = new WaitForSeconds(1f);

	public static string FrameCount => $"[{_frameCount.ToString()} @ {FrameState}]:: ";

	public static IReadOnlySignal UpdateSignal => Instance._updateSignal;

	public static IReadOnlySignal LateUpdateSignal => Instance._lateUpdateSignal;

	public static IReadOnlySignal LateUpdate1Signal => Instance._lateUpdate1Signal;

	public static IReadOnlySignal OnEndOfFrameSignal => Instance._onEndOfFrameSignal;

	private void Awake()
	{
		Instance = this;
	}

	public static void Relais(Action f)
	{
		Instance._onEndOfFrameSignalRelais.ConnectOnce(f);
	}

	public static void Relais<T>(Action<T> f, T t)
	{
		Instance._onEndOfFrameSignalRelais.ConnectOnce(f, t);
	}

	public static void Relais<T1, T2>(Action<T1, T2> f, T1 t1, T2 t2)
	{
		Instance._onEndOfFrameSignalRelais.ConnectOnce(f, t1, t2);
	}

	public static void Relais<T1, T2, T3>(Action<T1, T2, T3> f, T1 t1, T2 t2, T3 t3)
	{
		Instance._onEndOfFrameSignalRelais.ConnectOnce(f, t1, t2, t3);
	}

	public void Relais(Action f, float d)
	{
		StartCoroutine(Neo.AppelDynamique(f, d));
	}

	private void OnEnable()
	{
		_onEndOfFrameCoroutine = _onEndOfFrameCoroutine ?? OnEndOfFrameCoroutine();
		StartCoroutine(_onEndOfFrameCoroutine);
	}

	private void OnDisable()
	{
		StopCoroutine(_onEndOfFrameCoroutine);
		_onEndOfFrameSignalRelais.Clear();
	}

	private IEnumerator OnEndOfFrameCoroutine()
	{
		while (true)
		{
			yield return TmpWaitForEndOfFrame;
			FrameState = "Eof";
			_onEndOfFrameSignalRelais.Invoke();
			_onEndOfFrameSignal.Invoke();
			_frameCount++;
		}
	}

	private void Update()
	{
		FrameState = "Update";
		_updateSignal.Invoke();
	}

	private void LateUpdate()
	{
		FrameState = "LateUpdate";
		_lateUpdateSignal.Invoke();
		FrameState = "LateUpdate1";
		_lateUpdate1Signal.Invoke();
	}

	public static IEnumerator CallbackWhenThreadEnd(Thread thread, Action action)
	{
		while (thread.IsAlive)
		{
			yield return TmpWaitForOneSecond;
		}
		action();
	}
}
