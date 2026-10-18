using System.Collections;
using System.Collections.Generic;

public class ListeChainee<TValue> : IEnumerable<TValue>, IEnumerable
{
	private TValue tete;

	private ListeChainee<TValue> reste;

	public bool Empty => reste == null;

	public TValue Head => tete;

	public ListeChainee<TValue> Tail => reste;

	public ListeChainee()
	{
		reste = null;
	}

	public ListeChainee<TValue> Augmented(TValue val)
	{
		ListeChainee<TValue> listeChainee = new ListeChainee<TValue>();
		listeChainee.tete = val;
		listeChainee.reste = this;
		return listeChainee;
	}

	public void Push(TValue val)
	{
		ListeChainee<TValue> listeChainee = Augmented(val);
		ListeChainee<TValue> listeChainee2 = new ListeChainee<TValue>();
		listeChainee2.tete = tete;
		listeChainee2.reste = reste;
		listeChainee.reste = listeChainee2;
		tete = listeChainee.tete;
		reste = listeChainee.reste;
	}

	public ListeChainee<TValue> Reduced(out TValue val)
	{
		val = tete;
		return reste;
	}

	public TValue Pop()
	{
		TValue val;
		ListeChainee<TValue> listeChainee = Reduced(out val);
		tete = listeChainee.tete;
		reste = listeChainee.reste;
		return val;
	}

	public void Clear()
	{
		tete = default(TValue);
		reste = null;
	}

	public TValue[] ToBuiltin()
	{
		Classeur<TValue> classeur = new Classeur<TValue>();
		ListeChainee<TValue> listeChainee = this;
		while (listeChainee.reste != null)
		{
			classeur.Add(listeChainee.tete);
			listeChainee = listeChainee.reste;
		}
		TValue[] array = new TValue[classeur.Count];
		int num = classeur.Count;
		int num2 = 0;
		while (num != 0)
		{
			array[num2] = classeur[--num];
			num2++;
		}
		return array;
	}

	public IEnumerator<TValue> GetEnumerator()
	{
		TValue[] array = ToBuiltin();
		return ((IEnumerable<TValue>)array).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ToBuiltin().GetEnumerator();
	}
}
