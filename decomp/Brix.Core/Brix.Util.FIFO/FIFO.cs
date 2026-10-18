namespace Brix.Util.FIFO;

public class FIFO
{
	public int[] data;

	public int qte;

	public int debut;

	public FIFO()
	{
		data = new int[1000];
		qte = 0;
		debut = 0;
	}

	public void Add(int a)
	{
		qte++;
		if (debut + qte >= data.Length)
		{
			Grandire();
		}
		data[debut + qte - 1] = a;
	}

	public int Check()
	{
		if (qte >= 0)
		{
			return data[debut];
		}
		return -1;
	}

	public int Pop()
	{
		if (qte < 0)
		{
			return -1;
		}
		qte--;
		debut++;
		return data[debut - 1];
	}

	public void Grandire()
	{
		int[] array = new int[data.Length * 2];
		for (int i = 0; i < data.Length; i++)
		{
			array[i] = data[i];
		}
		data = array;
	}

	public void Reinitialiser()
	{
		qte = 0;
		debut = 0;
	}

	public int Count()
	{
		return qte;
	}

	public int[] Get()
	{
		int[] array = new int[qte];
		for (int i = 0; i < qte; i++)
		{
			array[i] = data[debut + i];
		}
		return array;
	}
}
