using System;
using System.Collections.Generic;

public class Coquille
{
	public static int Parcourir<T>(T position, Func<T, T> next, Func<T, bool> inBounds, Func<T, int> sign)
	{
		while (inBounds(position))
		{
			int num = sign(position);
			if (num != 0)
			{
				return num;
			}
			position = next(position);
		}
		return 0;
	}
}
public class Coquille<T>
{
	private Func<T, int> local;

	private Func<T, T> next;

	private Func<int, int> nextLocal;

	private Func<T, T, int> overflow;

	private int size;

	private int max;

	private Marqueur connus;

	private Marqueur pleins;

	public Coquille(int size, Func<T, int> local, Func<T, T> next, Func<int, int> nextLocal, Func<T, T, int> overflow)
	{
		max = size * size * size;
		connus = new Marqueur(max);
		pleins = new Marqueur(max);
		this.local = local;
		this.next = next;
		this.nextLocal = nextLocal;
		this.overflow = overflow;
	}

	public void Ajout(T position, bool plein)
	{
		int i = local(position);
		connus.Marquer(i);
		if (plein)
		{
			pleins.Marquer(i);
		}
		else
		{
			pleins.Enlever(i);
		}
	}

	public void AjoutSiAbsent(T position, bool plein)
	{
		int i = local(position);
		if (connus.Libre(i))
		{
			connus.Marquer(i);
			if (plein)
			{
				pleins.Marquer(i);
			}
			else
			{
				pleins.Enlever(i);
			}
		}
	}

	public void Effacer(T position)
	{
		int i = local(position);
		connus.Enlever(i);
	}

	public int Parcourir(T position)
	{
		return Parcourir(position, position);
	}

	public int Parcourir(T position, T origin)
	{
		List<int> colonne = new List<int>();
		int num = Coquille.Parcourir(local(position), delegate(int j)
		{
			colonne.Add(j);
			position = next(position);
			return nextLocal(j);
		}, (int j) => j >= 0 && j < max, (int j) => (!connus.Libre(j)) ? ((!pleins.Libre(j)) ? 1 : (-1)) : 0);
		if (num == 0)
		{
			num = overflow(position, origin);
			if (num == 0)
			{
				return 0;
			}
		}
		foreach (int item in colonne)
		{
			connus.Marquer(item);
		}
		if (num > 0)
		{
			foreach (int item2 in colonne)
			{
				pleins.Marquer(item2);
			}
		}
		return num;
	}
}
