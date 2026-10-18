using System;
using Brix.External.Signals;

namespace Brix.Utils.Closures;

public static class Closure
{
	public struct FBase<TReturn> : IFunc<TReturn>, IFuncBase<Func<TReturn>>
	{
		public Func<TReturn> F;

		public TReturn Invoke()
		{
			return F();
		}

		public FPadded<FBase<TReturn>, T1, TReturn> Pad<T1>()
		{
			return new FPadded<FBase<TReturn>, T1, TReturn>
			{
				Up = this
			};
		}

		public FPooled<FBase<TReturn>, TReturn> Pooled()
		{
			return ((FPooled<FBase<TReturn>, TReturn>)null).Request<FPooled<FBase<TReturn>, TReturn>, FBase<TReturn>>(this);
		}

		public ClosurePoolHandle<FPooled<FBase<TReturn>, TReturn>, FBase<TReturn>, IFunc<TReturn>> Pooled2()
		{
			return ((FPooled<FBase<TReturn>, TReturn>)null).Request2<FPooled<FBase<TReturn>, TReturn>, FBase<TReturn>, IFunc<TReturn>>(null, this);
		}
	}

	public struct FBase<T1, TReturn> : IFunc<T1, TReturn>, IFuncBase<Func<T1, TReturn>>
	{
		public Func<T1, TReturn> F;

		public TReturn Invoke(T1 t1)
		{
			return F(t1);
		}

		public FClosed<FBase<T1, TReturn>, T1, TReturn> Close(T1 t)
		{
			return new FClosed<FBase<T1, TReturn>, T1, TReturn>
			{
				V = t,
				Up = this
			};
		}

		public FPadded<FBase<T1, TReturn>, T1, T2, TReturn> Pad<T2>()
		{
			return new FPadded<FBase<T1, TReturn>, T1, T2, TReturn>
			{
				Up = this
			};
		}

		public FPooled<FBase<T1, TReturn>, T1, TReturn> Pooled()
		{
			return ((FPooled<FBase<T1, TReturn>, T1, TReturn>)null).Request<FPooled<FBase<T1, TReturn>, T1, TReturn>, FBase<T1, TReturn>>(this);
		}

		public ClosurePoolHandle<FPooled<FBase<T1, TReturn>, T1, TReturn>, FBase<T1, TReturn>, IFunc<T1, TReturn>> Pooled2()
		{
			return ((FPooled<FBase<T1, TReturn>, T1, TReturn>)null).Request2<FPooled<FBase<T1, TReturn>, T1, TReturn>, FBase<T1, TReturn>, IFunc<T1, TReturn>>(null, this);
		}
	}

	public struct FBase<T1, T2, TReturn> : IFunc<T1, T2, TReturn>, IFuncBase<Func<T1, T2, TReturn>>
	{
		public Func<T1, T2, TReturn> F;

		public TReturn Invoke(T1 t1, T2 t2)
		{
			return F(t1, t2);
		}

		public FClosed<FBase<T1, T2, TReturn>, T1, T2, TReturn> Close(T2 t)
		{
			return new FClosed<FBase<T1, T2, TReturn>, T1, T2, TReturn>
			{
				V = t,
				Up = this
			};
		}

		public FPadded<FBase<T1, T2, TReturn>, T1, T2, T3, TReturn> Pad<T3>()
		{
			return new FPadded<FBase<T1, T2, TReturn>, T1, T2, T3, TReturn>
			{
				Up = this
			};
		}

		public FPooled<FBase<T1, T2, TReturn>, T1, T2, TReturn> Pooled()
		{
			return ((FPooled<FBase<T1, T2, TReturn>, T1, T2, TReturn>)null).Request<FPooled<FBase<T1, T2, TReturn>, T1, T2, TReturn>, FBase<T1, T2, TReturn>>(this);
		}

		public ClosurePoolHandle<FPooled<FBase<T1, T2, TReturn>, T1, T2, TReturn>, FBase<T1, T2, TReturn>, IFunc<T1, T2, TReturn>> Pooled2()
		{
			return ((FPooled<FBase<T1, T2, TReturn>, T1, T2, TReturn>)null).Request2<FPooled<FBase<T1, T2, TReturn>, T1, T2, TReturn>, FBase<T1, T2, TReturn>, IFunc<T1, T2, TReturn>>(null, this);
		}
	}

	public struct FBase<T1, T2, T3, TReturn> : IFunc<T1, T2, T3, TReturn>, IFuncBase<Func<T1, T2, T3, TReturn>>
	{
		public Func<T1, T2, T3, TReturn> F;

		public TReturn Invoke(T1 t1, T2 t2, T3 t3)
		{
			return F(t1, t2, t3);
		}

		public FClosed<FBase<T1, T2, T3, TReturn>, T1, T2, T3, TReturn> Close(T3 t)
		{
			return new FClosed<FBase<T1, T2, T3, TReturn>, T1, T2, T3, TReturn>
			{
				V = t,
				Up = this
			};
		}

		public FPadded<FBase<T1, T2, T3, TReturn>, T1, T2, T3, T4, TReturn> Pad<T4>()
		{
			return new FPadded<FBase<T1, T2, T3, TReturn>, T1, T2, T3, T4, TReturn>
			{
				Up = this
			};
		}

		public FPooled<FBase<T1, T2, T3, TReturn>, T1, T2, T3, TReturn> Pooled()
		{
			return ((FPooled<FBase<T1, T2, T3, TReturn>, T1, T2, T3, TReturn>)null).Request<FPooled<FBase<T1, T2, T3, TReturn>, T1, T2, T3, TReturn>, FBase<T1, T2, T3, TReturn>>(this);
		}

		public ClosurePoolHandle<FPooled<FBase<T1, T2, T3, TReturn>, T1, T2, T3, TReturn>, FBase<T1, T2, T3, TReturn>, IFunc<T1, T2, T3, TReturn>> Pooled2()
		{
			return ((FPooled<FBase<T1, T2, T3, TReturn>, T1, T2, T3, TReturn>)null).Request2<FPooled<FBase<T1, T2, T3, TReturn>, T1, T2, T3, TReturn>, FBase<T1, T2, T3, TReturn>, IFunc<T1, T2, T3, TReturn>>(null, this);
		}
	}

	public struct FBase<T1, T2, T3, T4, TReturn> : IFunc<T1, T2, T3, T4, TReturn>, IFuncBase<Func<T1, T2, T3, T4, TReturn>>
	{
		public Func<T1, T2, T3, T4, TReturn> F;

		public TReturn Invoke(T1 t1, T2 t2, T3 t3, T4 t4)
		{
			return F(t1, t2, t3, t4);
		}

		public FClosed<FBase<T1, T2, T3, T4, TReturn>, T1, T2, T3, T4, TReturn> Close(T4 t)
		{
			return new FClosed<FBase<T1, T2, T3, T4, TReturn>, T1, T2, T3, T4, TReturn>
			{
				V = t,
				Up = this
			};
		}

		public FPooled<FBase<T1, T2, T3, T4, TReturn>, T1, T2, T3, T4, TReturn> Pooled()
		{
			return ((FPooled<FBase<T1, T2, T3, T4, TReturn>, T1, T2, T3, T4, TReturn>)null).Request<FPooled<FBase<T1, T2, T3, T4, TReturn>, T1, T2, T3, T4, TReturn>, FBase<T1, T2, T3, T4, TReturn>>(this);
		}

		public ClosurePoolHandle<FPooled<FBase<T1, T2, T3, T4, TReturn>, T1, T2, T3, T4, TReturn>, FBase<T1, T2, T3, T4, TReturn>, IFunc<T1, T2, T3, T4, TReturn>> Pooled2()
		{
			return ((FPooled<FBase<T1, T2, T3, T4, TReturn>, T1, T2, T3, T4, TReturn>)null).Request2<FPooled<FBase<T1, T2, T3, T4, TReturn>, T1, T2, T3, T4, TReturn>, FBase<T1, T2, T3, T4, TReturn>, IFunc<T1, T2, T3, T4, TReturn>>(null, this);
		}
	}

	public struct FBase<T1, T2, T3, T4, T5, TReturn> : IFunc<T1, T2, T3, T4, T5, TReturn>, IFuncBase<Func<T1, T2, T3, T4, T5, TReturn>>
	{
		public Func<T1, T2, T3, T4, T5, TReturn> F;

		public TReturn Invoke(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
		{
			return F(t1, t2, t3, t4, t5);
		}

		public FClosed<FBase<T1, T2, T3, T4, T5, TReturn>, T1, T2, T3, T4, T5, TReturn> Close(T5 t)
		{
			return new FClosed<FBase<T1, T2, T3, T4, T5, TReturn>, T1, T2, T3, T4, T5, TReturn>
			{
				V = t,
				Up = this
			};
		}

		public FPooled<FBase<T1, T2, T3, T4, T5, TReturn>, T1, T2, T3, T4, T5, TReturn> Pooled()
		{
			return ((FPooled<FBase<T1, T2, T3, T4, T5, TReturn>, T1, T2, T3, T4, T5, TReturn>)null).Request<FPooled<FBase<T1, T2, T3, T4, T5, TReturn>, T1, T2, T3, T4, T5, TReturn>, FBase<T1, T2, T3, T4, T5, TReturn>>(this);
		}

		public ClosurePoolHandle<FPooled<FBase<T1, T2, T3, T4, T5, TReturn>, T1, T2, T3, T4, T5, TReturn>, FBase<T1, T2, T3, T4, T5, TReturn>, IFunc<T1, T2, T3, T4, T5, TReturn>> Pooled2()
		{
			return ((FPooled<FBase<T1, T2, T3, T4, T5, TReturn>, T1, T2, T3, T4, T5, TReturn>)null).Request2<FPooled<FBase<T1, T2, T3, T4, T5, TReturn>, T1, T2, T3, T4, T5, TReturn>, FBase<T1, T2, T3, T4, T5, TReturn>, IFunc<T1, T2, T3, T4, T5, TReturn>>(null, this);
		}
	}

