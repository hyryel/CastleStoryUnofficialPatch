using System;
using System.Collections;
using System.Collections.Generic;

namespace Brix.Util;

public static class ArrayEnumerator
{
	public struct OffsetRange2D : IEnumerator<int>, IDisposable, IEnumerator
	{
		private int _y0;

		private int _rw;

		private int _rh;

		private int _yFactor;

		private int i;

		private int next;

		object IEnumerator.Current => i;

		public int Current => i;

		public OffsetRange2D(int x0, int y0, int rw, int rh, int yFactor)
		{
			_y0 = y0;
			_rw = rw;
			_rh = rh;
			_yFactor = yFactor;
			int num = y0 * yFactor + x0;
			i = num - 1;
			next = num + rw;
		}

		public bool MoveNext()
		{
			i++;
			if (i == next)
			{
				i += _yFactor - _rw;
				if (i >= (_y0 + _rh) * _yFactor)
				{
					return false;
				}
				next = i + _rw;
			}
			return true;
		}

		public void Dispose()
		{
		}

		public void Reset()
		{
			throw new NotSupportedException();
		}
	}

	public struct XY
	{
		public int x;

		public int y;
	}

	public struct CoordRange2D : IEnumerator<XY>, IDisposable, IEnumerator
	{
		private int _x0;

		private int _y0;

		private int _rw;

		private int _rh;

		private int x;

		private int y;

		private int next;

		object IEnumerator.Current => new XY
		{
			x = x,
			y = y
		};

		public XY Current => new XY
		{
			x = x,
			y = y
		};

		public CoordRange2D(int x0, int y0, int rw, int rh, int yFactor)
		{
			_x0 = x0;
			_y0 = y0;
			_rw = rw;
			_rh = rh;
			x = x0 - 1;
			y = y0 - 1;
			next = x0;
		}

		public bool MoveNext()
		{
			x++;
			if (x == next)
			{
				x = _x0;
				y++;
				if (y >= _y0 + _rh)
				{
					return false;
				}
				next = x + _rw;
			}
			return true;
		}

		public void Dispose()
		{
		}

		public void Reset()
		{
			throw new NotSupportedException();
		}
	}

	public struct ValueRange2D<T> : IEnumerator<T>, IDisposable, IEnumerator
	{
		private IList<T> _array;

		private OffsetRange2D _offsets;

		object IEnumerator.Current => _array[_offsets.Current];

		public T Current => _array[_offsets.Current];

		public ValueRange2D(IList<T> array, int x0, int y0, int rw, int rh, int yFactor)
			: this(array, new OffsetRange2D(x0, y0, rw, rh, yFactor))
		{
		}

		public ValueRange2D(IList<T> array, OffsetRange2D offsets)
		{
			_array = array;
			_offsets = offsets;
		}

		public bool MoveNext()
		{
			return _offsets.MoveNext();
		}

		public void Dispose()
		{
			_offsets.Dispose();
		}

		public void Reset()
		{
			_offsets.Reset();
		}
	}
}
