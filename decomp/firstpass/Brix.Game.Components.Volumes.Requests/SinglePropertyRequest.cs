using System.Collections;
using System.Collections.Generic;
using Brix.Engine;
using Brix.Utils;
using Smooth.Slinq;
using Smooth.Slinq.Context;

namespace Brix.Game.Components.Volumes.Requests;

public struct SinglePropertyRequest : IEnumerable<IVolume>, IEnumerable
{
	private XYZ _position;

	private VProperty _vProperty;

	public SinglePropertyRequest(XYZ position, VProperty vProperty)
	{
		_position = position;
		_vProperty = vProperty;
	}

	private List<IVolume> GetEntry()
	{
		if (!VolumeRegistry._data.IndexedEntries.TryGetValue(_position, out var value))
		{
			return Empty.List<IVolume>();
		}
		return value.GetVolumesWith(_vProperty);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator<IVolume> IEnumerable<IVolume>.GetEnumerator()
	{
		return GetEnumerator();
	}

	public List<IVolume>.Enumerator GetEnumerator()
	{
		return GetEntry().GetEnumerator();
	}

	public Slinq<IVolume, IListContext<IVolume>> Slinq()
	{
		return GetEntry().Slinq();
	}
}
