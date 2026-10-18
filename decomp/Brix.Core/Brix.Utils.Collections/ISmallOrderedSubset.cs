using System.Collections;
using System.Collections.Generic;

namespace Brix.Utils.Collections;

public interface ISmallOrderedSubset : ISmallSubset, ICollection<int>, IEnumerable<int>, IEnumerable, IList<int>
{
	int First { get; }

	int GetOrAddIndexOf(int x);

	void Append(int x);

	int Pop();
}
