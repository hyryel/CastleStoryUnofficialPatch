using System;
using System.Collections;
using System.Collections.Generic;
using Brix.Utils;

namespace Brix.Game.Components.Volumes.Requests;

public struct MultiPropertyMultiVoxelRequest : IEnumerable<IVolume>, IEnumerable
{
	public struct Enumerator : IEnumerator<IVolume>, IDisposable, IEnumerator
	{
		private MultiPropertyMultiVoxelRequest _request;

		private RegistryEntry _currentEntry;

		private RegistryEntry.Enumerator _entryEnumerator;

		private VolumeMultiPropertyRequest.Enumerator _positionEnumerator;

		private HashSet<IVolume> _volumesEncountered;

		object IEnumerator.Current => Current;

		public IVolume Current => _entryEnumerator.Current;

		public Enumerator(MultiPropertyMultiVoxelRequest request)
		{
			_request = request;
			_positionEnumerator = request._positions.GetEnumerator();
			_currentEntry = RegistryEntry.EmptyEntry;
			_entryEnumerator = _currentEntry.GetVolumesWith(~VPropertyMask.All);
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
				if (_entryEnumerator.MoveNext())
				{
					IVolume current = _entryEnumerator.Current;
					if (!IsNewValue(current) || !_request._predicate(current))
					{
						continue;
					}
					return true;
				}
				if (!_positionEnumerator.MoveNext())
				{
					break;
				}
				_currentEntry = VolumeRegistry._data.IndexedEntries.GetValueOrDefault(_positionEnumerator.Current, RegistryEntry.EmptyEntry);
				_entryEnumerator.Dispose();
				_entryEnumerator = _currentEntry.GetVolumesWith(_request._vProperties);
			}
			return false;
		}

		public void Reset()
		{
		}

		public void Dispose()
		{
			_entryEnumerator.Dispose();
			_volumesEncountered.Dispose();
		}

		private bool IsNewValue(IVolume v)
		{
			return _volumesEncountered == null || _volumesEncountered.Add(v);
		}
	}

	private VolumeMultiPropertyRequest _positions;

	private VPropertyMask _vProperties;

	private Func<IVolume, bool> _predicate;

	private IVolume _excludedVolume;

	public MultiPropertyMultiVoxelRequest(VolumeMultiPropertyRequest positions, VPropertyMask vProperties, Func<IVolume, bool> predicate, IVolume excluded = null)
	{
		_positions = positions;
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
