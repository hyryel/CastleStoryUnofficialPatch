using System;
using UnityEngine;

namespace Brix.Input;

[Serializable]
public class InputModeBundle
{
	[SerializeField]
	private int _mode;

	public InputModeBundle(params InputMode[] modes)
	{
		Add(modes);
	}

	public static implicit operator int(InputModeBundle inputModeBundle)
	{
		return inputModeBundle?._mode ?? 0;
	}

	public static implicit operator InputModeBundle(int mode)
	{
		InputModeBundle inputModeBundle = new InputModeBundle();
		inputModeBundle.Set(mode);
		return inputModeBundle;
	}

	public static int Bundle(params InputMode[] modes)
	{
		int num = 0;
		foreach (InputMode inputMode in modes)
		{
			num |= (int)inputMode;
		}
		return num;
	}

	public void Set(int mode)
	{
		_mode = mode;
	}

	public void Set(params InputMode[] modes)
	{
		_mode = 0;
		Add(modes);
	}

	public void Add(InputMode mode)
	{
		_mode |= (int)mode;
	}

	public void Add(params InputMode[] modes)
	{
		foreach (InputMode inputMode in modes)
		{
			_mode |= (int)inputMode;
		}
	}

	public void Remove(InputMode mode)
	{
		_mode &= (int)(~mode);
	}

	public void Remove(params InputMode[] modes)
	{
		foreach (InputMode inputMode in modes)
		{
			_mode &= (int)(~inputMode);
		}
	}

	public int GetModes()
	{
		return _mode;
	}

	public bool ContainsAll(params InputMode[] modes)
	{
		return (~_mode & Bundle(modes)) == 0;
	}

	public bool ContainsAny(params InputMode[] modes)
	{
		return (_mode & Bundle(modes)) != 0;
	}

	public bool ContainsAll(int mode)
	{
		return (~_mode & mode) == 0;
	}

	public bool ContainsAny(int mode)
	{
		return (_mode & mode) != 0;
	}

	public bool Contains(InputMode mode)
	{
		return ((uint)_mode & (uint)mode) != 0;
	}
}
