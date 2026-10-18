namespace System.Numerics;

public struct Complex : IEquatable<Complex>, IFormattable
{
	private double real;

	private double imaginary;

	public static readonly Complex ImaginaryOne = new Complex(0.0, 1.0);

	public static readonly Complex One = new Complex(1.0, 0.0);

	public static readonly Complex Zero = new Complex(0.0, 0.0);

	public double Imaginary => imaginary;

	public double Real => real;

	public double Magnitude => Math.Sqrt(imaginary * imaginary + real * real);

	public double Phase => Math.Atan2(imaginary, real);

	public Complex(double real, double imaginary)
	{
		this.imaginary = imaginary;
		this.real = real;
	}

	public static Complex FromPolarCoordinates(double magnitude, double phase)
	{
		return new Complex(magnitude * Math.Cos(phase), magnitude * Math.Sin(phase));
	}

	public static Complex operator +(Complex left, Complex right)
	{
		return new Complex(left.real + right.real, left.imaginary + right.imaginary);
	}

	public static Complex Add(Complex left, Complex right)
	{
		return new Complex(left.real + right.real, left.imaginary + right.imaginary);
	}

	public static Complex operator -(Complex left, Complex right)
	{
		return new Complex(left.real - right.real, left.imaginary - right.imaginary);
	}

	public static Complex Subtract(Complex left, Complex right)
	{
		return new Complex(left.real - right.real, left.imaginary - right.imaginary);
	}

	public static Complex operator *(Complex left, Complex right)
	{
		return new Complex(left.real * right.real - left.imaginary * right.imaginary, left.real * right.imaginary + left.imaginary * right.real);
	}

	public static Complex Multiply(Complex left, Complex right)
	{
		return new Complex(left.real * right.real - left.imaginary * right.imaginary, left.real * right.imaginary + left.imaginary * right.real);
	}

	public static Complex operator /(Complex left, Complex right)
	{
		double num = right.real * right.real + right.imaginary * right.imaginary;
		return new Complex((left.real * right.real + left.imaginary * right.imaginary) / num, (left.imaginary * right.real - left.real * right.imaginary) / num);
	}

	public static Complex Divide(Complex dividend, Complex divisor)
	{
		double num = divisor.real * divisor.real + divisor.imaginary * divisor.imaginary;
		return new Complex((dividend.real * divisor.real + dividend.imaginary * divisor.imaginary) / num, (dividend.imaginary * divisor.real - dividend.real * divisor.imaginary) / num);
	}

	public static bool operator ==(Complex left, Complex right)
	{
		return left.real == right.real && left.imaginary == right.imaginary;
	}

	public bool Equals(Complex value)
	{
		return real == value.real && imaginary == value.imaginary;
	}

	public override bool Equals(object obj)
	{
		if (obj == null || !(obj is Complex))
		{
			return false;
		}
		Complex complex = (Complex)obj;
		return real == complex.real && imaginary == complex.imaginary;
	}

	public static bool operator !=(Complex left, Complex right)
	{
		return left.real != right.real || left.imaginary != right.imaginary;
	}

	public static Complex operator -(Complex value)
	{
		return new Complex(0.0 - value.real, 0.0 - value.imaginary);
	}

	public static implicit operator Complex(byte value)
	{
		return new Complex((int)value, 0.0);
	}

	public static implicit operator Complex(double value)
	{
		return new Complex(value, 0.0);
	}

	public static implicit operator Complex(short value)
	{
		return new Complex(value, 0.0);
	}

	public static implicit operator Complex(int value)
	{
		return new Complex(value, 0.0);
	}

	public static implicit operator Complex(long value)
	{
		return new Complex(value, 0.0);
	}

	public static implicit operator Complex(float value)
	{
		return new Complex(value, 0.0);
	}

	public static explicit operator Complex(decimal value)
	{
		return new Complex((double)value, 0.0);
	}

	public static double Abs(Complex value)
	{
		return Math.Sqrt(value.imaginary * value.imaginary + value.real * value.real);
	}

	public static Complex Conjugate(Complex value)
	{
		return new Complex(value.real, 0.0 - value.imaginary);
	}

	public static Complex Cos(Complex value)
	{
		return new Complex(Math.Cos(value.real) * Math.Cosh(value.imaginary), (0.0 - Math.Sin(value.real)) * Math.Sinh(value.imaginary));
	}

	public static Complex Cosh(Complex value)
	{
		return new Complex(Math.Cosh(value.real) * Math.Cos(value.imaginary), (0.0 - Math.Sinh(value.real)) * Math.Sin(value.imaginary));
	}

	public static Complex Negate(Complex value)
	{
		return -value;
	}

	public static Complex Sin(Complex value)
	{
		return new Complex(Math.Sin(value.real) * Math.Cosh(value.imaginary), Math.Cos(value.real) * Math.Sinh(value.imaginary));
	}

	public static Complex Sinh(Complex value)
	{
		return new Complex(Math.Sinh(value.real) * Math.Cos(value.imaginary), Math.Cosh(value.real) * Math.Sin(value.imaginary));
	}

	public static Complex Reciprocal(Complex value)
	{
		if (value == Zero)
		{
			return value;
		}
		return One / value;
	}

	public static Complex Tan(Complex value)
	{
		return Sin(value) / Cos(value);
	}

	public static Complex Tanh(Complex value)
	{
		return Sinh(value) / Cosh(value);
	}

	public static Complex Acos(Complex value)
	{
		return -ImaginaryOne * Log(value + ImaginaryOne * Sqrt(One - value * value));
	}

	public static Complex Asin(Complex value)
	{
		return -ImaginaryOne * Log(ImaginaryOne * value + Sqrt(One - value * value));
	}

	public static Complex Atan(Complex value)
	{
		return ImaginaryOne / new Complex(2.0, 0.0) * (Log(One - ImaginaryOne * value) - Log(One + ImaginaryOne * value));
	}

	public static Complex Exp(Complex value)
	{
		double num = Math.Exp(value.real);
		return new Complex(num * Math.Cos(value.imaginary), num * Math.Sin(value.imaginary));
	}

	public static Complex Log(Complex value)
	{
		return new Complex(Math.Log(Abs(value)), value.Phase);
	}

	public static Complex Log(Complex value, double baseValue)
	{
		return Log(value) / Log(new Complex(baseValue, 0.0));
	}

	public static Complex Log10(Complex value)
	{
		return Log(value, 10.0);
	}

	public static Complex Sqrt(Complex value)
	{
		return FromPolarCoordinates(Math.Sqrt(value.Magnitude), value.Phase / 2.0);
	}

	public static Complex Pow(Complex value, double power)
	{
		return Pow(value, new Complex(power, 0.0));
	}

	public static Complex Pow(Complex value, Complex power)
	{
		return Exp(Log(value) * power);
	}

	public override int GetHashCode()
	{
		return real.GetHashCode() ^ imaginary.GetHashCode();
	}

	public override string ToString()
	{
		return $"({real}, {imaginary})";
	}

	public string ToString(IFormatProvider provider)
	{
		return string.Format(provider, "({0}, {1})", real, imaginary);
	}

	public string ToString(string format)
	{
		return $"({real.ToString(format)}, {imaginary.ToString(format)})";
	}

	public string ToString(string format, IFormatProvider provider)
	{
		return $"({real.ToString(format, provider)}, {imaginary.ToString(format, provider)})";
	}
}
