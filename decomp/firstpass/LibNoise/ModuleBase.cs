using System;
using System.Xml.Serialization;
using UnityEngine;

namespace LibNoise;

public abstract class ModuleBase : IDisposable
{
	private ModuleBase[] _modules;

	[NonSerialized]
	[XmlIgnore]
	private bool _disposed;

	public virtual ModuleBase this[int index]
	{
		get
		{
			if (index < 0 || index >= _modules.Length)
			{
				throw new ArgumentOutOfRangeException("Index out of valid module range");
			}
			if (_modules[index] == null)
			{
				throw new ArgumentNullException("Desired element is null");
			}
			return _modules[index];
		}
		set
		{
			if (index < 0 || index >= _modules.Length)
			{
				throw new ArgumentOutOfRangeException("Index out of valid module range");
			}
			if (value == null)
			{
				throw new ArgumentNullException("Value should not be null");
			}
			_modules[index] = value;
		}
	}

	protected ModuleBase[] Modules => _modules;

	public int SourceModuleCount => (_modules != null) ? _modules.Length : 0;

	public bool IsDisposed => _disposed;

	protected ModuleBase(int count)
	{
		if (count > 0)
		{
			_modules = new ModuleBase[count];
		}
	}

	public abstract double GetValue(double x, double y, double z);

	public double GetValue(Vector3 coordinate)
	{
		return GetValue(coordinate.x, coordinate.y, coordinate.z);
	}

	public double GetValue(ref Vector3 coordinate)
	{
		return GetValue(coordinate.x, coordinate.y, coordinate.z);
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			_disposed = Disposing();
		}
		GC.SuppressFinalize(this);
	}

	protected virtual bool Disposing()
	{
		if (_modules != null)
		{
			for (int i = 0; i < _modules.Length; i++)
			{
				_modules[i].Dispose();
				_modules[i] = null;
			}
			_modules = null;
		}
		return true;
	}
}
