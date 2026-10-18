using System;
using System.Collections.Generic;

public interface IPlug
{
	List<Plug> Plugs { get; }

	Dictionary<Type, Dictionary<string, Plug>> PlugMap { get; }
}
