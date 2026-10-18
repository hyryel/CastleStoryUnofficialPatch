using System;
using System.Collections.Generic;
using Brix.Utils;
using Smooth.Algebraics;

namespace Assets.Standard_Assets.Brix.Game.AI;

public class UnbindedCache<TOwner, TInput, TResult> : IBindableCache<TOwner, TInput, TResult>
{
	public class BindedCache : IBindableCache<TOwner, TInput, TResult>
	{
		private readonly TOwner owner;

		private readonly UnbindedCache<TOwner, TInput, TResult> cache;

		public BindedCache(TOwner owner, UnbindedCache<TOwner, TInput, TResult> cache)
		{
			this.owner = owner;
			this.cache = cache;
		}

		public IBindableCache<TOwner, TInput, TResult> Bind(TOwner owner)
		{
			return this;
		}

		public void RefreshIfNeeded()
		{
			cache.RefreshIfNeeded(owner);
		}

		public TResult GetUnsafe(TInput input)
		{
			return cache.GetUnsafe(owner, input);
		}

		public TResult Get(TInput input)
		{
			return cache.Get(owner, input);
		}
	}

	private readonly Func<TOwner, long> getFrame;

	private readonly Func<TOwner, TInput, TResult> create;

	private Dictionary<TInput, TResult> cache;

	private long lastRefresh;

	private string ErrorMsg = "Bind the cache to a owner first.";

	public UnbindedCache(Func<TOwner, long> getFrame, Func<TOwner, TInput, TResult> create)
	{
		this.getFrame = getFrame;
		lastRefresh = 0L;
		this.create = create;
		cache = new Dictionary<TInput, TResult>();
	}

	protected void RefreshIfNeeded(TOwner owner)
	{
		long num = getFrame(owner);
		if (num > lastRefresh)
		{
			cache.Clear();
			lastRefresh = num;
		}
	}

	protected TResult Get(TOwner owner, TInput input)
	{
		RefreshIfNeeded(owner);
		return GetUnsafe(owner, input);
	}

	protected TResult GetUnsafe(TOwner owner, TInput input)
	{
		return cache.GetValueOrCreate(input, (TInput i, Tuple<UnbindedCache<TOwner, TInput, TResult>, TOwner> t) => t._1.create(t._2, i), Tuple.Create(this, owner));
	}

	protected void Clear()
	{
		cache.Clear();
	}

	public IBindableCache<TOwner, TInput, TResult> Bind(TOwner owner)
	{
		return new BindedCache(owner, this);
	}

	public TResult Get(TInput input)
	{
		throw new Exception(ErrorMsg);
	}

	public TResult GetUnsafe(TInput input)
	{
		throw new Exception(ErrorMsg);
	}

	public void RefreshIfNeeded()
	{
		throw new Exception(ErrorMsg);
	}
}
