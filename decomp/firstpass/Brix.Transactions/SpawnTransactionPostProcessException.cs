using System;

namespace Brix.Transactions;

public class SpawnTransactionPostProcessException : Exception
{
	public SpawnTransactionPostProcessException(SpawnPostProcessor p, Exception e)
		: base(string.Concat("PostProcess failed for processor '", p.GetType(), "': ", e))
	{
	}
}