	public struct FClosed<TUp, T1, TReturn> : IFunc<TReturn>, IFuncBase<Func<TReturn>> where TUp : struct, IFunc<T1, TReturn>
	{
		public TUp Up;

		public T1 V;

		public TReturn Invoke()
		{
			return Up.Invoke(V);
		}

		public FPadded<TUp, T1, T2, TReturn> Pad<T2>()
		{
			return new FPadded<TUp, T1, T2, TReturn>
			{
				Up = Up
			};
		}

		public FPooled<FClosed<TUp, T1, TReturn>, TReturn> Pooled()
		{
			return ((FPooled<FClosed<TUp, T1, TReturn>, TReturn>)null).Request<FPooled<FClosed<TUp, T1, TReturn>, TReturn>, FClosed<TUp, T1, TReturn>>(this);
		}

		public ClosurePoolHandle<FPooled<FClosed<TUp, T1, TReturn>, TReturn>, FClosed<TUp, T1, TReturn>, IFunc<TReturn>> Pooled2()
		{
			return ((FPooled<FClosed<TUp, T1, TReturn>, TReturn>)null).Request2<FPooled<FClosed<TUp, T1, TReturn>, TReturn>, FClosed<TUp, T1, TReturn>, IFunc<TReturn>>(null, this);
		}
	}

	public struct FClosed<TUp, T1, T2, TReturn> : IFunc<T1, TReturn>, IFuncBase<Func<T1, TReturn>> where TUp : struct, IFunc<T1, T2, TReturn>
	{
		public TUp Up;

		public T2 V;

		public TReturn Invoke(T1 t1)
		{
			return Up.Invoke(t1, V);
		}

		public FClosed<FClosed<TUp, T1, T2, TReturn>, T1, TReturn> Close(T1 t)
		{
			return new FClosed<FClosed<TUp, T1, T2, TReturn>, T1, TReturn>
			{
				V = t,
				Up = this
			};
		}

		public FPadded<TUp, T1, T2, T3, TReturn> Pad<T3>()
		{
			return new FPadded<TUp, T1, T2, T3, TReturn>
			{
				Up = Up
			};
		}

		public FPooled<FClosed<TUp, T1, T2, TReturn>, T1, TReturn> Pooled()
		{
			return ((FPooled<FClosed<TUp, T1, T2, TReturn>, T1, TReturn>)null).Request<FPooled<FClosed<TUp, T1, T2, TReturn>, T1, TReturn>, FClosed<TUp, T1, T2, TReturn>>(this);
		}

		public ClosurePoolHandle<FPooled<FClosed<TUp, T1, T2, TReturn>, T1, TReturn>, FClosed<TUp, T1, T2, TReturn>, IFunc<T1, TReturn>> Pooled2()
		{
			return ((FPooled<FClosed<TUp, T1, T2, TReturn>, T1, TReturn>)null).Request2<FPooled<FClosed<TUp, T1, T2, TReturn>, T1, TReturn>, FClosed<TUp, T1, T2, TReturn>, IFunc<T1, TReturn>>(null, this);
		}
	}

	public struct FClosed<TUp, T1, T2, T3, TReturn> : IFunc<T1, T2, TReturn>, IFuncBase<Func<T1, T2, TReturn>> where TUp : struct, IFunc<T1, T2, T3, TReturn>
	{
		public TUp Up;

		public T3 V;

		public TReturn Invoke(T1 t1, T2 t2)
		{
			return Up.Invoke(t1, t2, V);
		}

		public FClosed<FClosed<TUp, T1, T2, T3, TReturn>, T1, T2, TReturn> Close(T2 t)
		{
			return new FClosed<FClosed<TUp, T1, T2, T3, TReturn>, T1, T2, TReturn>
			{
				V = t,
				Up = this
			};
		}

		public FPadded<TUp, T1, T2, T3, T4, TReturn> Pad<T4>()
		{
			return new FPadded<TUp, T1, T2, T3, T4, TReturn>
			{
				Up = Up
			};
		}

		public FPooled<FClosed<TUp, T1, T2, T3, TReturn>, T1, T2, TReturn> Pooled()
		{
			return ((FPooled<FClosed<TUp, T1, T2, T3, TReturn>, T1, T2, TReturn>)null).Request<FPooled<FClosed<TUp, T1, T2, T3, TReturn>, T1, T2, TReturn>, FClosed<TUp, T1, T2, T3, TReturn>>(this);
		}

		public ClosurePoolHandle<FPooled<FClosed<TUp, T1, T2, T3, TReturn>, T1, T2, TReturn>, FClosed<TUp, T1, T2, T3, TReturn>, IFunc<T1, T2, TReturn>> Pooled2()
		{
			return ((FPooled<FClosed<TUp, T1, T2, T3, TReturn>, T1, T2, TReturn>)null).Request2<FPooled<FClosed<TUp, T1, T2, T3, TReturn>, T1, T2, TReturn>, FClosed<TUp, T1, T2, T3, TReturn>, IFunc<T1, T2, TReturn>>(null, this);
		}
	}

	public struct FClosed<TUp, T1, T2, T3, T4, TReturn> : IFunc<T1, T2, T3, TReturn>, IFuncBase<Func<T1, T2, T3, TReturn>> where TUp : struct, IFunc<T1, T2, T3, T4, TReturn>
	{
		public TUp Up;

		public T4 V;

		public TReturn Invoke(T1 t1, T2 t2, T3 t3)
		{
			return Up.Invoke(t1, t2, t3, V);
		}

		public FClosed<FClosed<TUp, T1, T2, T3, T4, TReturn>, T1, T2, T3, TReturn> Close(T3 t)
		{
			return new FClosed<FClosed<TUp, T1, T2, T3, T4, TReturn>, T1, T2, T3, TReturn>
			{
				V = t,
				Up = this
			};
		}

		public FPooled<FClosed<TUp, T1, T2, T3, T4, TReturn>, T1, T2, T3, TReturn> Pooled()
		{
			return ((FPooled<FClosed<TUp, T1, T2, T3, T4, TReturn>, T1, T2, T3, TReturn>)null).Request<FPooled<FClosed<TUp, T1, T2, T3, T4, TReturn>, T1, T2, T3, TReturn>, FClosed<TUp, T1, T2, T3, T4, TReturn>>(this);
		}

		public ClosurePoolHandle<FPooled<FClosed<TUp, T1, T2, T3, T4, TReturn>, T1, T2, T3, TReturn>, FClosed<TUp, T1, T2, T3, T4, TReturn>, IFunc<T1, T2, T3, TReturn>> Pooled2()
		{
			return ((FPooled<FClosed<TUp, T1, T2, T3, T4, TReturn>, T1, T2, T3, TReturn>)null).Request2<FPooled<FClosed<TUp, T1, T2, T3, T4, TReturn>, T1, T2, T3, TReturn>, FClosed<TUp, T1, T2, T3, T4, TReturn>, IFunc<T1, T2, T3, TReturn>>(null, this);
		}
	}

	public struct FClosed<TUp, T1, T2, T3, T4, T5, TReturn> : IFunc<T1, T2, T3, T4, TReturn>, IFuncBase<Func<T1, T2, T3, T4, TReturn>> where TUp : struct, IFunc<T1, T2, T3, T4, T5, TReturn>
	{
		public TUp Up;

		public T5 V;

		public TReturn Invoke(T1 t1, T2 t2, T3 t3, T4 t4)
		{
			return Up.Invoke(t1, t2, t3, t4, V);
		}

		public FClosed<FClosed<TUp, T1, T2, T3, T4, T5, TReturn>, T1, T2, T3, T4, TReturn> Close(T4 t)
		{
			return new FClosed<FClosed<TUp, T1, T2, T3, T4, T5, TReturn>, T1, T2, T3, T4, TReturn>
			{
				V = t,
				Up = this
			};
		}

		public FPooled<FClosed<TUp, T1, T2, T3, T4, T5, TReturn>, T1, T2, T3, T4, TReturn> Pooled()
		{
			return ((FPooled<FClosed<TUp, T1, T2, T3, T4, T5, TReturn>, T1, T2, T3, T4, TReturn>)null).Request<FPooled<FClosed<TUp, T1, T2, T3, T4, T5, TReturn>, T1, T2, T3, T4, TReturn>, FClosed<TUp, T1, T2, T3, T4, T5, TReturn>>(this);
		}

		public ClosurePoolHandle<FPooled<FClosed<TUp, T1, T2, T3, T4, T5, TReturn>, T1, T2, T3, T4, TReturn>, FClosed<TUp, T1, T2, T3, T4, T5, TReturn>, IFunc<T1, T2, T3, T4, TReturn>> Pooled2()
		{
			return ((FPooled<FClosed<TUp, T1, T2, T3, T4, T5, TReturn>, T1, T2, T3, T4, TReturn>)null).Request2<FPooled<FClosed<TUp, T1, T2, T3, T4, T5, TReturn>, T1, T2, T3, T4, TReturn>, FClosed<TUp, T1, T2, T3, T4, T5, TReturn>, IFunc<T1, T2, T3, T4, TReturn>>(null, this);
		}
	}

