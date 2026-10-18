using System;
using System.Collections;
using System.Collections.Generic;
using Brix.Engine;
using Brix.Utils;
using Smooth.Slinq;

namespace Brix.Game.Components.Volumes.Requests;

public struct SinglePropertyFilteredRequest : IEnumerable<IVolume>, IEnumerable
{
	public struct Enumerator : IEnumerator<IVolume>, IDisposable, IEnumerator
	{
		private SinglePropertyFilteredRequest _request;

		private List<IVolume>.Enumerator _currentEnumerator;

		object IEnumerator.Current => Current;

		public IVolume Current => _currentEnumerator.Current;

		public Enumerator(SinglePropertyFilteredRequest request)
		{
			_request = request;
			if (VolumeRegistry._data.IndexedEntries.TryGetValue(request._position, out var value))
			{
				_currentEnumerator = value.GetVolumesWith(request._vProperty).GetEnumerator();
			}
			else
			{
				_currentEnumerator = Empty.List<IVolume>().GetEnumerator();
			}
		}

		public bool MoveNext()
		{
			while (_currentEnumerator.MoveNext())
			{
				IVolume current = _currentEnumerator.Current;
				if (_request._predicate(current))
				{
					return true;
				}
			}
			return false;
		}

		public void Reset()
		{
		}

		public void Dispose()
		{
			_currentEnumerator.Dispose();
		}
	}

	private XYZ _position;

	private VProperty _vProperty;

	private Func<IVolume, bool> _predicate;

	public SinglePropertyFilteredRequest(XYZ position, VProperty vProperty, Func<IVolume, bool> predicate)
	{
		_position = position;
		_vProperty = vProperty;
		_predicate = predicate;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator<IVolume> IEnumerable<IVolume>.GetEnumerator()
	{
		return GetEnumerator();
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	public Slinq<IVolume, TemplatedEnumeratorContext<IVolume, Enumerator>> Slinq()
	{
		return TemplatedEnumeratorContext<IVolume, Enumerator>.Slinq(GetEnumerator());
	}
}
public struct SinglePropertyFilteredRequest<TArgs> : IEnumerable<IVolume>, IEnumerable
{
	public struct Enumerator : IEnumerator<IVolume>, IDisposable, IEnumerator
	{
		private SinglePropertyFilteredRequest<TArgs> _request;

		private List<IVolume>.Enumerator _currentEnumerator;

		object IEnumerator.Current => Current;

		public IVolume Current => _currentEnumerator.Current;

		public Enumerator(SinglePropertyFilteredRequest<TArgs> request)
		{
			_request = request;
			if (VolumeRegistry._data.IndexedEntries.TryGetValue(request._position, out var value))
			{
				_currentEnumerator = value.GetVolumesWith(request._vProperty).GetEnumerator();
			}
			else
			{
				_currentEnumerator = Empty.List<IVolume>().GetEnumerator();
			}
		}

		public bool MoveNext()
		{
			while (_currentEnumerator.MoveNext())
			{
				IVolume current = _currentEnumerator.Current;
				if (_request._predicate(current, _request._args))
				{
					return true;
				}
			}
			return false;
		}

		public void Reset()
		{
		}

		public void Dispose()
		{
			_currentEnumerator.Dispose();
		}
	}

	private XYZ _position;

	private VProperty _vProperty;

	private Func<IVolume, TArgs, bool> _predicate;

	private TArgs _args;

	public SinglePropertyFilteredRequest(XYZ position, VProperty vProperty, Func<IVolume, TArgs, bool> predicate, TArgs args)
	{
		_position = position;
		_vProperty = vProperty;
		_predicate = predicate;
		_args = args;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator<IVolume> IEnumerable<IVolume>.GetEnumerator()
	{
		return GetEnumerator();
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	public Slinq<IVolume, TemplatedEnumeratorContext<IVolume, Enumerator>> Slinq()
	{
		return TemplatedEnumeratorContext<IVolume, Enumerator>.Slinq(GetEnumerator());
	}
}
