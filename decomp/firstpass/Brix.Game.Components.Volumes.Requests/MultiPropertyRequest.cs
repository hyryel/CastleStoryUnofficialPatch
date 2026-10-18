using System;
using System.Collections;
using System.Collections.Generic;
using Brix.Engine;
using Brix.Utils;
using Smooth.Slinq;

namespace Brix.Game.Components.Volumes.Requests;

public struct MultiPropertyRequest : IEnumerable<IVolume>, IEnumerable
{
	public struct Enumerator : IEnumerator<IVolume>, IDisposable, IEnumerator
	{
		private MultiPropertyRequest _request;

		private RegistryEntry _currentEntry;

		private RegistryEntry.Enumerator _currentEnumerator;

		private HashSet<IVolume> _volumesEncountered;

		object IEnumerator.Current => Current;

		public IVolume Current => _currentEnumerator.Current;

		public Enumerator(MultiPropertyRequest request)
		{
			_request = request;
			_currentEntry = VolumeRegistry._data.IndexedEntries.GetValueOrDefault(request._position, RegistryEntry.EmptyEntry);
			_currentEnumerator = _currentEntry.GetVolumesWith(request._vProperties);
			_volumesEncountered = CollectionPool.RequestHashSet<IVolume>();
			if (request._excludedVolume != null)
			{
				_volumesEncountered.Add(request._excludedVolume);
			}
		}

		public bool MoveNext()
		{
			while (_currentEnumerator.MoveNext())
			{
				IVolume current = _currentEnumerator.Current;
				if (IsNewValue(current) && _request._predicate(current))
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
			if (_volumesEncountered != null)
			{
				_volumesEncountered.Dispose();
			}
		}

		private bool IsNewValue(IVolume v)
		{
			return _volumesEncountered == null || _volumesEncountered.Add(v);
		}
	}

	private XYZ _position;

	private VPropertyMask _vProperties;

	private Func<IVolume, bool> _predicate;

	private IVolume _excludedVolume;

	public MultiPropertyRequest(XYZ position, VPropertyMask vProperties, Func<IVolume, bool> predicate, IVolume excluded = null)
	{
		_position = position;
		_vProperties = vProperties;
		_predicate = predicate;
		_excludedVolume = excluded;
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
public struct MultiPropertyRequest<TArgs> : IEnumerable<IVolume>, IEnumerable
{
	public struct Enumerator : IEnumerator<IVolume>, IDisposable, IEnumerator
	{
		private MultiPropertyRequest<TArgs> _request;

		private RegistryEntry _currentEntry;

		private RegistryEntry.Enumerator _currentEnumerator;

		private HashSet<IVolume> _volumesEncountered;

		object IEnumerator.Current => Current;

		public IVolume Current => _currentEnumerator.Current;

		public Enumerator(MultiPropertyRequest<TArgs> request)
		{
			_request = request;
			_currentEntry = VolumeRegistry._data.IndexedEntries.GetValueOrDefault(request._position, RegistryEntry.EmptyEntry);
			_currentEnumerator = _currentEntry.GetVolumesWith(request._vProperties);
			_volumesEncountered = CollectionPool.RequestHashSet<IVolume>();
			if (request._excludedVolume != null)
			{
				_volumesEncountered.Add(request._excludedVolume);
			}
		}

		public bool MoveNext()
		{
			while (_currentEnumerator.MoveNext())
			{
				IVolume current = _currentEnumerator.Current;
				if (IsNewValue(current) && _request._predicate(current, _request._args))
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
			if (_volumesEncountered != null)
			{
				_volumesEncountered.Dispose();
			}
		}

		private bool IsNewValue(IVolume v)
		{
			return _volumesEncountered == null || _volumesEncountered.Add(v);
		}
	}

	private XYZ _position;

	private VPropertyMask _vProperties;

	private Func<IVolume, TArgs, bool> _predicate;

	private TArgs _args;

	private IVolume _excludedVolume;

	public MultiPropertyRequest(XYZ position, VPropertyMask vProperties, Func<IVolume, TArgs, bool> predicate, TArgs args, IVolume excluded = null)
	{
		_position = position;
		_vProperties = vProperties;
		_predicate = predicate;
		_args = args;
		_excludedVolume = excluded;
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
