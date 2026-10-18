using System;

namespace Brix.Util.Quatuor;

public class ArrayQuatuor
{
	public Quatuor[] data;

	public int index;

	public ArrayQuatuor(int i)
	{
		data = new Quatuor[i];
		index = -1;
	}

	public void Add(Quatuor a)
	{
		index++;
		if (index >= data.Length)
		{
			Grandire();
		}
		data[index] = a;
	}

	public void Grandire()
	{
		Quatuor[] array = new Quatuor[data.Length * 2];
		for (int i = 0; i < data.Length; i++)
		{
			array[i] = data[i];
		}
		data = array;
	}

	public void Reinitialiser()
	{
		index = -1;
	}

	public int Count()
	{
		return index + 1;
	}

	public Quatuor[] Get()
	{
		Quatuor[] array = new Quatuor[Count()];
		Array.Copy(data, array, Count());
		return array;
	}

	public void Sort()
	{
		Array.Sort(data, 0, Count(), null);
	}
}
