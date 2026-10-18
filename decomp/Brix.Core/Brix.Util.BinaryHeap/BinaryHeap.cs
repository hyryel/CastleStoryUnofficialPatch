using System;
using System.Collections.Generic;
using Brix.Util.Paire;
using UnityEngine;

namespace Brix.Util.BinaryHeap;

public class BinaryHeap
{
	public int[] listA;

	public int[] listB;

	public int[] listC;

	public int qte;

	public int max;

	private int m;

	private int demi;

	private int temp;

	private int u;

	private int v;

	private Brix.Util.Paire.Paire _out;

	public BinaryHeap(int a)
	{
		listA = new int[a + 1];
		listB = new int[a + 1];
		listC = new int[a + 1];
		_out = new Brix.Util.Paire.Paire(0, 0);
	}

	public void Reinitialiser()
	{
		qte = 0;
		max = 0;
		m = 0;
		demi = 0;
		temp = 0;
		_out = new Brix.Util.Paire.Paire(0, 0);
		u = 0;
		v = 0;
	}

	public void Add(int d, int i)
	{
		max++;
		qte++;
		listA[qte] = max;
		listB[max] = d;
		listC[max] = i;
		m = qte;
		while (m != 1)
		{
			demi = Mathf.FloorToInt(m / 2);
			if (listB[listA[m]] <= listB[listA[demi]])
			{
				temp = listA[demi];
				listA[demi] = listA[m];
				listA[m] = temp;
				m = demi;
				continue;
			}
			break;
		}
	}

	public Brix.Util.Paire.Paire Take()
	{
		_out.a = listB[listA[1]];
		_out.b = listC[listA[1]];
		listA[1] = listA[qte];
		qte--;
		v = 1;
		while (true)
		{
			u = v;
			if (2 * u + 1 <= qte)
			{
				if (listB[listA[u]] >= listB[listA[2 * u]])
				{
					v = 2 * u;
				}
				if (listB[listA[v]] >= listB[listA[2 * u + 1]])
				{
					v = 2 * u + 1;
				}
			}
			else if (2 * u <= qte && listB[listA[u]] >= listB[listA[2 * u]])
			{
				v = 2 * u;
			}
			if (u != v)
			{
				temp = listA[u];
				listA[u] = listA[v];
				listA[v] = temp;
				continue;
			}
			break;
		}
		return _out;
	}

	public Brix.Util.Paire.Paire Check()
	{
		_out.a = listB[listA[1]];
		_out.b = listC[listA[1]];
		return _out;
	}

	public string Print()
	{
		string text = "classeur";
		if (qte > 0)
		{
			for (int i = 1; i < qte; i++)
			{
				text = text + ";    " + listB[listA[i]] + "(" + listC[listA[i]] + ")";
			}
		}
		return text;
	}

	public void Grandire()
	{
		int[] array = new int[listA.Length * 2];
		listA.CopyTo(array, 0);
		listA = (int[])array.Clone();
		listB.CopyTo(array, 0);
		listB = (int[])array.Clone();
		listC.CopyTo(array, 0);
		listC = array;
	}

	public int[] Clone(int[] array)
	{
		int[] array2 = new int[array.Length];
		array.CopyTo(array2, 0);
		return array2;
	}
}
public class BinaryHeap<TValue>
{
	public int[] listA;

	public int[] listB;

	public TValue[] listC;

	public int qte;

	public int max;

	private int m;

	private int demi;

	private int temp;

	private int u;

	private int v;

	public BinaryHeap(int a)
	{
		listA = new int[a + 1];
		listB = new int[a + 1];
		listC = new TValue[a + 1];
	}

	public void Reinitialiser()
	{
		for (int i = 1; i <= qte; i++)
		{
			listC[i] = default(TValue);
		}
		qte = 0;
		max = 0;
		m = 0;
		demi = 0;
		temp = 0;
		u = 0;
		v = 0;
	}

	public void Add(int priority, TValue value)
	{
		max++;
		qte++;
		listA[qte] = max;
		listB[max] = priority;
		listC[max] = value;
		m = qte;
		while (m != 1)
		{
			demi = m / 2;
			if (listB[listA[m]] <= listB[listA[demi]])
			{
				temp = listA[demi];
				listA[demi] = listA[m];
				listA[m] = temp;
				m = demi;
				continue;
			}
			break;
		}
	}

	public KeyValuePair<int, TValue> Take()
	{
		int key = listB[listA[1]];
		TValue value = listC[listA[1]];
		listC[listA[1]] = default(TValue);
		RemoveFirst();
		return new KeyValuePair<int, TValue>(key, value);
	}

	public TValue TakeValue()
	{
		TValue result = listC[listA[1]];
		listC[listA[1]] = default(TValue);
		RemoveFirst();
		return result;
	}

	public KeyValuePair<int, TValue> Check()
	{
		int key = listB[listA[1]];
		TValue value = listC[listA[1]];
		return new KeyValuePair<int, TValue>(key, value);
	}

	private void RemoveFirst()
	{
		listA[1] = listA[qte];
		qte--;
		v = 1;
		while (true)
		{
			u = v;
			if (2 * u + 1 <= qte)
			{
				if (listB[listA[u]] >= listB[listA[2 * u]])
				{
					v = 2 * u;
				}
				if (listB[listA[v]] >= listB[listA[2 * u + 1]])
				{
					v = 2 * u + 1;
				}
			}
			else if (2 * u <= qte && listB[listA[u]] >= listB[listA[2 * u]])
			{
				v = 2 * u;
			}
			if (u != v)
			{
				temp = listA[u];
				listA[u] = listA[v];
				listA[v] = temp;
				continue;
			}
			break;
		}
	}

	public string Print()
	{
		string text = "classeur";
		if (qte > 0)
		{
			for (int i = 1; i < qte; i++)
			{
				text = text + ";    " + listB[listA[i]] + " -> " + listC[listA[i]];
			}
		}
		return text;
	}

	public void MakeRoomForMore()
	{
		int num = max + 1;
		if (num >= listA.Length - 1)
		{
			Resize(num * 2);
		}
	}

	public void RemoveExcessRoom()
	{
		int num = qte;
		if (num < (listA.Length - 1) / 4)
		{
			Resize(num * 2);
		}
	}

	public void Grandire()
	{
		Resize(listA.Length * 2);
	}

	public void Resize(int n)
	{
		Array.Resize(ref listA, n);
		Array.Resize(ref listB, n);
		Array.Resize(ref listC, n);
	}
}
