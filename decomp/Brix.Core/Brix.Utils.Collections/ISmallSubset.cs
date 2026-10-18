using System.Collections;
using System.Collections.Generic;

namespace Brix.Utils.Collections;

public interface ISmallSubset : ICollection<int>, IEnumerable<int>, IEnumerable
{
	int Capacity { get; }

	bool Empty { get; }

	bool Full { get; }

	new bool Add(int x);

	void AddOrIgnore(int x);

	void RemoveOrIgnore(int x);
}
