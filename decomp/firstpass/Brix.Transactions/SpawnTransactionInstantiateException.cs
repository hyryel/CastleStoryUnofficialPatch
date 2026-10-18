using System;
using Brix.External.Factories;

namespace Brix.Transactions;

public class SpawnTransactionInstantiateException : Exception
{
	public SpawnTransactionInstantiateException(Factory.AssetKey assetKey, Exception e)
		: base(string.Concat("Instantiate failed for key '", assetKey, "': ", e))
	{
	}
}
