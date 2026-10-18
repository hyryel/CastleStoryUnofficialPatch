using System.Collections.Generic;
using UnityEngine;

namespace LibNoise.Operator;

public class Curve : ModuleBase
{
	private readonly List<KeyValuePair<double, double>> _data = new List<KeyValuePair<double, double>>();

	public int ControlPointCount => _data.Count;

	public List<KeyValuePair<double, double>> ControlPoints => _data;

	public Curve()
		: base(1)
	{
	}

	public Curve(ModuleBase input)
		: base(1)
	{
		base.Modules[0] = input;
	}

	public void Add(double input, double output)
	{
		KeyValuePair<double, double> item = new KeyValuePair<double, double>(input, output);
		if (!_data.Contains(item))
		{
			_data.Add(item);
		}
		_data.Sort((KeyValuePair<double, double> lhs, KeyValuePair<double, double> rhs) => lhs.Key.CompareTo(rhs.Key));
	}

	public void Clear()
	{
		_data.Clear();
	}

	public override double GetValue(double x, double y, double z)
	{
		double value = base.Modules[0].GetValue(x, y, z);
		int i;
		for (i = 0; i < _data.Count && !(value < _data[i].Key); i++)
		{
		}
		int index = Mathf.Clamp(i - 2, 0, _data.Count - 1);
		int num = Mathf.Clamp(i - 1, 0, _data.Count - 1);
		int num2 = Mathf.Clamp(i, 0, _data.Count - 1);
		int index2 = Mathf.Clamp(i + 1, 0, _data.Count - 1);
		if (num == num2)
		{
			return _data[num].Value;
		}
		double key = _data[num].Key;
		double key2 = _data[num2].Key;
		double position = (value - key) / (key2 - key);
		return Utils.InterpolateCubic(_data[index].Value, _data[num].Value, _data[num2].Value, _data[index2].Value, position);
	}
}
