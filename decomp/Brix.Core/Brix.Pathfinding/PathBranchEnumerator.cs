using System;
using System.Collections;
using System.Collections.Generic;

namespace Brix.Pathfinding;

public abstract class PathBranchEnumerator : IEnumerable<PathBranch>, IEnumerable, IEnumerator<PathBranch>, IDisposable, IEnumerator
{
	public class EmptyEnumerator : PathBranchEnumerator
	{
		public override PathBranch Current => PathBranch.Empty;

		public override bool MoveNext()
		{
			return false;
		}

		public override void Reset()
		{
		}
	}

	public static EmptyEnumerator Empty = new EmptyEnumerator();

	public abstract PathBranch Current { get; }

	object IEnumerator.Current => Current;

	public abstract bool MoveNext();

	public abstract void Reset();

	public void Dispose()
	{
	}

	public IEnumerator<PathBranch> GetEnumerator()
	{
		Reset();
		return this;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
