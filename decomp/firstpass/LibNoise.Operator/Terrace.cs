using System;
using System.Collections.Generic;
using UnityEngine;

namespace LibNoise.Operator;

public class Terrace : ModuleBase
{
	private readonly List<double> _data = new List<double>();

	private bool _inverted;

	public int ControlPointCount => _data.Count;

	public List<double> ControlPoints => _data;

	public bool IsInverted
	{
		get
		{
			return _inverted;
		}
		set
		{
			_inverted = value;
		}
	}

	public Terrace()
		: base(1)
	{
	}

	public Terrace(ModuleBase input)
		: base(1)
	{
		base.Modules[0] = input;
	}

	public Terrace(bool inverted, ModuleBase input)
		: base(1)
	{
		base.Modules[0] = input;
		IsInverted = inverted;
	}

	public void Add(double input)
	{
		if (!_data.Contains(input))
		{
			_data.Add(input);
		}
		_data.Sort((double lhs, double rhs) => lhs.CompareTo(rhs));
	}

	public void Clear()
	{
		_data.Clear();
	}

	public void Generate(int steps)
	{
		if (steps < 2)
		{
			throw new ArgumentException("Need at least two steps");
		}
		Clear();
		double num = 2.0 / ((double)steps - 1.0);
		double num2 = -1.0;
		for (int i = 0; i < steps; i++)
		{
			Add(num2);
			num2 += num;
		}
	}

	public override double GetValue(double x, double y, double z)
	{
		double value = base.Modules[0].GetValue(x, y, z);
		int i;
		for (i = 0; i < _data.Count && !(value < _data[i]); i++)
		{
		}
		int num = Mathf.Clamp(i - 1, 0, _data.Count - 1);
		int num2 = Mathf.Clamp(i, 0, _data.Count - 1);
		if (num == num2)
		{
			return _data[num2];
		}
		double num3 = _data[num];
		double num4 = _data[num2];
		double num5 = (value - num3) / (num4 - num3);
		if (_inverted)
		{
			num5 = 1.0 - num5;
			double num6 = num3;
			num3 = num4;
			num4 = num6;
		}
		num5 *= num5;
		return Utils.InterpolateLinear(num3, num4, num5);
	}
}
