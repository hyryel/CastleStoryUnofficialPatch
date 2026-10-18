using System.Collections;
using System.Collections.Generic;

public class Tas<T> : IEnumerable
{
	private class TasItem<TItem> : Item<TItem>
	{
		public LinkedList<TItem> list;

		public LinkedListNode<TItem> node;

		public override TItem Valeur => node.Value;
	}

	private class TasEnumerator : IEnumerator
	{
		private TasItem<T> item;

		public object Current => item;

		public TasEnumerator(LinkedList<T> ll)
		{
			item = new TasItem<T>();
			item.list = ll;
			item.node = null;
		}

		public bool MoveNext()
		{
			if (item.node == null)
			{
				if (item.list.Count == 0)
				{
					return false;
				}
				item.node = item.list.First;
				return true;
			}
			if (item.node.Next == null)
			{
				return false;
			}
			item.node = item.node.Next;
			return true;
		}

		public void Reset()
		{
			item.node = null;
		}
	}

	private LinkedList<T> ll;

	public Tas()
	{
		ll = new LinkedList<T>();
	}

	public IEnumerator GetEnumerator()
	{
		return new TasEnumerator(ll);
	}

	public void Add(T val)
	{
		ll.AddLast(val);
	}

	public void Remove(Item<T> item)
	{
		TasItem<T> tasItem = (TasItem<T>)item;
		ll.Remove(tasItem.node);
	}
}
