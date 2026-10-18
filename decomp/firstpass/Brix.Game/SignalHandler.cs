using Brix.Components;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game.AI;
using UnityEngine;

namespace Brix.Game;

public class SignalHandler : BrixComponent
{
	[SerializeField]
	private ScreenSignaler screenSignaler;

	private Signal _enterScreen;

	private Signal _exitScreen;

	private Signal _enterFog;

	private Signal _exitFog;

	private Signal _workEnded;

	private Signal<Occupation.Type, Factory.AssetKey> _spawnUnit = new Signal<Occupation.Type, Factory.AssetKey>();

	private Signal<GameObject> _changeTarget = new Signal<GameObject>();

	private Signal<Labor, GameObject> pathToObjectFailed = new Signal<Labor, GameObject>();

	public IReadOnlySignal EnterScreen
	{
		get
		{
			if (_enterScreen == null)
			{
				_enterScreen = new Signal();
			}
			return _enterScreen;
		}
	}

	public IReadOnlySignal ExitScreen
	{
		get
		{
			if (_exitScreen == null)
			{
				_exitScreen = new Signal();
			}
			return _exitScreen;
		}
	}

	public Signal EnterFog
	{
		get
		{
			if (_enterFog == null)
			{
				_enterFog = new Signal();
			}
			return _enterFog;
		}
	}

	public Signal ExitFog
	{
		get
		{
			if (_exitFog == null)
			{
				_exitFog = new Signal();
			}
			return _exitFog;
		}
	}

	public Signal WorkEnded
	{
		get
		{
			if (_workEnded == null)
			{
				_workEnded = new Signal();
			}
			return _workEnded;
		}
	}

	public Signal<Occupation.Type, Factory.AssetKey> SpawnUnit
	{
		get
		{
			if (_spawnUnit == null)
			{
				_spawnUnit = new Signal<Occupation.Type, Factory.AssetKey>();
			}
			return _spawnUnit;
		}
	}

	public Signal<GameObject> ChangeTarget
	{
		get
		{
			if (_changeTarget == null)
			{
				_changeTarget = new Signal<GameObject>();
			}
			return _changeTarget;
		}
	}

	public Signal<Labor, GameObject> PathToObjectFailed
	{
		get
		{
			if (pathToObjectFailed == null)
			{
				pathToObjectFailed = new Signal<Labor, GameObject>();
			}
			return pathToObjectFailed;
		}
	}

	private void DoEnterScreen()
	{
		((Signal)EnterScreen).Invoke();
	}

	private void DoExitScreen()
	{
		((Signal)ExitScreen).Invoke();
	}

	private void ClearAll()
	{
		if (_enterScreen != null)
		{
			_enterScreen.Clear();
		}
		if (_exitScreen != null)
		{
			_exitScreen.Clear();
		}
		if (_enterFog != null)
		{
			_enterFog.Clear();
		}
		if (_exitFog != null)
		{
			_exitFog.Clear();
		}
		if (_spawnUnit != null)
		{
			_spawnUnit.Clear();
		}
		if (_workEnded != null)
		{
			_workEnded.Clear();
		}
		if (_changeTarget != null)
		{
			_changeTarget.Clear();
		}
	}

	public override void Initialize()
	{
		base.Initialize();
		ClearAll();
	}

	public override void OnRequested()
	{
		base.OnRequested();
		if (screenSignaler != null)
		{
			screenSignaler.Enter.Connect(DoEnterScreen);
			screenSignaler.Exit.Connect(DoExitScreen);
		}
	}

	public override void ResetComponent()
	{
		base.ResetComponent();
		ClearAll();
	}
}