	public struct FPadded<TUp, T1, TReturn> : IFunc<T1, TReturn>, IFuncBase<Func<T1, TReturn>> where TUp : struct, IFunc<TReturn>
	{
		public TUp Up;

		public TReturn Invoke(T1 t1)
		{
			return Up.Invoke();
		}

		public TUp Close(T1 t1)
		{
			return Up;
		}

		public FPadded<FPadded<TUp, T1, TReturn>, T1, T2, TReturn> Pad<T2>()
		{
			return new FPadded<FPadded<TUp, T1, TReturn>, T1, T2, TReturn>
			{
				Up = this
			};
		}

		public FPooled<FPadded<TUp, T1, TReturn>, T1, TReturn> Pooled()
		{
			return ((FPooled<FPadded<TUp, T1, TReturn>, T1, TReturn>)null).Request<FPooled<FPadded<TUp, T1, TReturn>, T1, TReturn>, FPadded<TUp, T1, TReturn>>(this);
		}

		public ClosurePoolHandle<FPooled<FPadded<TUp, T1, TReturn>, T1, TReturn>, FPadded<TUp, T1, TReturn>, IFunc<T1, TReturn>> Pooled2()
		{
			return ((FPooled<FPadded<TUp, T1, TReturn>, T1, TReturn>)null).Request2<FPooled<FPadded<TUp, T1, TReturn>, T1, TReturn>, FPadded<TUp, T1, TReturn>, IFunc<T1, TReturn>>(null, this);
		}
	}

	public struct FPadded<TUp, T1, T2, TReturn> : IFunc<T1, T2, TReturn>, IFuncBase<Func<T1, T2, TReturn>> where TUp : struct, IFunc<T1, TReturn>
	{
		public TUp Up;

		public TReturn Invoke(T1 t1, T2 t2)
		{
			return Up.Invoke(t1);
		}

		public TUp Close(T2 t2)
		{
			return Up;
		}

		public FPadded<FPadded<TUp, T1, T2, TReturn>, T1, T2, T3, TReturn> Pad<T3>()
		{
			return new FPadded<FPadded<TUp, T1, T2, TReturn>, T1, T2, T3, TReturn>
			{
				Up = this
			};
		}

		public FPooled<FPadded<TUp, T1, T2, TReturn>, T1, T2, TReturn> Pooled()
		{
			return ((FPooled<FPadded<TUp, T1, T2, TReturn>, T1, T2, TReturn>)null).Request<FPooled<FPadded<TUp, T1, T2, TReturn>, T1, T2, TReturn>, FPadded<TUp, T1, T2, TReturn>>(this);
		}

		public ClosurePoolHandle<FPooled<FPadded<TUp, T1, T2, TReturn>, T1, T2, TReturn>, FPadded<TUp, T1, T2, TReturn>, IFunc<T1, T2, TReturn>> Pooled2()
		{
			return ((FPooled<FPadded<TUp, T1, T2, TReturn>, T1, T2, TReturn>)null).Request2<FPooled<FPadded<TUp, T1, T2, TReturn>, T1, T2, TReturn>, FPadded<TUp, T1, T2, TReturn>, IFunc<T1, T2, TReturn>>(null, this);
		}
	}

	public struct FPadded<TUp, T1, T2, T3, TReturn> : IFunc<T1, T2, T3, TReturn>, IFuncBase<Func<T1, T2, T3, TReturn>> where TUp : struct, IFunc<T1, T2, TReturn>
	{
		public TUp Up;

		public TReturn Invoke(T1 t1, T2 t2, T3 t3)
		{
			return Up.Invoke(t1, t2);
		}

		public TUp Close(T3 t3)
		{
			return Up;
		}

		public FPadded<FPadded<TUp, T1, T2, T3, TReturn>, T1, T2, T3, T4, TReturn> Pad<T4>()
		{
			return new FPadded<FPadded<TUp, T1, T2, T3, TReturn>, T1, T2, T3, T4, TReturn>
			{
				Up = this
			};
		}

		public FPooled<FPadded<TUp, T1, T2, T3, TReturn>, T1, T2, T3, TReturn> Pooled()
		{
			return ((FPooled<FPadded<TUp, T1, T2, T3, TReturn>, T1, T2, T3, TReturn>)null).Request<FPooled<FPadded<TUp, T1, T2, T3, TReturn>, T1, T2, T3, TReturn>, FPadded<TUp, T1, T2, T3, TReturn>>(this);
		}

		public ClosurePoolHandle<FPooled<FPadded<TUp, T1, T2, T3, TReturn>, T1, T2, T3, TReturn>, FPadded<TUp, T1, T2, T3, TReturn>, IFunc<T1, T2, T3, TReturn>> Pooled2()
		{
			return ((FPooled<FPadded<TUp, T1, T2, T3, TReturn>, T1, T2, T3, TReturn>)null).Request2<FPooled<FPadded<TUp, T1, T2, T3, TReturn>, T1, T2, T3, TReturn>, FPadded<TUp, T1, T2, T3, TReturn>, IFunc<T1, T2, T3, TReturn>>(null, this);
		}
	}

	public struct FPadded<TUp, T1, T2, T3, T4, TReturn> : IFunc<T1, T2, T3, T4, TReturn>, IFuncBase<Func<T1, T2, T3, T4, TReturn>> where TUp : struct, IFunc<T1, T2, T3, TReturn>
	{
		public TUp Up;

		public TReturn Invoke(T1 t1, T2 t2, T3 t3, T4 t4)
		{
			return Up.Invoke(t1, t2, t3);
		}

		public TUp Close(T4 t4)
		{
			return Up;
		}

		public FPooled<FPadded<TUp, T1, T2, T3, T4, TReturn>, T1, T2, T3, T4, TReturn> Pooled()
		{
			return ((FPooled<FPadded<TUp, T1, T2, T3, T4, TReturn>, T1, T2, T3, T4, TReturn>)null).Request<FPooled<FPadded<TUp, T1, T2, T3, T4, TReturn>, T1, T2, T3, T4, TReturn>, FPadded<TUp, T1, T2, T3, T4, TReturn>>(this);
		}

		public ClosurePoolHandle<FPooled<FPadded<TUp, T1, T2, T3, T4, TReturn>, T1, T2, T3, T4, TReturn>, FPadded<TUp, T1, T2, T3, T4, TReturn>, IFunc<T1, T2, T3, T4, TReturn>> Pooled2()
		{
			return ((FPooled<FPadded<TUp, T1, T2, T3, T4, TReturn>, T1, T2, T3, T4, TReturn>)null).Request2<FPooled<FPadded<TUp, T1, T2, T3, T4, TReturn>, T1, T2, T3, T4, TReturn>, FPadded<TUp, T1, T2, T3, T4, TReturn>, IFunc<T1, T2, T3, T4, TReturn>>(null, this);
		}
	}

	public struct FPadded<TUp, T1, T2, T3, T4, T5, TReturn> : IFunc<T1, T2, T3, T4, T5, TReturn>, IFuncBase<Func<T1, T2, T3, T4, T5, TReturn>> where TUp : struct, IFunc<T1, T2, T3, T4, TReturn>
	{
		public TUp Up;

		public TReturn Invoke(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
		{
			return Up.Invoke(t1, t2, t3, t4);
		}

		public TUp Close(T5 t5)
		{
			return Up;
		}

		public FPooled<FPadded<TUp, T1, T2, T3, T4, T5, TReturn>, T1, T2, T3, T4, T5, TReturn> Pooled()
		{
			return ((FPooled<FPadded<TUp, T1, T2, T3, T4, T5, TReturn>, T1, T2, T3, T4, T5, TReturn>)null).Request<FPooled<FPadded<TUp, T1, T2, T3, T4, T5, TReturn>, T1, T2, T3, T4, T5, TReturn>, FPadded<TUp, T1, T2, T3, T4, T5, TReturn>>(this);
		}

		public ClosurePoolHandle<FPooled<FPadded<TUp, T1, T2, T3, T4, T5, TReturn>, T1, T2, T3, T4, T5, TReturn>, FPadded<TUp, T1, T2, T3, T4, T5, TReturn>, IFunc<T1, T2, T3, T4, T5, TReturn>> Pooled2()
		{
			return ((FPooled<FPadded<TUp, T1, T2, T3, T4, T5, TReturn>, T1, T2, T3, T4, T5, TReturn>)null).Request2<FPooled<FPadded<TUp, T1, T2, T3, T4, T5, TReturn>, T1, T2, T3, T4, T5, TReturn>, FPadded<TUp, T1, T2, T3, T4, T5, TReturn>, IFunc<T1, T2, T3, T4, T5, TReturn>>(null, this);
		}
	}

	public class FPooled<TClosed, TReturn> : Pooling.PooledBase<TClosed>, IFunc<TReturn>, IFuncBase<Func<TReturn>> where TClosed : struct, IFunc<TReturn>
	{
		public readonly Func<TReturn> Func;

		public FPooled()
		{
			Func = () => Closed.Invoke();
		}

		public TReturn Invoke()
		{
			return Func();
		}
	}

