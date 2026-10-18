using System;
using UnityEngine;

namespace Brix.NewUI.BreadBar;

public class Crumb
{
	public GameObject _original;

	public Action<IPlug>[] _plugActions;

	public Crumb(GameObject original, Action<IPlug>[] plugActions)
	{
		_original = original;
		_plugActions = plugActions;
	}
}
