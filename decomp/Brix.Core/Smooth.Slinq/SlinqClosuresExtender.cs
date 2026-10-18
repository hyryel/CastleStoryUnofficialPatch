using Smooth.Algebraics;
using Smooth.Delegates;
using Smooth.Slinq.Context;

namespace Smooth.Slinq;

public static class SlinqClosuresExtender
{
	private static class SlinqWrapper<T, P1, P2>
	{
		public static readonly DelegateFunc<T, Tuple<DelegateFunc<T, P1, P2, bool>, P1, P2>, bool> Predicate = (T s, Tuple<DelegateFunc<T, P1, P2, bool>, P1, P2> t) => t._1(s, t._2, t._3);
	}

	private static class SlinqWrapper<T, P1, P2, P3>
	{
		public static readonly DelegateFunc<T, Tuple<DelegateFunc<T, P1, P2, P3, bool>, P1, P2, P3>, bool> Predicate = (T s, Tuple<DelegateFunc<T, P1, P2, P3, bool>, P1, P2, P3> t) => t._1(s, t._2, t._3, t._4);
	}

	private static class SlinqWrapper<T, P1, P2, P3, P4>
	{
		public static readonly DelegateFunc<T, Tuple<DelegateFunc<T, P1, P2, P3, P4, bool>, P1, P2, P3, P4>, bool> Predicate = (T s, Tuple<DelegateFunc<T, P1, P2, P3, P4, bool>, P1, P2, P3, P4> t) => t._1(s, t._2, t._3, t._4, t._5);
	}

	public static Slinq<bool, SelectContext<bool, T, C, Tuple<DelegateFunc<T, P1, P2, bool>, P1, P2>>> Select<T, C, P1, P2>(this Slinq<T, C> slinq, DelegateFunc<T, P1, P2, bool> predicate, P1 p1, P2 p2)
	{
		return slinq.Select(SlinqWrapper<T, P1, P2>.Predicate, Tuple.Create(predicate, p1, p2));
	}

	public static Slinq<bool, SelectContext<bool, T, C, Tuple<DelegateFunc<T, P1, P2, P3, bool>, P1, P2, P3>>> Select<T, C, P1, P2, P3>(this Slinq<T, C> slinq, DelegateFunc<T, P1, P2, P3, bool> predicate, P1 p1, P2 p2, P3 p3)
	{
		return slinq.Select(SlinqWrapper<T, P1, P2, P3>.Predicate, Tuple.Create(predicate, p1, p2, p3));
	}

	public static Slinq<bool, SelectContext<bool, T, C, Tuple<DelegateFunc<T, P1, P2, P3, P4, bool>, P1, P2, P3, P4>>> Select<T, C, P1, P2, P3, P4>(this Slinq<T, C> slinq, DelegateFunc<T, P1, P2, P3, P4, bool> predicate, P1 p1, P2 p2, P3 p3, P4 p4)
	{
		return slinq.Select(SlinqWrapper<T, P1, P2, P3, P4>.Predicate, Tuple.Create(predicate, p1, p2, p3, p4));
	}

	public static Slinq<T, PredicateContext<T, C, Tuple<DelegateFunc<T, P1, P2, bool>, P1, P2>>> Where<T, C, P1, P2>(this Slinq<T, C> slinq, DelegateFunc<T, P1, P2, bool> predicate, P1 p1, P2 p2)
	{
		return slinq.Where(SlinqWrapper<T, P1, P2>.Predicate, Tuple.Create(predicate, p1, p2));
	}

	public static Slinq<T, PredicateContext<T, C, Tuple<DelegateFunc<T, P1, P2, P3, bool>, P1, P2, P3>>> Where<T, C, P1, P2, P3>(this Slinq<T, C> slinq, DelegateFunc<T, P1, P2, P3, bool> predicate, P1 p1, P2 p2, P3 p3)
	{
		return slinq.Where(SlinqWrapper<T, P1, P2, P3>.Predicate, Tuple.Create(predicate, p1, p2, p3));
	}