	public class FPooled<TClosed, T1, TReturn> : Pooling.PooledBase<TClosed>, IFunc<T1, TReturn>, IFuncBase<Func<T1, TReturn>> where TClosed : struct, IFunc<T1, TReturn>
	{
		public readonly Func<T1, TReturn> Func;

		public FPooled()
		{
			Func = (T1 a) => Closed.Invoke(a);
		}

		public TReturn Invoke(T1 t1)
		{
			return Func(t1);
		}
	}

	public class FPooled<TClosed, T1, T2, TReturn> : Pooling.PooledBase<TClosed>, IFunc<T1, T2, TReturn>, IFuncBase<Func<T1, T2, TReturn>> where TClosed : struct, IFunc<T1, T2, TReturn>
	{
		public readonly Func<T1, T2, TReturn> Func;

		public FPooled()
		{
			Func = (T1 a, T2 b) => Closed.Invoke(a, b);
		}

		public TReturn Invoke(T1 t1, T2 t2)
		{
			return Func(t1, t2);
		}
	}

	public class FPooled<TClosed, T1, T2, T3, TReturn> : Pooling.PooledBase<TClosed>, IFunc<T1, T2, T3, TReturn>, IFuncBase<Func<T1, T2, T3, TReturn>> where TClosed : struct, IFunc<T1, T2, T3, TReturn>
	{
		public readonly Func<T1, T2, T3, TReturn> Func;

		public FPooled()
		{
			Func = (T1 a, T2 b, T3 c) => Closed.Invoke(a, b, c);
		}

		public TReturn Invoke(T1 t1, T2 t2, T3 t3)
		{
			return Func(t1, t2, t3);
		}
	}

	public class FPooled<TClosed, T1, T2, T3, T4, TReturn> : Pooling.PooledBase<TClosed>, IFunc<T1, T2, T3, T4, TReturn>, IFuncBase<Func<T1, T2, T3, T4, TReturn>> where TClosed : struct, IFunc<T1, T2, T3, T4, TReturn>
	{
		public readonly Func<T1, T2, T3, T4, TReturn> Func;

		public FPooled()
		{
			Func = (T1 a, T2 b, T3 c, T4 d) => Closed.Invoke(a, b, c, d);
		}

		public TReturn Invoke(T1 t1, T2 t2, T3 t3, T4 t4)
		{
			return Func(t1, t2, t3, t4);
		}
	}

	public class FPooled<TClosed, T1, T2, T3, T4, T5, TReturn> : Pooling.PooledBase<TClosed>, IFunc<T1, T2, T3, T4, T5, TReturn>, IFuncBase<Func<T1, T2, T3, T4, T5, TReturn>> where TClosed : struct, IFunc<T1, T2, T3, T4, T5, TReturn>
	{
		public readonly Func<T1, T2, T3, T4, T5, TReturn> Func;

		public FPooled()
		{
			Func = (T1 a, T2 b, T3 c, T4 d, T5 e) => Closed.Invoke(a, b, c, d, e);
		}

		public TReturn Invoke(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
		{
			return Func(t1, t2, t3, t4, t5);
		}
	}

	public struct ABase : IAction, IActionBase<Action>
	{
		public Action F;

		public void Invoke()
		{
			F();
		}

		public APadded<ABase, T1> Pad<T1>()
		{
			return new APadded<ABase, T1>
			{
				Up = this
			};
		}

		public APooled<ABase> Pooled()
		{
			return ((APooled<ABase>)null).Request<APooled<ABase>, ABase>(this);
		}

		public ClosurePoolHandle<APooled<ABase>, ABase, IAction> Pooled2()
		{
			return ((APooled<ABase>)null).Request2<APooled<ABase>, ABase, IAction>(null, this);
		}
	}

	public struct ABase<T1> : IAction<T1>, IActionBase<Action<T1>>
	{
		public Action<T1> F;

		public void Invoke(T1 t1)
		{
			F(t1);
		}

		public AClosed<ABase<T1>, T1> Close(T1 t1)
		{
			return new AClosed<ABase<T1>, T1>
			{
				V = t1,
				Up = this
			};
		}

		public APadded<ABase<T1>, T1, T2> Pad<T2>()
		{
			return new APadded<ABase<T1>, T1, T2>
			{
				Up = this
			};
		}

		public APooled<ABase<T1>, T1> Pooled()
		{
			return ((APooled<ABase<T1>, T1>)null).Request<APooled<ABase<T1>, T1>, ABase<T1>>(this);
		}

		public ClosurePoolHandle<APooled<ABase<T1>, T1>, ABase<T1>, IAction<T1>> Pooled2()
		{
			return ((APooled<ABase<T1>, T1>)null).Request2<APooled<ABase<T1>, T1>, ABase<T1>, IAction<T1>>(null, this);
		}
	}

	public struct ABase<T1, T2> : IAction<T1, T2>, IActionBase<Action<T1, T2>>
	{
		public Action<T1, T2> F;

		public void Invoke(T1 t1, T2 t2)
		{
			F(t1, t2);
		}

		public AClosed<ABase<T1, T2>, T1, T2> Close(T2 t2)
		{
			return new AClosed<ABase<T1, T2>, T1, T2>
			{
				V = t2,
				Up = this
			};
		}

		public AClosed2<ABase<T1, T2>, T1, T2> Close(T1 t1, T2 t2)
		{
			return new AClosed2<ABase<T1, T2>, T1, T2>
			{
				V1 = t1,
				V2 = t2,
				Up = this
			};
		}

		public APadded<ABase<T1, T2>, T1, T2, T3> Pad<T3>()
		{
			return new APadded<ABase<T1, T2>, T1, T2, T3>
			{
				Up = this
			};
		}

		public APooled<ABase<T1, T2>, T1, T2> Pooled()
		{
			return ((APooled<ABase<T1, T2>, T1, T2>)null).Request<APooled<ABase<T1, T2>, T1, T2>, ABase<T1, T2>>(this);
		}

		public ClosurePoolHandle<APooled<ABase<T1, T2>, T1, T2>, ABase<T1, T2>, IAction<T1, T2>> Pooled2()
		{
			return ((APooled<ABase<T1, T2>, T1, T2>)null).Request2<APooled<ABase<T1, T2>, T1, T2>, ABase<T1, T2>, IAction<T1, T2>>(null, this);
		}
	}

	public struct ABase<T1, T2, T3> : IAction<T1, T2, T3>, IActionBase<Action<T1, T2, T3>>
	{
		public Action<T1, T2, T3> F;

		public void Invoke(T1 t1, T2 t2, T3 t3)
		{
			F(t1, t2, t3);
		}

		public AClosed<ABase<T1, T2, T3>, T1, T2, T3> Close(T3 t3)
		{
			return new AClosed<ABase<T1, T2, T3>, T1, T2, T3>
			{
				V = t3,
				Up = this
			};
		}

		public AClosed2<ABase<T1, T2, T3>, T1, T2, T3> Close(T2 t2, T3 t3)
		{
			return new AClosed2<ABase<T1, T2, T3>, T1, T2, T3>
			{
				V2 = t2,
				V3 = t3,
				Up = this
			};
		}

		public AClosed3<ABase<T1, T2, T3>, T1, T2, T3> Close(T1 t1, T2 t2, T3 t3)
		{
			return new AClosed3<ABase<T1, T2, T3>, T1, T2, T3>
			{
				V1 = t1,
				V2 = t2,
				V3 = t3,
				Up = this
			};
		}

		public APadded<ABase<T1, T2, T3>, T1, T2, T3, T4> Pad<T4>()
		{
			return new APadded<ABase<T1, T2, T3>, T1, T2, T3, T4>
			{
				Up = this
			};
		}

		public APooled<ABase<T1, T2, T3>, T1, T2, T3> Pooled()
		{
			return ((APooled<ABase<T1, T2, T3>, T1, T2, T3>)null).Request<APooled<ABase<T1, T2, T3>, T1, T2, T3>, ABase<T1, T2, T3>>(this);
		}

		public ClosurePoolHandle<APooled<ABase<T1, T2, T3>, T1, T2, T3>, ABase<T1, T2, T3>, IAction<T1, T2, T3>> Pooled2()
		{
			return ((APooled<ABase<T1, T2, T3>, T1, T2, T3>)null).Request2<APooled<ABase<T1, T2, T3>, T1, T2, T3>, ABase<T1, T2, T3>, IAction<T1, T2, T3>>(null, this);
		}
	}

	public struct ABase<T1, T2, T3, T4> : IAction<T1, T2, T3, T4>, IActionBase<Action<T1, T2, T3, T4>>
	{
		public Action<T1, T2, T3, T4> F;

		public void Invoke(T1 t1, T2 t2, T3 t3, T4 t4)
		{
			F(t1, t2, t3, t4);
		}

		public AClosed<ABase<T1, T2, T3, T4>, T1, T2, T3, T4> Close(T4 t4)
		{
			return new AClosed<ABase<T1, T2, T3, T4>, T1, T2, T3, T4>
			{
				V = t4,
				Up = this
			};
		}

		public AClosed2<ABase<T1, T2, T3, T4>, T1, T2, T3, T4> Close(T3 t3, T4 t4)
		{
			return new AClosed2<ABase<T1, T2, T3, T4>, T1, T2, T3, T4>
			{
				V3 = t3,
				V4 = t4,
				Up = this
			};
		}

		public AClosed3<ABase<T1, T2, T3, T4>, T1, T2, T3, T4> Close(T2 t2, T3 t3, T4 t4)
		{
			return new AClosed3<ABase<T1, T2, T3, T4>, T1, T2, T3, T4>
			{
				V2 = t2,
				V3 = t3,
				V4 = t4,
				Up = this
			};
		}

