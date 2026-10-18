using System;
using System.Collections.Generic;
using System.IO;

namespace Brix.Game.Names;

public class NameLoader
{
	private string path;

	public NameLoader(string path)
	{
		this.path = path;
	}

	public HashSet<Name> Load()
	{
		string[] array = File.ReadAllLines(path);
		HashSet<Name> hashSet = new HashSet<Name>();
		for (int i = 1; i < array.Length; i++)
		{
			hashSet.Add(LoadLine(array[i]));
		}
		return hashSet;
	}

	public Name LoadLine(string line)
	{
		string[] array = line.Split(new char[1] { ',' }, StringSplitOptions.None);
		return new Name(set: (!(array[4] == string.Empty)) ? int.Parse(array[4]) : 0, name: array[0], nature: StringToNature(array[2]), quality: int.Parse(array[3]));
	}

	private Name.Nature StringToNature(string str)
	{
		return str switch
		{
			"Véritable" => Name.Nature.official, 
			"Joke" => Name.Nature.joke, 
			"Référence" => Name.Nature.reference, 
			_ => Name.Nature.error, 
		};
	}
}
