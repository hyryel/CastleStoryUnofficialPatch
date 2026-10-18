using System.Collections.Generic;
using System.Linq;
using Brix.Engine.Volumes;
using Brix.External.Signals;
using Newtonsoft.Json;

namespace Brix.Engine.FlatWorld.Layers;

[JsonObject(MemberSerialization.OptIn)]
public class ColumnPointSet
{
	private readonly Dictionary<XYZ, IntClasse> _removedSet = new Dictionary<XYZ, IntClasse>();

	public Signal<XYZ> AddSignal = new Signal<XYZ>();

	public Signal<XYZ> RemoveSignal = new Signal<XYZ>();

	public Signal<XZ> RemoveColumnSignal = new Signal<XZ>();

	public Signal ClearSignal = new Signal();

	[JsonProperty]
	public XYZ[] points
	{
		get
		{
			return GetPoints().ToArray();
		}
		set
		{
			_removedSet.Clear();
			AddCustomRegion(new VoxelSet(value));
		}
	}

	public void AddCustomRegion(IVoxelVolume region)
	{
		region.ApplyNow(delegate(XYZ p)
		{
			Add(p);
		});
	}

	public int ColumnCount()
	{
		return _removedSet.Count;
	}

	public bool Add(XYZ p)
	{
		IntClasse intClasse = _removedSet.NewKey(new XYZ(p.x, 0, p.z));
		if (intClasse.Ajout(p.y) < 0)
		{
			return false;
		}
		AddSignal.Invoke(p);
		return true;
	}

	public bool Remove(XYZ p)
	{
		IntClasse valueOrDefault = _removedSet.GetValueOrDefault(new XYZ(p.x, 0, p.z));
		if (valueOrDefault == null || valueOrDefault.Retrait(p.y) < 0)
		{
			return false;
		}
		RemoveSignal.Invoke(p);
		return true;
	}

	public bool RemoveColumn(XZ p)
	{
		XYZ key = new XYZ(p.x, 0, p.z);
		if (!_removedSet.TryGetValue(key, out var value))
		{
			return false;
		}
		_removedSet.Remove(key);
		if (value.Empty)
		{
			return false;
		}
		RemoveColumnSignal.Invoke(p);
		return true;
	}

	public void Clear()
	{
		_removedSet.Clear();
		ClearSignal.Invoke();
	}

	public bool Contains(XYZ p)
	{
		return _removedSet.GetValueOrDefault(new XYZ(p.x, 0, p.z))?.Contains(p.y) ?? false;
	}

	public IEnumerable<XYZ> GetPoints()
	{
		return _removedSet.SelectMany((KeyValuePair<XYZ, IntClasse> kv) => MakePoints(kv));
	}

	public IEnumerable<KeyValuePair<XYZ, IntClasse>> GetColumns()
	{
		return _removedSet;
	}

	public IntClasse GetColumn(XZ p)
	{
		return _removedSet.GetValueOrDefault(new XYZ(p.x, 0, p.z));
	}

	public IntClasse GetOrCreateColumn(XZ p)
	{
		return _removedSet.NewKey(new XYZ(p.x, 0, p.z));
	}

	private IEnumerable<XYZ> MakePoints(KeyValuePair<XYZ, IntClasse> kv)
	{
		return MakePoints(kv.Key, kv.Value);
	}

	private IEnumerable<XYZ> MakePoints(XYZ p, IntClasse col)
	{
		return col.Select((int y) => new XYZ(p.x, y, p.z));
	}
}