		public AClosed4<ABase<T1, T2, T3, T4>, T1, T2, T3, T4> Close(T1 t1, T2 t2, T3 t3, T4 t4)
		{
			return new AClosed4<ABase<T1, T2, T3, T4>, T1, T2, T3, T4>
			{
				V1 = t1,
				V2 = t2,
				V3 = t3,
				V4 = t4,
				Up = this
			};
		}

		public APooled<ABase<T1, T2, T3, T4>, T1, T2, T3, T4> Pooled()
		{
			return ((APooled<ABase<T1, T2, T3, T4>, T1, T2, T3, T4>)null).Request<APooled<ABase<T1, T2, T3, T4>, T1, T2, T3, T4>, ABase<T1, T2, T3, T4>>(this);
		}

		public ClosurePoolHandle<APooled<ABase<T1, T2, T3, T4>, T1, T2, T3, T4>, ABase<T1, T2, T3, T4>, IAction<T1, T2, T3, T4>> Pooled2()
		{
			return ((APooled<ABase<T1, T2, T3, T4>, T1, T2, T3, T4>)null).Request2<APooled<ABase<T1, T2, T3, T4>, T1, T2, T3, T4>, ABase<T1, T2, T3, T4>, IAction<T1, T2, T3, T4>>(null, this);
		}
	}

	public struct ABase<T1, T2, T3, T4, T5> : IAction<T1, T2, T3, T4, T5>, IActionBase<Action<T1, T2, T3, T4, T5>>
	{
		public Action<T1, T2, T3, T4, T5> F;

