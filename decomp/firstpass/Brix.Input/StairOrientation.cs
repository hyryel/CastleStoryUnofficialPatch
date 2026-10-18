using System;
using System.Collections.Generic;
using Brix.Engine;

namespace Brix.Input;

[Serializable]
public class StairOrientation
{
	public XYZ corner;

	public XYZ direction;

	public static StairOrientation Invalid = new StairOrientation(XYZ.zero, XYZ.zero);

	public static List<StairOrientation> All = new List<StairOrientation>
	{
		new StairOrientation(new XYZ(1, 0, 1), new XYZ(0, 0, -1)),
		new StairOrientation(new XYZ(1, 0, 1), new XYZ(-1, 0, 0)),
		new StairOrientation(new XYZ(1, 0, -1), new XYZ(-1, 0, 0)),
		new StairOrientation(new XYZ(1, 0, -1), new XYZ(0, 0, 1)),
		new StairOrientation(new XYZ(-1, 0, -1), new XYZ(0, 0, 1)),
		new StairOrientation(new XYZ(-1, 0, -1), new XYZ(1, 0, 0)),
		new StairOrientation(new XYZ(-1, 0, 1), new XYZ(1, 0, 0)),
		new StairOrientation(new XYZ(-1, 0, 1), new XYZ(0, 0, -1))
	};

	public static StairOrientation Default => All[0];

	private StairOrientation()
	{
		corner = Default.corner;
		direction = Default.direction;
	}

	private StairOrientation(XYZ _corner, XYZ _direction)
	{
		corner = _corner;
		direction = _direction;
	}

	public StairOrientation Next()
	{
		StairOrientation item = From(corner, direction);
		int num = All.IndexOf(item);
		int index = (num + 1) % 8;
		return All[index];
	}

	public static StairOrientation From(XYZ corner, XYZ direction)
	{
		foreach (StairOrientation item in All)
		{
			if (item.corner == corner && item.direction == direction)
			{
				return item;
			}
		}
		return Default;
	}

	public override string ToString()
	{
		return "Stair orientation no : " + All.IndexOf(this);
	}
}
