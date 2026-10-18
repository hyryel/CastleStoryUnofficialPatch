using System;
using System.Collections.Generic;
using System.Linq;

namespace Brix.NewUI.GameModes;

public class GameModeVariable
{
	public enum dataType
	{
		Bool,
		Enum,
		Int,
		Float,
		String
	}

	public string luaKey;

	public dataType type;

	public List<string> possibleValues;

	public string toolTip;

	public GameModeVariable(string luaKey, string type, string possibleValues, string tooltip)
	{
		this.luaKey = luaKey;
		this.possibleValues = ((possibleValues != null) ? possibleValues.Split(',').ToList() : new List<string>());
		this.type = stringToDataType(type);
		toolTip = tooltip;
	}

	private dataType stringToDataType(string str)
	{
		return str switch
		{
			"bool" => dataType.Bool, 
			"enum" => dataType.Enum, 
			"int" => dataType.Int, 
			"float" => dataType.Float, 
			"string" => dataType.String, 
			_ => throw new Exception("Cannot convert type of item : " + luaKey), 
		};
	}

	public override string ToString()
	{
		string empty = string.Empty;
		string text = empty;
		empty = string.Concat(text, luaKey, ", ", type, ", ", toolTip);
		empty += ", possibleValues: ";
		if (possibleValues != null)
		{
			foreach (string possibleValue in possibleValues)
			{
				empty = empty + possibleValue + ", ";
			}
		}
		else
		{
			empty += "null";
		}
		return empty;
	}
}
