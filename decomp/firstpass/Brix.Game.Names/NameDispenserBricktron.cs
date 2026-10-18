using System.Collections.Generic;
using Brix.External.Signals;

namespace Brix.Game.Names;

public class NameDispenserBricktron : INameDispenser
{
	private readonly IEnumerator<string> _e;

	private readonly Signal<string> _newNameReserved = new Signal<string>();

	public NameDispenserBricktron(string path)
	{
		_e = new NameJsonEnumerator(path);
	}

	public void Destroy()
	{
		_e.Dispose();
	}

	public string GetName()
	{
		_e.MoveNext();
		_newNameReserved.Invoke(_e.Current);
		return _e.Current;
	}

	public Signal<string> GetNewNameSignal()
	{
		return _newNameReserved;
	}
}
