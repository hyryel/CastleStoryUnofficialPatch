using System;
using System.Collections;
using System.Collections.Generic;
using Brix.Game.Bricktron;
using Smooth.Collections;
using UnityEngine;

namespace Brix.Game.Names;

public class NameJsonEnumerator : IEnumerator<string>, IDisposable, IEnumerator
{
	private NameJsonFile _file;

	private IEnumerator<string> _e;

	private System.Random _random;

	public const int MaxConsecutiveCollisions = 10000;

	object IEnumerator.Current => Current;

	public HashSet<string> ExistingNames => Nom.Reserves;

	public string Current => _e.Current;

	public NameJsonEnumerator(string path)
	{
		_file = new NameJsonFile(path);
		_file.ReadFromDisk();
		_e = Enumerate();
	}

	public void Dispose()
	{
		_file.Destroy();
		_file = null;
		_e.Dispose();
		_e = null;
	}

	private IEnumerator<string> Enumerate()
	{
		int tries = 0;
		while (tries < 10000)
		{
			_random = new System.Random();
			UnityEngine.Random.InitState(_random.Next());
			_file.Entries.Shuffle();
			foreach (NameJsonFile.IEntry e in _file.Entries)
			{
				int score = _random.Next(10);
				if (e.Score < score)
				{
					continue;
				}
				if (e is NameJsonFile.ThemeEntry)
				{
					NameJsonFile.ThemeEntry theme = e as NameJsonFile.ThemeEntry;
					theme.Data.Shuffle();
					int chanceScore = theme.RepeatChance;
					int chanceThreshold = 0;
					foreach (NameJsonFile.NameEntry nameEntry in theme.Data)
					{
						if (chanceThreshold > chanceScore && score > nameEntry.Score)
						{
							break;
						}
						tries++;
						if (!ExistingNames.Contains(nameEntry.Name))
						{
							tries = 0;
							yield return nameEntry.Name;
						}
						score = _random.Next(10);
						chanceThreshold = _random.Next(10);
					}
				}
				else if (e is NameJsonFile.NameEntry)
				{
					NameJsonFile.NameEntry nameEntry2 = e as NameJsonFile.NameEntry;
					tries++;
					if (!ExistingNames.Contains(nameEntry2.Name))
					{
						tries = 0;
						yield return nameEntry2.Name;
					}
				}
			}
		}
	}

	public bool MoveNext()
	{
		return _e.MoveNext();
	}

	public void Reset()
	{
		_e.Dispose();
		_e = Enumerate();
	}
}
