using Brix.Engine;

namespace Brix.Pathfinding;

public class PathPositionAccepter : PathAccepter
{
	public XYZ RepresentativePosition;

	private PathLink result;

	public PathLink Result => result;

	public PathPositionAccepter(XYZ representativePosition)
	{
		RepresentativePosition = representativePosition;
	}

	public void Reset(XYZ representativePosition)
	{
		RepresentativePosition = representativePosition;
		result = null;
	}

	public override bool Visit(PathLink link, PathLink fromSource)
	{
		if (link.head.Node.RepresentativePosition != RepresentativePosition)
		{
			return false;
		}
		result = link.Copy;
		return true;
	}
}