		public void Invoke(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
		{
			F(t1, t2, t3, t4, t5);
		}

		public AClosed<ABase<T1, T2, T3, T4, T5>, T1, T2, T3, T4, T5> Close(T5 t5)
		{
			return new AClosed<ABase<T1, T2, T3, T4, T5>, T1, T2, T3, T4, T5>
			{
				V = t5,
				Up = this
			};
		}

		public AClosed2<ABase<T1, T2, T3, T4, T5>, T1, T2, T3, T4, T5> Close(T4 t4, T5 t5)
		{
			return new AClosed2<ABase<T1, T2, T3, T4, T5>, T1, T2, T3, T4, T5>
			{
				V4 = t4,
				V5 = t5,
				Up = this
			};
		}

		public AClosed3<ABase<T1, T2, T3, T4, T5>, T1, T2, T3, T4, T5> Close(T3 t3, T4 t4, T5 t5)
		{
			return new AClosed3<ABase<T1, T2, T3, T4, T5>, T1, T2, T3, T4, T5>
			{
				V3 = t3,
				V4 = t4,
				V5 = t5,
				Up = this
			};
		}

		public AClosed4<ABase<T1, T2, T3, T4, T5>, T1, T2, T3, T4, T5> Close(T2 t2, T3 t3, T4 t4, T5 t5)
		{
			return new AClosed4<ABase<T1, T2, T3, T4, T5>, T1, T2, T3, T4, T5>
			{
				V2 = t2,
				V3 = t3,
				V4 = t4,
				V5 = t5,
				Up = this
			};
		}

		public AClosed5<ABase<T1, T2, T3, T4, T5>, T1, T2, T3, T4, T5> Close(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
		{
			return new AClosed5<ABase<T1, T2, T3, T4, T5>, T1, T2, T3, T4, T5>
			{
				V1 = t1,
				V2 = t2,
				V3 = t3,
				V4 = t4,
				V5 = t5,
				Up = this
			};
		}

		public APooled<ABase<T1, T2, T3, T4, T5>, T1, T2, T3, T4, T5> Pooled()
		{
			return ((APooled<ABase<T1, T2, T3, T4, T5>, T1, T2, T3, T4, T5>)null).Request<APooled<ABase<T1, T2, T3, T4, T5>, T1, T2, T3, T4, T5>, ABase<T1, T2, T3, T4, T5>>(this);
		}

		public ClosurePoolHandle<APooled<ABase<T1, T2, T3, T4, T5>, T1, T2, T3, T4, T5>, ABase<T1, T2, T3, T4, T5>, IAction<T1, T2, T3, T4, T5>> Pooled2()
		{
			return ((APooled<ABase<T1, T2, T3, T4, T5>, T1, T2, T3, T4, T5>)null).Request2<APooled<ABase<T1, T2, T3, T4, T5>, T1, T2, T3, T4, T5>, ABase<T1, T2, T3, T4, T5>, IAction<T1, T2, T3, T4, T5>>(null, this);
		}
	}

	public struct AClosed<TUp, T1> : IAction, IActionBase<Action> where TUp : struct, IAction<T1>
	{
		public TUp Up;

		public T1 V;

		public void Invoke()
		{
			Up.Invoke(V);
		}

		public APadded<TUp, T1, T2> Pad<T2>()
		{
			return new APadded<TUp, T1, T2>
			{
				Up = Up
			};
		}

		public APooled<AClosed<TUp, T1>> Pooled()
		{
			return ((APooled<AClosed<TUp, T1>>)null).Request<APooled<AClosed<TUp, T1>>, AClosed<TUp, T1>>(this);
		}

		public ClosurePoolHandle<APooled<AClosed<TUp, T1>>, AClosed<TUp, T1>, IAction> Pooled2()
		{
			return ((APooled<AClosed<TUp, T1>>)null).Request2<APooled<AClosed<TUp, T1>>, AClosed<TUp, T1>, IAction>(null, this);
		}
	}

	public struct AClosed<TUp, T1, T2> : IAction<T1>, IActionBase<Action<T1>> where TUp : struct, IAction<T1, T2>
	{
		public TUp Up;

		public T2 V;

		public void Invoke(T1 t1)
		{
			Up.Invoke(t1, V);
		}

		public AClosed<AClosed<TUp, T1, T2>, T1> Close(T1 t1)
		{
			return new AClosed<AClosed<TUp, T1, T2>, T1>
			{
				V = t1,
				Up = this
			};
		}

		public APadded<TUp, T1, T2, T3> Pad<T3>()
		{
			return new APadded<TUp, T1, T2, T3>
			{
				Up = Up
			};
		}

		public APooled<AClosed<TUp, T1, T2>, T1> Pooled()
		{
			return ((APooled<AClosed<TUp, T1, T2>, T1>)null).Request<APooled<AClosed<TUp, T1, T2>, T1>, AClosed<TUp, T1, T2>>(this);
		}

		public ClosurePoolHandle<APooled<AClosed<TUp, T1, T2>, T1>, AClosed<TUp, T1, T2>, IAction<T1>> Pooled2()
		{
			return ((APooled<AClosed<TUp, T1, T2>, T1>)null).Request2<APooled<AClosed<TUp, T1, T2>, T1>, AClosed<TUp, T1, T2>, IAction<T1>>(null, this);
		}
	}

	public struct AClosed<TUp, T1, T2, T3> : IAction<T1, T2>, IActionBase<Action<T1, T2>> where TUp : struct, IAction<T1, T2, T3>
	{
		public TUp Up;

		public T3 V;

		public void Invoke(T1 t1, T2 t2)
		{
			Up.Invoke(t1, t2, V);
		}

		public AClosed<AClosed<TUp, T1, T2, T3>, T1, T2> Close(T2 t2)
		{
			return new AClosed<AClosed<TUp, T1, T2, T3>, T1, T2>
			{
				V = t2,
				Up = this
			};
		}

		public APadded<TUp, T1, T2, T3, T4> Pad<T4>()
		{
			return new APadded<TUp, T1, T2, T3, T4>
			{
				Up = Up
			};
		}

		public APooled<AClosed<TUp, T1, T2, T3>, T1, T2> Pooled()
		{
			return ((APooled<AClosed<TUp, T1, T2, T3>, T1, T2>)null).Request<APooled<AClosed<TUp, T1, T2, T3>, T1, T2>, AClosed<TUp, T1, T2, T3>>(this);
		}

		public ClosurePoolHandle<APooled<AClosed<TUp, T1, T2, T3>, T1, T2>, AClosed<TUp, T1, T2, T3>, IAction<T1, T2>> Pooled2()
		{
			return ((APooled<AClosed<TUp, T1, T2, T3>, T1, T2>)null).Request2<APooled<AClosed<TUp, T1, T2, T3>, T1, T2>, AClosed<TUp, T1, T2, T3>, IAction<T1, T2>>(null, this);
		}
	}

	public struct AClosed<TUp, T1, T2, T3, T4> : IAction<T1, T2, T3>, IActionBase<Action<T1, T2, T3>> where TUp : IAction<T1, T2, T3, T4>
	{
		public TUp Up;

		public T4 V;

		public void Invoke(T1 t1, T2 t2, T3 t3)
		{
			ref TUp up = ref Up;
			T4 v = V;
			up.Invoke(t1, t2, t3, v);
		}

		public AClosed<AClosed<TUp, T1, T2, T3, T4>, T1, T2, T3> Close(T3 t3)
		{
			return new AClosed<AClosed<TUp, T1, T2, T3, T4>, T1, T2, T3>
			{
				V = t3,
				Up = this
			};
		}

		public APooled<AClosed<TUp, T1, T2, T3, T4>, T1, T2, T3> Pooled()
		{
			return ((APooled<AClosed<TUp, T1, T2, T3, T4>, T1, T2, T3>)null).Request<APooled<AClosed<TUp, T1, T2, T3, T4>, T1, T2, T3>, AClosed<TUp, T1, T2, T3, T4>>(this);
		}

		public ClosurePoolHandle<APooled<AClosed<TUp, T1, T2, T3, T4>, T1, T2, T3>, AClosed<TUp, T1, T2, T3, T4>, IAction<T1, T2, T3>> Pooled2()
		{
			return ((APooled<AClosed<TUp, T1, T2, T3, T4>, T1, T2, T3>)null).Request2<APooled<AClosed<TUp, T1, T2, T3, T4>, T1, T2, T3>, AClosed<TUp, T1, T2, T3, T4>, IAction<T1, T2, T3>>(null, this);
		}
	}

	public struct AClosed<TUp, T1, T2, T3, T4, T5> : IAction<T1, T2, T3, T4>, IActionBase<Action<T1, T2, T3, T4>> where TUp : IAction<T1, T2, T3, T4, T5>
	{
		public TUp Up;

		public T5 V;

		public void Invoke(T1 t1, T2 t2, T3 t3, T4 t4)
		{
			ref TUp up = ref Up;
			T5 v = V;
			up.Invoke(t1, t2, t3, t4, v);
		}

		public AClosed<AClosed<TUp, T1, T2, T3, T4, T5>, T1, T2, T3, T4> Close(T4 t4)
		{
			return new AClosed<AClosed<TUp, T1, T2, T3, T4, T5>, T1, T2, T3, T4>
			{
				V = t4,
				Up = this
			};
		}

		public APooled<AClosed<TUp, T1, T2, T3, T4, T5>, T1, T2, T3, T4> Pooled()
		{
			return ((APooled<AClosed<TUp, T1, T2, T3, T4, T5>, T1, T2, T3, T4>)null).Request<APooled<AClosed<TUp, T1, T2, T3, T4, T5>, T1, T2, T3, T4>, AClosed<TUp, T1, T2, T3, T4, T5>>(this);
		}

		public ClosurePoolHandle<APooled<AClosed<TUp, T1, T2, T3, T4, T5>, T1, T2, T3, T4>, AClosed<TUp, T1, T2, T3, T4, T5>, IAction<T1, T2, T3, T4>> Pooled2()
		{
			return ((APooled<AClosed<TUp, T1, T2, T3, T4, T5>, T1, T2, T3, T4>)null).Request2<APooled<AClosed<TUp, T1, T2, T3, T4, T5>, T1, T2, T3, T4>, AClosed<TUp, T1, T2, T3, T4, T5>, IAction<T1, T2, T3, T4>>(null, this);
		}
	}

	public struct AClosed2<TUp, T1, T2, T3, T4, T5> : IAction<T1, T2, T3>, IActionBase<Action<T1, T2, T3>> where TUp : IAction<T1, T2, T3, T4, T5>
	{
		public TUp Up;

		public T4 V4;

		public T5 V5;

		public void Invoke(T1 t1, T2 t2, T3 t3)
		{
			ref TUp up = ref Up;
			T4 v = V4;
			T5 v2 = V5;
			up.Invoke(t1, t2, t3, v, v2);
		}

		public APooled<AClosed2<TUp, T1, T2, T3, T4, T5>, T1, T2, T3> Pooled()
		{
			return ((APooled<AClosed2<TUp, T1, T2, T3, T4, T5>, T1, T2, T3>)null).Request<APooled<AClosed2<TUp, T1, T2, T3, T4, T5>, T1, T2, T3>, AClosed2<TUp, T1, T2, T3, T4, T5>>(this);
		}

		public ClosurePoolHandle<APooled<AClosed2<TUp, T1, T2, T3, T4, T5>, T1, T2, T3>, AClosed2<TUp, T1, T2, T3, T4, T5>, IAction<T1, T2, T3>> Pooled2()
		{
			return ((APooled<AClosed2<TUp, T1, T2, T3, T4, T5>, T1, T2, T3>)null).Request2<APooled<AClosed2<TUp, T1, T2, T3, T4, T5>, T1, T2, T3>, AClosed2<TUp, T1, T2, T3, T4, T5>, IAction<T1, T2, T3>>(null, this);
		}
	}

	public struct AClosed2<TUp, T1, T2, T3, T4> : IAction<T1, T2>, IActionBase<Action<T1, T2>> where TUp : IAction<T1, T2, T3, T4>
	{
		public TUp Up;

		public T3 V3;

		public T4 V4;

		public void Invoke(T1 t1, T2 t2)
		{
			ref TUp up = ref Up;
			T3 v = V3;
			T4 v2 = V4;
			up.Invoke(t1, t2, v, v2);
		}

		public APooled<AClosed2<TUp, T1, T2, T3, T4>, T1, T2> Pooled()
		{
			return ((APooled<AClosed2<TUp, T1, T2, T3, T4>, T1, T2>)null).Request<APooled<AClosed2<TUp, T1, T2, T3, T4>, T1, T2>, AClosed2<TUp, T1, T2, T3, T4>>(this);
		}

		public ClosurePoolHandle<APooled<AClosed2<TUp, T1, T2, T3, T4>, T1, T2>, AClosed2<TUp, T1, T2, T3, T4>, IAction<T1, T2>> Pooled2()
		{
			return ((APooled<AClosed2<TUp, T1, T2, T3, T4>, T1, T2>)null).Request2<APooled<AClosed2<TUp, T1, T2, T3, T4>, T1, T2>, AClosed2<TUp, T1, T2, T3, T4>, IAction<T1, T2>>(null, this);
		}
	}

	public struct AClosed2<TUp, T1, T2, T3> : IAction<T1>, IActionBase<Action<T1>> where TUp : IAction<T1, T2, T3>
	{
		public TUp Up;

		public T2 V2;

		public T3 V3;

		public void Invoke(T1 t1)
		{
			ref TUp up = ref Up;
			T2 v = V2;
			T3 v2 = V3;
			up.Invoke(t1, v, v2);
		}

		public APooled<AClosed2<TUp, T1, T2, T3>, T1> Pooled()
		{
			return ((APooled<AClosed2<TUp, T1, T2, T3>, T1>)null).Request<APooled<AClosed2<TUp, T1, T2, T3>, T1>, AClosed2<TUp, T1, T2, T3>>(this);
		}

		public ClosurePoolHandle<APooled<AClosed2<TUp, T1, T2, T3>, T1>, AClosed2<TUp, T1, T2, T3>, IAction<T1>> Pooled2()
		{
			return ((APooled<AClosed2<TUp, T1, T2, T3>, T1>)null).Request2<APooled<AClosed2<TUp, T1, T2, T3>, T1>, AClosed2<TUp, T1, T2, T3>, IAction<T1>>(null, this);
		}
	}

	public struct AClosed2<TUp, T1, T2> : IAction, IActionBase<Action> where TUp : IAction<T1, T2>
	{
		public TUp Up;

		public T1 V1;

		public T2 V2;

		public void Invoke()
		{
			ref TUp up = ref Up;
			T1 v = V1;
			T2 v2 = V2;
			up.Invoke(v, v2);
		}

		public APooled<AClosed2<TUp, T1, T2>> Pooled()
		{
			return ((APooled<AClosed2<TUp, T1, T2>>)null).Request<APooled<AClosed2<TUp, T1, T2>>, AClosed2<TUp, T1, T2>>(this);
		}

		public ClosurePoolHandle<APooled<AClosed2<TUp, T1, T2>>, AClosed2<TUp, T1, T2>, IAction> Pooled2()
		{
			return ((APooled<AClosed2<TUp, T1, T2>>)null).Request2<APooled<AClosed2<TUp, T1, T2>>, AClosed2<TUp, T1, T2>, IAction>(null, this);
		}
	}

	public struct AClosed3<TUp, T1, T2, T3, T4, T5> : IAction<T1, T2>, IActionBase<Action<T1, T2>> where TUp : IAction<T1, T2, T3, T4, T5>
	{
		public TUp Up;

		public T3 V3;

		public T4 V4;

		public T5 V5;

		public void Invoke(T1 t1, T2 t2)
		{
			ref TUp up = ref Up;
			T3 v = V3;
			T4 v2 = V4;
			T5 v3 = V5;
			up.Invoke(t1, t2, v, v2, v3);
		}

		public APooled<AClosed3<TUp, T1, T2, T3, T4, T5>, T1, T2> Pooled()
		{
			return ((APooled<AClosed3<TUp, T1, T2, T3, T4, T5>, T1, T2>)null).Request<APooled<AClosed3<TUp, T1, T2, T3, T4, T5>, T1, T2>, AClosed3<TUp, T1, T2, T3, T4, T5>>(this);
		}

		public ClosurePoolHandle<APooled<AClosed3<TUp, T1, T2, T3, T4, T5>, T1, T2>, AClosed3<TUp, T1, T2, T3, T4, T5>, IAction<T1, T2>> Pooled2()
		{
			return ((APooled<AClosed3<TUp, T1, T2, T3, T4, T5>, T1, T2>)null).Request2<APooled<AClosed3<TUp, T1, T2, T3, T4, T5>, T1, T2>, AClosed3<TUp, T1, T2, T3, T4, T5>, IAction<T1, T2>>(null, this);
		}
	}

	public struct AClosed3<TUp, T1, T2, T3, T4> : IAction<T1>, IActionBase<Action<T1>> where TUp : IAction<T1, T2, T3, T4>
	{
		public TUp Up;

		public T2 V2;

		public T3 V3;

		public T4 V4;

		public void Invoke(T1 t1)
		{
			ref TUp up = ref Up;
			T2 v = V2;
			T3 v2 = V3;
			T4 v3 = V4;
			up.Invoke(t1, v, v2, v3);
		}

		public APooled<AClosed3<TUp, T1, T2, T3, T4>, T1> Pooled()
		{
			return ((APooled<AClosed3<TUp, T1, T2, T3, T4>, T1>)null).Request<APooled<AClosed3<TUp, T1, T2, T3, T4>, T1>, AClosed3<TUp, T1, T2, T3, T4>>(this);
		}

		public ClosurePoolHandle<APooled<AClosed3<TUp, T1, T2, T3, T4>, T1>, AClosed3<TUp, T1, T2, T3, T4>, IAction<T1>> Pooled2()
		{
			return ((APooled<AClosed3<TUp, T1, T2, T3, T4>, T1>)null).Request2<APooled<AClosed3<TUp, T1, T2, T3, T4>, T1>, AClosed3<TUp, T1, T2, T3, T4>, IAction<T1>>(null, this);
		}
	}

	public struct AClosed3<TUp, T1, T2, T3> : IAction, IActionBase<Action> where TUp : IAction<T1, T2, T3>
	{
		public TUp Up;

		public T1 V1;

		public T2 V2;

		public T3 V3;

		public void Invoke()
		{
			ref TUp up = ref Up;
			T1 v = V1;
			T2 v2 = V2;
			T3 v3 = V3;
			up.Invoke(v, v2, v3);
		}

		public APooled<AClosed3<TUp, T1, T2, T3>> Pooled()
		{
			return ((APooled<AClosed3<TUp, T1, T2, T3>>)null).Request<APooled<AClosed3<TUp, T1, T2, T3>>, AClosed3<TUp, T1, T2, T3>>(this);
		}

		public ClosurePoolHandle<APooled<AClosed3<TUp, T1, T2, T3>>, AClosed3<TUp, T1, T2, T3>, IAction> Pooled2()
		{
			return ((APooled<AClosed3<TUp, T1, T2, T3>>)null).Request2<APooled<AClosed3<TUp, T1, T2, T3>>, AClosed3<TUp, T1, T2, T3>, IAction>(null, this);
		}
	}

	public struct AClosed4<TUp, T1, T2, T3, T4, T5> : IAction<T1>, IActionBase<Action<T1>> where TUp : IAction<T1, T2, T3, T4, T5>
	{
		public TUp Up;

		public T2 V2;

		public T3 V3;

		public T4 V4;

		public T5 V5;

		public void Invoke(T1 t1)
		{
			ref TUp up = ref Up;
			T2 v = V2;
			T3 v2 = V3;
			T4 v3 = V4;
			T5 v4 = V5;
			up.Invoke(t1, v, v2, v3, v4);
		}

		public APooled<AClosed4<TUp, T1, T2, T3, T4, T5>, T1> Pooled()
		{
			return ((APooled<AClosed4<TUp, T1, T2, T3, T4, T5>, T1>)null).Request<APooled<AClosed4<TUp, T1, T2, T3, T4, T5>, T1>, AClosed4<TUp, T1, T2, T3, T4, T5>>(this);
		}

		public ClosurePoolHandle<APooled<AClosed4<TUp, T1, T2, T3, T4, T5>, T1>, AClosed4<TUp, T1, T2, T3, T4, T5>, IAction<T1>> Pooled2()
		{
			return ((APooled<AClosed4<TUp, T1, T2, T3, T4, T5>, T1>)null).Request2<APooled<AClosed4<TUp, T1, T2, T3, T4, T5>, T1>, AClosed4<TUp, T1, T2, T3, T4, T5>, IAction<T1>>(null, this);
		}
	}

	public struct AClosed4<TUp, T1, T2, T3, T4> : IAction, IActionBase<Action> where TUp : IAction<T1, T2, T3, T4>
	{
		public TUp Up;

		public T1 V1;

		public T2 V2;

		public T3 V3;

		public T4 V4;

		public void Invoke()
		{
			ref TUp up = ref Up;
			T1 v = V1;
			T2 v2 = V2;
			T3 v3 = V3;
			T4 v4 = V4;
			up.Invoke(v, v2, v3, v4);
		}

		public APooled<AClosed4<TUp, T1, T2, T3, T4>> Pooled()
		{
			return ((APooled<AClosed4<TUp, T1, T2, T3, T4>>)null).Request<APooled<AClosed4<TUp, T1, T2, T3, T4>>, AClosed4<TUp, T1, T2, T3, T4>>(this);
		}

		public ClosurePoolHandle<APooled<AClosed4<TUp, T1, T2, T3, T4>>, AClosed4<TUp, T1, T2, T3, T4>, IAction> Pooled2()
		{
			return ((APooled<AClosed4<TUp, T1, T2, T3, T4>>)null).Request2<APooled<AClosed4<TUp, T1, T2, T3, T4>>, AClosed4<TUp, T1, T2, T3, T4>, IAction>(null, this);
		}
	}

	public struct AClosed5<TUp, T1, T2, T3, T4, T5> : IAction, IActionBase<Action> where TUp : IAction<T1, T2, T3, T4, T5>
	{
		public TUp Up;

		public T1 V1;

		public T2 V2;

		public T3 V3;

		public T4 V4;

		public T5 V5;

		public void Invoke()
		{
			ref TUp up = ref Up;
			T1 v = V1;
			T2 v2 = V2;
			T3 v3 = V3;
			T4 v4 = V4;
			T5 v5 = V5;
			up.Invoke(v, v2, v3, v4, v5);
		}

		public APooled<AClosed5<TUp, T1, T2, T3, T4, T5>> Pooled()
		{
			return ((APooled<AClosed5<TUp, T1, T2, T3, T4, T5>>)null).Request<APooled<AClosed5<TUp, T1, T2, T3, T4, T5>>, AClosed5<TUp, T1, T2, T3, T4, T5>>(this);
		}

		public ClosurePoolHandle<APooled<AClosed5<TUp, T1, T2, T3, T4, T5>>, AClosed5<TUp, T1, T2, T3, T4, T5>, IAction> Pooled2()
		{
			return ((APooled<AClosed5<TUp, T1, T2, T3, T4, T5>>)null).Request2<APooled<AClosed5<TUp, T1, T2, T3, T4, T5>>, AClosed5<TUp, T1, T2, T3, T4, T5>, IAction>(null, this);
		}
	}

	public struct APadded<TUp, T1> : IAction<T1>, IActionBase<Action<T1>> where TUp : struct, IAction
	{
		public TUp Up;

		public void Invoke(T1 t1)
		{
			Up.Invoke();
		}

		public TUp Close(T1 t1)
		{
			return Up;
		}

		public APadded<APadded<TUp, T1>, T1, T2> Pad<T2>()
		{
			return new APadded<APadded<TUp, T1>, T1, T2>
			{
				Up = this
			};
		}

		public APooled<APadded<TUp, T1>, T1> Pooled()
		{
			return ((APooled<APadded<TUp, T1>, T1>)null).Request<APooled<APadded<TUp, T1>, T1>, APadded<TUp, T1>>(this);
		}

		public ClosurePoolHandle<APooled<APadded<TUp, T1>, T1>, APadded<TUp, T1>, IAction<T1>> Pooled2()
		{
			return ((APooled<APadded<TUp, T1>, T1>)null).Request2<APooled<APadded<TUp, T1>, T1>, APadded<TUp, T1>, IAction<T1>>(null, this);
		}
	}

	public struct APadded<TUp, T1, T2> : IAction<T1, T2>, IActionBase<Action<T1, T2>> where TUp : struct, IAction<T1>
	{
		public TUp Up;

		public void Invoke(T1 t1, T2 t2)
		{
			Up.Invoke(t1);
		}

		public TUp Close(T2 t2)
		{
			return Up;
		}

		public APadded<APadded<TUp, T1, T2>, T1, T2, T3> Pad<T3>()
		{
			return new APadded<APadded<TUp, T1, T2>, T1, T2, T3>
			{
				Up = this
			};
		}

		public APooled<APadded<TUp, T1, T2>, T1, T2> Pooled()
		{
			return ((APooled<APadded<TUp, T1, T2>, T1, T2>)null).Request<APooled<APadded<TUp, T1, T2>, T1, T2>, APadded<TUp, T1, T2>>(this);
		}

		public ClosurePoolHandle<APooled<APadded<TUp, T1, T2>, T1, T2>, APadded<TUp, T1, T2>, IAction<T1, T2>> Pooled2()
		{
			return ((APooled<APadded<TUp, T1, T2>, T1, T2>)null).Request2<APooled<APadded<TUp, T1, T2>, T1, T2>, APadded<TUp, T1, T2>, IAction<T1, T2>>(null, this);
		}
	}

	public struct APadded<TUp, T1, T2, T3> : IAction<T1, T2, T3>, IActionBase<Action<T1, T2, T3>> where TUp : struct, IAction<T1, T2>
	{
		public TUp Up;

		public void Invoke(T1 t1, T2 t2, T3 t3)
		{
			Up.Invoke(t1, t2);
		}

		public TUp Close(T3 t3)
		{
			return Up;
		}

		public APadded<APadded<TUp, T1, T2, T3>, T1, T2, T3, T4> Pad<T4>()
		{
			return new APadded<APadded<TUp, T1, T2, T3>, T1, T2, T3, T4>
			{
				Up = this
			};
		}

		public APooled<APadded<TUp, T1, T2, T3>, T1, T2, T3> Pooled()
		{
			return ((APooled<APadded<TUp, T1, T2, T3>, T1, T2, T3>)null).Request<APooled<APadded<TUp, T1, T2, T3>, T1, T2, T3>, APadded<TUp, T1, T2, T3>>(this);
		}

		public ClosurePoolHandle<APooled<APadded<TUp, T1, T2, T3>, T1, T2, T3>, APadded<TUp, T1, T2, T3>, IAction<T1, T2, T3>> Pooled2()
		{
			return ((APooled<APadded<TUp, T1, T2, T3>, T1, T2, T3>)null).Request2<APooled<APadded<TUp, T1, T2, T3>, T1, T2, T3>, APadded<TUp, T1, T2, T3>, IAction<T1, T2, T3>>(null, this);
		}
	}

	public struct APadded<TUp, T1, T2, T3, T4> : IAction<T1, T2, T3, T4>, IActionBase<Action<T1, T2, T3, T4>> where TUp : struct, IAction<T1, T2, T3>
	{
		public TUp Up;

		public void Invoke(T1 t1, T2 t2, T3 t3, T4 t4)
		{
			Up.Invoke(t1, t2, t3);
		}

		public TUp Close(T4 t4)
		{
			return Up;
		}

		public APooled<APadded<TUp, T1, T2, T3, T4>, T1, T2, T3, T4> Pooled()
		{
			return ((APooled<APadded<TUp, T1, T2, T3, T4>, T1, T2, T3, T4>)null).Request<APooled<APadded<TUp, T1, T2, T3, T4>, T1, T2, T3, T4>, APadded<TUp, T1, T2, T3, T4>>(this);
		}

		public ClosurePoolHandle<APooled<APadded<TUp, T1, T2, T3, T4>, T1, T2, T3, T4>, APadded<TUp, T1, T2, T3, T4>, IAction<T1, T2, T3, T4>> Pooled2()
		{
			return ((APooled<APadded<TUp, T1, T2, T3, T4>, T1, T2, T3, T4>)null).Request2<APooled<APadded<TUp, T1, T2, T3, T4>, T1, T2, T3, T4>, APadded<TUp, T1, T2, T3, T4>, IAction<T1, T2, T3, T4>>(null, this);
		}
	}

	public struct APadded<TUp, T1, T2, T3, T4, T5> : IAction<T1, T2, T3, T4, T5>, IActionBase<Action<T1, T2, T3, T4, T5>> where TUp : struct, IAction<T1, T2, T3, T4>
	{
		public TUp Up;

		public void Invoke(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
		{
			Up.Invoke(t1, t2, t3, t4);
		}

		public TUp Close(T5 t5)
		{
			return Up;
		}

		public APooled<APadded<TUp, T1, T2, T3, T4, T5>, T1, T2, T3, T4, T5> Pooled()
		{
			return ((APooled<APadded<TUp, T1, T2, T3, T4, T5>, T1, T2, T3, T4, T5>)null).Request<APooled<APadded<TUp, T1, T2, T3, T4, T5>, T1, T2, T3, T4, T5>, APadded<TUp, T1, T2, T3, T4, T5>>(this);
		}

		public ClosurePoolHandle<APooled<APadded<TUp, T1, T2, T3, T4, T5>, T1, T2, T3, T4, T5>, APadded<TUp, T1, T2, T3, T4, T5>, IAction<T1, T2, T3, T4, T5>> Pooled2()
		{
			return ((APooled<APadded<TUp, T1, T2, T3, T4, T5>, T1, T2, T3, T4, T5>)null).Request2<APooled<APadded<TUp, T1, T2, T3, T4, T5>, T1, T2, T3, T4, T5>, APadded<TUp, T1, T2, T3, T4, T5>, IAction<T1, T2, T3, T4, T5>>(null, this);
		}
	}

	public class APooled<TClosed> : Pooling.PooledBase<TClosed>, IAction, IActionBase<Action> where TClosed : struct, IAction
	{
		public readonly Action Action;

		public APooled()
		{
			Action = delegate
			{
				Closed.Invoke();
			};
		}

		public void Invoke()
		{
			Action();
		}
	}

	public class APooled<TClosed, T1> : Pooling.PooledBase<TClosed>, IAction<T1>, IActionBase<Action<T1>> where TClosed : struct, IAction<T1>
	{
		public readonly Action<T1> Action;

		public APooled()
		{
			Action = delegate(T1 a)
			{
				Closed.Invoke(a);
			};
		}

		public void Invoke(T1 t1)
		{
			Action(t1);
		}
	}

	public class APooled<TClosed, T1, T2> : Pooling.PooledBase<TClosed>, IAction<T1, T2>, IActionBase<Action<T1, T2>> where TClosed : struct, IAction<T1, T2>
	{
		public readonly Action<T1, T2> Action;

		public APooled()
		{
			Action = delegate(T1 a, T2 b)
			{
				Closed.Invoke(a, b);
			};
		}

		public void Invoke(T1 t1, T2 t2)
		{
			Action(t1, t2);
		}
	}

	public class APooled<TClosed, T1, T2, T3> : Pooling.PooledBase<TClosed>, IAction<T1, T2, T3>, IActionBase<Action<T1, T2, T3>> where TClosed : struct, IAction<T1, T2, T3>
	{
		public readonly Action<T1, T2, T3> Action;

		public APooled()
		{
			Action = delegate(T1 a, T2 b, T3 c)
			{
				Closed.Invoke(a, b, c);
			};
		}

		public void Invoke(T1 t1, T2 t2, T3 t3)
		{
			Action(t1, t2, t3);
		}
	}

	public class APooled<TClosed, T1, T2, T3, T4> : Pooling.PooledBase<TClosed>, IAction<T1, T2, T3, T4>, IActionBase<Action<T1, T2, T3, T4>> where TClosed : struct, IAction<T1, T2, T3, T4>
	{
		public readonly Action<T1, T2, T3, T4> Action;

		public APooled()
		{
			Action = delegate(T1 a, T2 b, T3 c, T4 d)
			{
				Closed.Invoke(a, b, c, d);
			};
		}

		public void Invoke(T1 t1, T2 t2, T3 t3, T4 t4)
		{
			Action(t1, t2, t3, t4);
		}
	}

	public class APooled<TClosed, T1, T2, T3, T4, T5> : Pooling.PooledBase<TClosed>, IAction<T1, T2, T3, T4, T5>, IActionBase<Action<T1, T2, T3, T4, T5>> where TClosed : struct, IAction<T1, T2, T3, T4, T5>
	{
		public readonly Action<T1, T2, T3, T4, T5> Action;

		public APooled()
		{
			Action = delegate(T1 a, T2 b, T3 c, T4 d, T5 e)
			{
				Closed.Invoke(a, b, c, d, e);
			};
		}

		public void Invoke(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
		{
			Action(t1, t2, t3, t4, t5);
		}
	}

	public static FBase<TReturn> Make<TReturn>(this Func<TReturn> f)
	{
		return new FBase<TReturn>
		{
			F = f
		};
	}

	public static FBase<T1, TReturn> Make<T1, TReturn>(this Func<T1, TReturn> f)
	{
		return new FBase<T1, TReturn>
		{
			F = f
		};
	}

	public static FBase<T1, T2, TReturn> Make<T1, T2, TReturn>(this Func<T1, T2, TReturn> f)
	{
		return new FBase<T1, T2, TReturn>
		{
			F = f
		};
	}

	public static FBase<T1, T2, T3, TReturn> Make<T1, T2, T3, TReturn>(this Func<T1, T2, T3, TReturn> f)
	{
		return new FBase<T1, T2, T3, TReturn>
		{
			F = f
		};
	}

	public static FBase<T1, T2, T3, T4, TReturn> Make<T1, T2, T3, T4, TReturn>(this Func<T1, T2, T3, T4, TReturn> f)
	{
		return new FBase<T1, T2, T3, T4, TReturn>
		{
			F = f
		};
	}

	public static FBase<T1, T2, T3, T4, T5, TReturn> Make<T1, T2, T3, T4, T5, TReturn>(this Func<T1, T2, T3, T4, T5, TReturn> f)
	{
		return new FBase<T1, T2, T3, T4, T5, TReturn>
		{
			F = f
		};
	}

	public static ABase Make(this Action f)
	{
		return new ABase
		{
			F = f
		};
	}

	public static ABase<T1> Make<T1>(this Action<T1> f)
	{
		return new ABase<T1>
		{
			F = f
		};
	}

	public static ABase<T1, T2> Make<T1, T2>(this Action<T1, T2> f)
	{
		return new ABase<T1, T2>
		{
			F = f
		};
	}

	public static ABase<T1, T2, T3> Make<T1, T2, T3>(this Action<T1, T2, T3> f)
	{
		return new ABase<T1, T2, T3>
		{
			F = f
		};
	}

	public static ABase<T1, T2, T3, T4> Make<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> f)
	{
		return new ABase<T1, T2, T3, T4>
		{
			F = f
		};
	}

	public static ABase<T1, T2, T3, T4, T5> Make<T1, T2, T3, T4, T5>(this Action<T1, T2, T3, T4, T5> f)
	{
		return new ABase<T1, T2, T3, T4, T5>
		{
			F = f
		};
	}
}