	public static Slinq<T, PredicateContext<T, C, Tuple<DelegateFunc<T, P1, P2, P3, P4, bool>, P1, P2, P3, P4>>> Where<T, C, P1, P2, P3, P4>(this Slinq<T, C> slinq, DelegateFunc<T, P1, P2, P3, P4, bool> predicate, P1 p1, P2 p2, P3 p3, P4 p4)
	{
		return slinq.Where(SlinqWrapper<T, P1, P2, P3, P4>.Predicate, Tuple.Create(predicate, p1, p2, p3, p4));
	}

	public static bool Any<T, C, P1, P2>(this Slinq<T, C> slinq, DelegateFunc<T, P1, P2, bool> predicate, P1 p1, P2 p2)
	{
		return slinq.Any(SlinqWrapper<T, P1, P2>.Predicate, Tuple.Create(predicate, p1, p2));
	}

	public static bool Any<T, C, P1, P2, P3>(this Slinq<T, C> slinq, DelegateFunc<T, P1, P2, P3, bool> predicate, P1 p1, P2 p2, P3 p3)
	{
		return slinq.Any(SlinqWrapper<T, P1, P2, P3>.Predicate, Tuple.Create(predicate, p1, p2, p3));
	}

	public static bool Any<T, C, P1, P2, P3, P4>(this Slinq<T, C> slinq, DelegateFunc<T, P1, P2, P3, P4, bool> predicate, P1 p1, P2 p2, P3 p3, P4 p4)
	{
		return slinq.Any(SlinqWrapper<T, P1, P2, P3, P4>.Predicate, Tuple.Create(predicate, p1, p2, p3, p4));
	}

	public static bool All<T, C, P1, P2>(this Slinq<T, C> slinq, DelegateFunc<T, P1, P2, bool> predicate, P1 p1, P2 p2)
	{
		return slinq.All(SlinqWrapper<T, P1, P2>.Predicate, Tuple.Create(predicate, p1, p2));
	}

	public static bool All<T, C, P1, P2, P3>(this Slinq<T, C> slinq, DelegateFunc<T, P1, P2, P3, bool> predicate, P1 p1, P2 p2, P3 p3)
	{
		return slinq.All(SlinqWrapper<T, P1, P2, P3>.Predicate, Tuple.Create(predicate, p1, p2, p3));
	}

	public static bool All<T, C, P1, P2, P3, P4>(this Slinq<T, C> slinq, DelegateFunc<T, P1, P2, P3, P4, bool> predicate, P1 p1, P2 p2, P3 p3, P4 p4)
	{
		return slinq.All(SlinqWrapper<T, P1, P2, P3, P4>.Predicate, Tuple.Create(predicate, p1, p2, p3, p4));
	}

	public static T FirstOrDefault<T, C, P1>(this Slinq<T, C> slinq, DelegateFunc<T, P1, bool> predicate, P1 p1)
	{
		return slinq.Where(predicate, p1).FirstOrDefault();
	}

	public static T FirstOrDefault<T, C, P1, P2>(this Slinq<T, C> slinq, DelegateFunc<T, P1, P2, bool> predicate, P1 p1, P2 p2)
	{
		return slinq.Where(SlinqWrapper<T, P1, P2>.Predicate, Tuple.Create(predicate, p1, p2)).FirstOrDefault();
	}

	public static T FirstOrDefault<T, C, P1, P2, P3>(this Slinq<T, C> slinq, DelegateFunc<T, P1, P2, P3, bool> predicate, P1 p1, P2 p2, P3 p3)
	{
		return slinq.Where(SlinqWrapper<T, P1, P2, P3>.Predicate, Tuple.Create(predicate, p1, p2, p3)).FirstOrDefault();
	}

	public static T FirstOrDefault<T, C, P1, P2, P3, P4>(this Slinq<T, C> slinq, DelegateFunc<T, P1, P2, P3, P4, bool> predicate, P1 p1, P2 p2, P3 p3, P4 p4)
	{
		return slinq.Where(SlinqWrapper<T, P1, P2, P3, P4>.Predicate, Tuple.Create(predicate, p1, p2, p3, p4)).FirstOrDefault();
	}
}
