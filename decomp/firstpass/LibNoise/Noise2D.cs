using System;
using System.Xml.Serialization;
using UnityEngine;

namespace LibNoise;

public class Noise2D : IDisposable
{
	public static readonly double South = -90.0;

	public static readonly double North = 90.0;

	public static readonly double West = -180.0;

	public static readonly double East = 180.0;

	public static readonly double AngleMin = -180.0;

	public static readonly double AngleMax = 180.0;

	public static readonly double Left = -1.0;

	public static readonly double Right = 1.0;

	public static readonly double Top = -1.0;

	public static readonly double Bottom = 1.0;

	private int _width;

	private int _height;

	private float[,] _data;

	private readonly int _ucWidth;

	private readonly int _ucHeight;

	private int _ucBorder = 1;

	private readonly float[,] _ucData;

	private float _borderValue = float.NaN;

	private ModuleBase _generator;

	[NonSerialized]
	[XmlIgnore]
	private bool _disposed;

	public float this[int x, int y, bool isCropped = true]
	{
		get
		{
			if (isCropped)
			{
				if (x < 0 && x >= _width)
				{
					throw new ArgumentOutOfRangeException("Invalid x position");
				}
				if (y < 0 && y >= _height)
				{
					throw new ArgumentOutOfRangeException("Invalid y position");
				}
				return _data[x, y];
			}
			if (x < 0 && x >= _ucWidth)
			{
				throw new ArgumentOutOfRangeException("Invalid x position");
			}
			if (y < 0 && y >= _ucHeight)
			{
				throw new ArgumentOutOfRangeException("Invalid y position");
			}
			return _ucData[x, y];
		}
		set
		{
			if (isCropped)
			{
				if (x < 0 && x >= _width)
				{
					throw new ArgumentOutOfRangeException("Invalid x position");
				}
				if (y < 0 && y >= _height)
				{
					throw new ArgumentOutOfRangeException("Invalid y position");
				}
				_data[x, y] = value;
			}
			else
			{
				if (x < 0 && x >= _ucWidth)
				{
					throw new ArgumentOutOfRangeException("Invalid x position");
				}
				if (y < 0 && y >= _ucHeight)
				{
					throw new ArgumentOutOfRangeException("Invalid y position");
				}
				_ucData[x, y] = value;
			}
		}
	}

	public float Border
	{
		get
		{
			return _borderValue;
		}
		set
		{
			_borderValue = value;
		}
	}

	public ModuleBase Generator
	{
		get
		{
			return _generator;
		}
		set
		{
			_generator = value;
		}
	}

	public int Height => _height;

	public int Width => _width;

	public bool IsDisposed => _disposed;

	protected Noise2D()
	{
	}

	public Noise2D(int size)
		: this(size, size)
	{
	}

	public Noise2D(int size, ModuleBase generator)
		: this(size, size, generator)
	{
	}

	public Noise2D(int width, int height, ModuleBase generator = null)
	{
		_generator = generator;
		_width = width;
		_height = height;
		_data = new float[width, height];
		_ucWidth = width + _ucBorder * 2;
		_ucHeight = height + _ucBorder * 2;
		_ucData = new float[width + _ucBorder * 2, height + _ucBorder * 2];
	}

	public float[,] GetNormalizedData(bool isCropped = true, int xCrop = 0, int yCrop = 0)
	{
		return GetData(isCropped, xCrop, yCrop, isNormalized: true);
	}

