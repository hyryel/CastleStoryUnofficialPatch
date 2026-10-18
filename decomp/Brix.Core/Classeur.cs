#define DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

[Serializable]
public class Classeur<T> : IClasseur<T>, IEnumerable<T>, IEnumerable
{
	protected T[] a;

	protected int count;

	public T this[int i]
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

	public T Last => a[count - 1];

	public bool Empty => count == 0;

	public bool Full => a.Length == count;

	public int AvailableSpace => a.Length - count;

	protected Classeur(T[] a, int count)
	{
		this.a = a;
		this.count = count;
	}

	public Classeur()
	{
		Clear();
	}

	public Classeur(IEnumerable<T> list)
	{
		if (list is T[])
		{
			a = list as T[];
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

	public static implicit operator T[](Classeur<T> a)
	{
		return a?.ToBuiltin();
	}

	public static implicit operator Classeur<T>(T[] a)
	{
		return new Classeur<T>(a);
	}

	public static Classeur<T> FromArraySlice(T[] array, int n)
	{
		return new Classeur<T>(array, n);
	}

	public void Alloc(int size)
	{
		Array.Resize(ref a, size);
		count = size;
	}

	public int Add(T x)
	{
		if (count >= a.Length)
		{
			Array.Resize(ref a, (a.Length == 0) ? 1 : (a.Length << 1));
		}
		a[count] = x;
		return count++;
	}

	public int Concat(Classeur<T> liste)
	{
		return Concat(liste, liste.count);
	}

	public int Concat(Classeur<T> liste, int listeCount)
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

	public int Push(T x)
	{
		Add(x);
		return count;
	}

	public T Pop()
	{
		T result = a[--count];
		a[count] = default(T);
		return result;
	}

	public void Swap(int i, int j)
	{
		T val = a[i];
		a[i] = a[j];
		a[j] = val;
	}

	public void Insert(int i, T x)
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

	public bool Remove(T x)
	{
		for (int i = 0; i < count; i++)
		{
			ref readonly T reference = ref a[i];
			object obj = x;
			if (reference.Equals(obj))
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
			a[count] = default(T);
		}
	}

	public void RemoveRange(int i, int n)
	{
		if (count > n)
		{
			count -= n;
			Array.Copy(a, i + n, a, i, count - i);
			Array.Clear(a, count, n);
		}
		else
		{
			Clear();
		}
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
			a = new T[8];
		}
	}

	public Classeur<T> Clone()
	{
		return new Classeur<T>((T[])a.Clone(), count);
	}

	public void ReadFrom(Classeur<T> other)
	{
		count = other.count;
		a = other.a;
	}

	public void Sort()
	{
		Array.Sort(a, 0, count);
	}

	public T[] ToBuiltin()
	{
		Array.Resize(ref a, count);
		return a;
	}

	public T[] ToBuiltin(Type someT)
	{
		Debug.Assert(someT is T, "Classeur<" + typeof(T).Name + ">.ToBuiltin(" + someT.Name + ") ?!");
		return ToBuiltin();
	}

	public T[] GetInternalArray()
	{
		return a;
	}

	public void SetInternalArray(T[] a)
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

	public int BinarySearch(T x)
	{
		return Array.BinarySearch(a, 0, count, x);
	}

	public int Find(T x)
	{
		int num = BinarySearch(x);
		if (num >= 0 && num < count)
		{
			return num;
		}
		return -1;
	}

	public bool Contains(T x)
	{
		int num = BinarySearch(x);
		return num >= 0 && num < count;
	}

	public int Ajout(T x)
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

	public int Retrait(T x)
	{
		int num = BinarySearch(x);
		if (num < 0)
		{
			return -1;
		}
		RemoveAt(num);
		return 0;
	}

	public void Enlever(int i)
	{
		RemoveRange(0, i);
	}

	public T EnleverPlusProche(T x)
	{
		int num = BinarySearch(x);
		if (num < 0)
		{
			num = ~num;
		}
		if (num >= count)
		{
			return default(T);
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

	public IEnumerator<T> GetEnumerator()
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
