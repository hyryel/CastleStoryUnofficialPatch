using System;
using System.Collections;
using System.Collections.Generic;
using Brix.Engine;
using Brix.Utils;
using Brix.Utils.Closures;
using Smooth.Slinq;

namespace Brix.Game.Components.Volumes.Requests;

public struct SinglePropertyClosedFilteredRequest<TFunc> : IEnumerable<IVolume>, IEnumerable where TFunc : IFunc<IVolume, bool>
{
	public struct Enumerator : IEnumerator<IVolume>, IDisposable, IEnumerator
	{
		private SinglePropertyClosedFilteredRequest<TFunc> _request;

		private List<IVolume>.Enumerator _currentEnumerator;

		object IEnumerator.Current => Current;

		public IVolume Current => _currentEnumerator.Current;

		public Enumerator(SinglePropertyClosedFilteredRequest<TFunc> request)
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
				if (_request._predicate.Invoke(current))
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

	private TFunc _predicate;

	public SinglePropertyClosedFilteredRequest(XYZ position, VProperty vProperty, TFunc predicate)
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
