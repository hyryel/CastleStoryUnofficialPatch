using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class IntClasse : IClasseur<int>, IEnumerable<int>, IEnumerable
{
	protected int[] a;

	protected int count;

	public int this[int i]
	{
		get
		{
			return a[i];
		}
		set
		{
			a[i] = value;
		}
	}

	public int Count => count;

	public int Length => count;

	public int length => count;

	public int Last => a[count - 1];

	public bool Empty => count == 0;

	public bool Full => a.Length == count;

	public int AvailableSpace => a.Length - count;

	protected IntClasse(int[] a, int count)
	{
		this.a = a;
		this.count = count;
	}

	public IntClasse()
	{
		Clear();
	}

	public IntClasse(IEnumerable<int> list)
	{
		if (list is int[])
		{
			a = list as int[];
			count = a.Length;
		}
		else if (list != null)
		{
			a = list.ToArray();
			count = a.Length;
		}
		else
		{
			Clear();
		}
	}

	public static implicit operator int[](IntClasse a)
	{
		return a.ToBuiltin();
	}

	public static implicit operator IntClasse(int[] a)
	{
		return new IntClasse(a);
	}

	public static explicit operator Classeur<int>(IntClasse a)
	{
		return Classeur<int>.FromArraySlice(a.a, a.count);
	}

	public static explicit operator IntClasse(Classeur<int> a)
	{
		return new IntClasse(a.GetInternalArray(), a.Count);
	}

	public static IntClasse FromArraySlice(int[] array, int n)
	{
		return new IntClasse(array, n);
	}

	public void Alloc(int size)
	{
		Array.Resize(ref a, size);
		count = size;
	}

	public int Add(int x)
	{
		if (count >= a.Length)
		{
			Array.Resize(ref a, (a.Length == 0) ? 1 : (a.Length << 1));
		}
		a[count] = x;
		return count++;
	}

	public int Concat(IntClasse liste)
	{
		return Concat(liste, liste.count);
	}

	public int Concat(IntClasse liste, int listeCount)
	{
		int num = count + listeCount;
		if (num > a.Length)
		{
			int num2 = ((a.Length == 0) ? 1 : (a.Length << 1));
			while (num > num2)
			{
				num2 <<= 1;
			}
			Array.Resize(ref a, num2);
		}
		Array.Copy(liste.a, 0, a, count, listeCount);
		count += listeCount;
		return count - listeCount;
	}

	public int Push(int x)
	{
		Add(x);
		return count;
	}

	public int Pop()
	{
		return a[--count];
	}

	public void Swap(int i, int j)
	{
		int num = a[i];
		a[i] = a[j];
		a[j] = num;
	}

	public void Insert(int i, int x)
	{
		if (count >= a.Length)
		{
			Array.Resize(ref a, (a.Length == 0) ? 1 : (a.Length << 1));
		}
		if (i < count)
		{
			Array.Copy(a, i, a, i + 1, count - i);
		}
		a[i] = x;
		count++;
	}

	public bool Remove(int x)
	{
		for (int i = 0; i < count; i++)
		{
			if (a[i] == x)
			{
				RemoveAt(i);
				return true;
			}
		}
		return false;
	}

	public void RemoveAt(int i)
	{
		if (count >= 1 && i >= 0 && i < count)
		{
			count--;
			Array.Copy(a, i + 1, a, i, count - i);
		}
	}

	public void RemoveRange(int i, int n)
	{
		if (count <= n)
		{
			Clear();
			return;
		}
		count -= n;
		Array.Copy(a, i + n, a, i, count - i);
	}

	public void Clear()
	{
		Clear(realloc: true);
	}

	public void Clear(bool realloc)
	{
		count = 0;
		if (realloc)
		{
			a = new int[8];
		}
	}

	public void ReadFrom(IntClasse other)
	{
		count = other.count;
		a = other.a;
	}

	public IntClasse Clone()
	{
		return new IntClasse((int[])a.Clone(), count);
	}

	public void Sort()
	{
		Array.Sort(a, 0, count);
	}

	public int[] ToBuiltin()
	{
		Array.Resize(ref a, count);
		return a;
	}

	public int[] ToBuiltin(Type someT)
	{
		return ToBuiltin();
	}

	public int[] GetInternalArray()
	{
		return a;
	}

	public void SetInternalArray(int[] a)
	{
		this.a = a;
	}

	public void ClearInternalArrayPadding()
	{
		if (count < a.Length)
		{
			Array.Clear(a, count, a.Length - count);
		}
	}

	public void RemoveInternalArrayPadding()
	{
		if (count < a.Length)
		{
			Array.Resize(ref a, count);
		}
	}

	public int BinarySearch(int x)
	{
		if (count == 0 || x > a[count - 1])
		{
			return ~count;
		}
		int i = 0;
		for (int num = count >> 1; num > 2; num >>= 1)
		{
			if (a[i + num] <= x)
			{
				i += num;
			}
		}
		for (; a[i] < x; i++)
		{
		}
		return (a[i] == x) ? i : (~i);
	}

	public int Find(int x)
	{
		int num = BinarySearch(x);
		if (num >= 0 && num < count)
		{
			return num;
		}
		return -1;
	}

	public bool Contains(int x)
	{
		int num = BinarySearch(x);
		return num >= 0 && num < count;
	}

	public int Ajout(int x)
	{
		int num = BinarySearch(x);
		if (num < 0)
		{
			num = ~num;
			Insert(num, x);
			return num;
		}
		return -1;
	}

	public int Ajout(IntClasse liste)
	{
		int num = count + liste.count;
		if (num > a.Length)
		{
			int num2 = ((a.Length == 0) ? 1 : (a.Length << 1));
			while (num > num2)
			{
				num2 <<= 1;
			}
			Array.Resize(ref a, num2);
		}
		Array.Copy(a, 0, a, liste.count, count);
		int num3 = liste.count;
		int num4 = liste.count;
		int num5 = 0;
		foreach (int item in liste)
		{
			int num6 = Array.BinarySearch(a, num3, num - num3, item);
			if (num6 < 0)
			{
				int num7 = ~num6 - num4;
				Array.Copy(a, num4, a, num5, num7);
				num5 += num7;
				num4 += num7;
				num3 += num7;
				a[num5++] = item;
			}
			else
			{
				num3 += num6 - num4;
			}
		}
		int num8 = liste.count + num5 - num4;
		count += num8;
		Array.Copy(a, num4, a, num5, count - num5);
		return num8;
	}

	public int Retrait(int x)
	{
		int num = BinarySearch(x);
		if (num < 0)
		{
			return -1;
		}
		RemoveAt(num);
		return num;
	}

	public void Enlever(int i)
	{
		RemoveRange(0, i);
	}

	public int EnleverPlusProche(int x)
	{
		int num = BinarySearch(x);
		if (num < 0)
		{
			num = ~num;
		}
		if (num >= count)
		{
			return 0;
		}
		RemoveAt(num);
		return a[num];
	}

	public string AuRapport()
	{
		string text = "";
		if (count > 0)
		{
			text += a[0];
		}
		for (int i = 1; i < count; i++)
		{
			text = text + ", " + a[i];
		}
		return text;
	}

	public IEnumerator<int> GetEnumerator()
	{
		for (int i = 0; i < count; i++)
		{
			yield return a[i];
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		Array.Resize(ref a, count);
		return a.GetEnumerator();
	}
}
