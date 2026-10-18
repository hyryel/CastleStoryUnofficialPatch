using System;

namespace Brix.External.Factories;

public class FactoryException : Exception
{
	public FactoryException(string desc = "")
		: base(desc)
	{
	}
}