	public float[,] GetData(bool isCropped = true, int xCrop = 0, int yCrop = 0, bool isNormalized = false)
	{
		float[,] array;
		int num;
		int num2;
		if (isCropped)
		{
			num = _width;
			num2 = _height;
			array = _data;
		}
		else
		{
			num = _ucWidth;
			num2 = _ucHeight;
			array = _ucData;
		}
		num -= xCrop;
		num2 -= yCrop;
		float[,] array2 = new float[num, num2];
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				float num3 = ((!isNormalized) ? array[i, j] : ((array[i, j] + 1f) / 2f));
				array2[i, j] = num3;
			}
		}
		return array2;
	}

	public void Clear(float value = 0f)
	{
		for (int i = 0; i < _width; i++)
		{
			for (int j = 0; j < _height; j++)
			{
				_data[i, j] = value;
			}
		}
	}

	private double GeneratePlanar(double x, double y)
	{
		return _generator.GetValue(x, 0.0, y);
	}

	public void GeneratePlanar(double left, double right, double top, double bottom, bool isSeamless = true)
	{
		if (right <= left || bottom <= top)
		{
			throw new ArgumentException("Invalid right/left or bottom/top combination");
		}
		if (_generator == null)
		{
			throw new ArgumentNullException("Generator is null");
		}
		double num = right - left;
		double num2 = bottom - top;
		double num3 = num / ((double)_width - (double)_ucBorder);
		double num4 = num2 / ((double)_height - (double)_ucBorder);
		double num5 = left;
		for (int i = 0; i < _ucWidth; i++)
		{
			double num6 = top;
			for (int j = 0; j < _ucHeight; j++)
			{
				float num7;
				if (isSeamless)
				{
					num7 = (float)GeneratePlanar(num5, num6);
				}
				else
				{
					double a = GeneratePlanar(num5, num6);
					double b = GeneratePlanar(num5 + num, num6);
					double a2 = GeneratePlanar(num5, num6 + num2);
					double b2 = GeneratePlanar(num5 + num, num6 + num2);
					double position = 1.0 - (num5 - left) / num;
					double position2 = 1.0 - (num6 - top) / num2;
					double a3 = Utils.InterpolateLinear(a, b, position);
					double b3 = Utils.InterpolateLinear(a2, b2, position);
					num7 = (float)Utils.InterpolateLinear(a3, b3, position2);
				}
				_ucData[i, j] = num7;
				if (i >= _ucBorder && j >= _ucBorder && i < _width + _ucBorder && j < _height + _ucBorder)
				{
					_data[i - _ucBorder, j - _ucBorder] = num7;
				}
				num6 += num4;
			}
			num5 += num3;
		}
	}

	private double GenerateCylindrical(double angle, double height)
	{
		double x = Math.Cos(angle * 0.01745329238474369);
		double z = Math.Sin(angle * 0.01745329238474369);
		return _generator.GetValue(x, height, z);
	}

	public void GenerateCylindrical(double angleMin, double angleMax, double heightMin, double heightMax)
	{
		if (angleMax <= angleMin || heightMax <= heightMin)
		{
			throw new ArgumentException("Invalid angle or height parameters");
		}
		if (_generator == null)
		{
			throw new ArgumentNullException("Generator is null");
		}
		double num = angleMax - angleMin;
		double num2 = heightMax - heightMin;
		double num3 = num / ((double)_width - (double)_ucBorder);
		double num4 = num2 / ((double)_height - (double)_ucBorder);
		double num5 = angleMin;
		for (int i = 0; i < _ucWidth; i++)
		{
			double num6 = heightMin;
			for (int j = 0; j < _ucHeight; j++)
			{
				_ucData[i, j] = (float)GenerateCylindrical(num5, num6);
				if (i >= _ucBorder && j >= _ucBorder && i < _width + _ucBorder && j < _height + _ucBorder)
				{
					_data[i - _ucBorder, j - _ucBorder] = (float)GenerateCylindrical(num5, num6);
				}
				num6 += num4;
			}
			num5 += num3;
		}
	}

	private double GenerateSpherical(double lat, double lon)
	{
		double num = Math.Cos(0.01745329238474369 * lat);
		return _generator.GetValue(num * Math.Cos(0.01745329238474369 * lon), Math.Sin(0.01745329238474369 * lat), num * Math.Sin(0.01745329238474369 * lon));
	}

	public void GenerateSpherical(double south, double north, double west, double east)
	{
		if (east <= west || north <= south)
		{
			throw new ArgumentException("Invalid east/west or north/south combination");
		}
		if (_generator == null)
		{
			throw new ArgumentNullException("Generator is null");
		}
		double num = east - west;
		double num2 = north - south;
		double num3 = num / ((double)_width - (double)_ucBorder);
		double num4 = num2 / ((double)_height - (double)_ucBorder);
		double num5 = west;
		for (int i = 0; i < _ucWidth; i++)
		{
			double num6 = south;
			for (int j = 0; j < _ucHeight; j++)
			{
				_ucData[i, j] = (float)GenerateSpherical(num6, num5);
				if (i >= _ucBorder && j >= _ucBorder && i < _width + _ucBorder && j < _height + _ucBorder)
				{
					_data[i - _ucBorder, j - _ucBorder] = (float)GenerateSpherical(num6, num5);
				}
				num6 += num4;
			}
			num5 += num3;
		}
	}

	public Texture2D GetTexture()
	{
		return GetTexture(GradientPresets.Grayscale);
	}

	public Texture2D GetTexture(Gradient gradient)
	{
		Texture2D texture2D = new Texture2D(_width, _height);
		Color[] array = new Color[_width * _height];
		for (int i = 0; i < _width; i++)
		{
			for (int j = 0; j < _height; j++)
			{
				float num = ((float.IsNaN(_borderValue) || (i != 0 && i != _width - _ucBorder && j != 0 && j != _height - _ucBorder)) ? _data[i, j] : _borderValue);
				ref Color reference = ref array[i + j * _width];
				reference = gradient.Evaluate((num + 1f) / 2f);
			}
		}
		texture2D.SetPixels(array);
		texture2D.wrapMode = TextureWrapMode.Clamp;
		texture2D.Apply();
		return texture2D;
	}

	public Texture2D GetNormalMap(float intensity)
	{
		Texture2D texture2D = new Texture2D(_width, _height);
		Color[] array = new Color[_width * _height];
		for (int i = 0; i < _ucWidth; i++)
		{
			for (int j = 0; j < _ucHeight; j++)
			{
				float num = (_ucData[Mathf.Max(0, i - _ucBorder), j] - _ucData[Mathf.Min(i + _ucBorder, _height + _ucBorder), j]) / 2f;
				float num2 = (_ucData[i, Mathf.Max(0, j - _ucBorder)] - _ucData[i, Mathf.Min(j + _ucBorder, _width + _ucBorder)]) / 2f;
				Vector3 vector = new Vector3(num * intensity, 0f, 1f);
				Vector3 vector2 = new Vector3(0f, num2 * intensity, 1f);
				Vector3 vector3 = vector + vector2;
				vector3.Normalize();
				Vector3 zero = Vector3.zero;
				zero.x = (vector3.x + 1f) / 2f;
				zero.y = (vector3.y + 1f) / 2f;
				zero.z = (vector3.z + 1f) / 2f;
				if (i >= _ucBorder && j >= _ucBorder && i < _width + _ucBorder && j < _height + _ucBorder)
				{
					ref Color reference = ref array[i - _ucBorder + (j - _ucBorder) * _width];
					reference = new Color(zero.x, zero.y, zero.z);
				}
			}
		}
		texture2D.SetPixels(array);
		texture2D.wrapMode = TextureWrapMode.Clamp;
		texture2D.Apply();
		return texture2D;
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			_disposed = Disposing();
		}
		GC.SuppressFinalize(this);
	}

	protected virtual bool Disposing()
	{
		_data = null;
		_width = 0;
		_height = 0;
		return true;
	}
}
