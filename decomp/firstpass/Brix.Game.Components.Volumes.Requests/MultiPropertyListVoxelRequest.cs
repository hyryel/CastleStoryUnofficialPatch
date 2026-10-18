using System;
using System.Collections;
using System.Collections.Generic;
using Brix.Engine;
using Brix.Utils;

namespace Brix.Game.Components.Volumes.Requests;

public struct MultiPropertyListVoxelRequest : IEnumerable<IVolume>, IEnumerable
{
	public struct Enumerator : IEnumerator<IVolume>, IDisposable, IEnumerator
	{
		private MultiPropertyListVoxelRequest _request;

		private int _positionIndex;

		private RegistryEntry _currentEntry;

		private RegistryEntry.Enumerator _currentEnumerator;

		private HashSet<IVolume> _volumesEncountered;

		object IEnumerator.Current => Current;

		public IVolume Current => _currentEnumerator.Current;

		public Enumerator(MultiPropertyListVoxelRequest request)
		{
			this = default(Enumerator);
			_request = request;
			_positionIndex = -1;
			_currentEntry = RegistryEntry.EmptyEntry;
			_currentEnumerator = _currentEntry.GetVolumesWith(~VPropertyMask.All);
			_volumesEncountered = CollectionPool.RequestHashSet<IVolume>();
			if (request._excludedVolume != null)
			{
				_volumesEncountered.Add(request._excludedVolume);
			}
		}

		public bool MoveNext()
		{
			while (true)
			{
				if (_currentEnumerator.MoveNext())
				{
					IVolume current = _currentEnumerator.Current;
					if (!IsNewValue(current) || !_request._predicate(current))
					{
						continue;
					}
					return true;
				}
				_positionIndex++;
				if (_positionIndex >= _request._positions.Count)
				{
					break;
				}
				_currentEntry = VolumeRegistry._data.IndexedEntries.GetValueOrDefault(_request._positions[_positionIndex], RegistryEntry.EmptyEntry);
				_currentEnumerator.Dispose();
				_currentEnumerator = _currentEntry.GetVolumesWith(_request._vProperties);
			}
			return false;
		}

		public void Reset()
		{
		}

		public void Dispose()
		{
			if (_currentEntry != null)
			{
				_currentEnumerator.Dispose();
			}
			_volumesEncountered.Dispose();
		}

		private bool IsNewValue(IVolume v)
		{
			return _volumesEncountered == null || _volumesEncountered.Add(v);
		}
	}

	private IList<XYZ> _positions;

	private VPropertyMask _vProperties;

	private Func<IVolume, bool> _predicate;

	private IVolume _excludedVolume;

	public MultiPropertyListVoxelRequest(IList<XYZ> position, VPropertyMask vProperties, Func<IVolume, bool> predicate, IVolume excluded = null)
	{
		_positions = position;
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
}
