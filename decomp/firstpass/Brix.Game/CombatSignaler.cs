using Brix.External.Signals;
using UnityEngine;

namespace Brix.Game;

public class CombatSignaler : MonoBehaviour
{
	public Signal Enter = new Signal();

	public Signal Exit = new Signal();

	private bool _flag;

	private bool Flag => false;

	private void Start()
	{
		OnFlagChange();
	}

	private void Update()
	{
		TryOnFlagChange();
	}

	private void TryOnFlagChange()
	{
		if (_flag != Flag)
		{
			OnFlagChange();
		}
	}

	private void OnFlagChange()
	{
		_flag = Flag;
		if (_flag)
		{
			Enter.Invoke();
		}
		else
		{
			Exit.Invoke();
		}
	}
}
