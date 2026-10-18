using System.Collections;
using System.Collections.Generic;

public interface IClasseur<T> : IEnumerable<T>, IEnumerable
{
	int Count { get; }

	T[] GetInternalArray();

	void ClearInternalArrayPadding();

	void RemoveInternalArrayPadding();
}
