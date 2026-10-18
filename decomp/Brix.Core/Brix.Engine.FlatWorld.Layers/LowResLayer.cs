using System.Collections.Generic;
using Brix.Utils;

namespace Brix.Engine.FlatWorld.Layers;

public class LowResLayer : Layer
{
	public virtual void Init(Layer parentLayer, XYZ scale)
	{
		initialized = true;
	}

	public virtual bool IsOccupiedOrUncertain(XYZ p)
	{
		return false;
	}

	public virtual bool IsVisibleOrUncertain(XYZ p)
	{
		return IsOccupiedOrUncertain(p);
	}

	public virtual bool IsFullyInternal(XYZ p)
	{
		return false;
	}

	public virtual IEnumerable<XYZ> GetOccupiedOrUncertain()
	{
		return Empty.List<XYZ>();
	}

	public virtual IEnumerable<XYZ> GetFullyOccupied()
	{
		return Empty.List<XYZ>();
	}

	public virtual IEnumerable<XYZ> GetOccupiedOrUncertain(XYZ start, XYZ size)
	{
		return Empty.List<XYZ>();
	}

	public virtual XYZ GetOccupiedOrUncertainTotalSize(out XYZ start)
	{
		start = XYZ.zero;
		return XYZ.zero;
	}

	public virtual IEnumerable<XYZ> GetVisibleOrUncertain()
	{
		return GetOccupiedOrUncertain();
	}

	public virtual IEnumerable<XYZ> GetVisibleOrUncertain(XYZ start, XYZ size)
	{
		return GetOccupiedOrUncertain(start, size);
	}
}
