using System.Collections.Generic;
using Brix.Legacy;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Bricktron;

public class GestionnaireDeNom
{
	public static Classeur<string> liste = new Classeur<string>();

	private static HashSet<string> retaits = new HashSet<string>();

	private static Dictionary<string, GameObject> repertoire = new Dictionary<string, GameObject>();

	private static Dictionary<string, int> quantites = new Dictionary<string, int>();

	public static void CompilePhrase(Livre.Phrase phrase, PhraseCompileInfo info)
	{
		if (!phrase.entete.Contains("SECTION"))
		{
			liste.Ajout(phrase.entete);
		}
	}

	public static string GetPrenom()
	{
		return GetPrenom(vraiNom: true);
	}

	public static string GetPrenom(bool vraiNom)
	{
		if (liste.Count == 0)
		{
			return "Bricktron";
		}
		int num = Mathf.FloorToInt(Random.value * (float)liste.Count);
		if (num == liste.Count)
		{
			num--;
		}
		string text = liste[num];
		if (vraiNom)
		{
			liste.Remove(text);
		}
		else
		{
			string[] array = new string[6] { " bwarg", " gwrd", " gwerp", " oueb", " wrap", " sushi" };
			text += array[RandomInt(array.Length)];
			if (repertoire.ContainsKey(text))
			{
				text += array[RandomInt(array.Length)];
			}
		}
		return text;
	}

	public static string GetId(GameObject obj)
	{
		string text = obj.name.Substring(0, 2);
		if (!quantites.ContainsKey(text))
		{
			quantites.Add(text, 0);
		}
		int num = quantites[text];
		string text2 = (string.Empty + num).PadLeft(4, "0"[0]);
		while (repertoire.ContainsKey(text + text2))
		{
			num++;
			text2 = (string.Empty + num).PadLeft(4, "0"[0]);
		}
		quantites[text] = (num + 1) % 10000;
		return text + text2;
	}

	public static void Reserver(string nom)
	{
		if (liste.Contains(nom))
		{
			liste.Remove(nom);
		}
		retaits.Add(nom);
	}

	public static void Liberer(Nom qui)
	{
		repertoire.Remove(qui.nom);
	}

	public static void Liberer(string nom)
	{
		if (!"Bricktron(Clone)".Equals(nom) && liste.BinarySearch(nom) < 0)
		{
			liste.Ajout(nom);
		}
	}

	public static void RenommerExclusif(GameObject qui, string ancien, string nouveau)
	{
		if (!string.IsNullOrEmpty(ancien))
		{
			Liberer(ancien);
		}
		if (NetworkServer.active)
		{
			Reserver(nouveau);
		}
		Renommer(qui, ancien, nouveau);
	}

	public static void Renommer(GameObject qui, string ancien, string nouveau)
	{
		if (!(qui == null))
		{
			if (!string.IsNullOrEmpty(ancien) && repertoire.ContainsKey(ancien))
			{
				repertoire.Remove(ancien);
			}
			if (!string.IsNullOrEmpty(nouveau) && !repertoire.ContainsKey(nouveau))
			{
				repertoire.Add(nouveau, qui);
			}
		}
	}

	public static void Terminer()
	{
		foreach (string retait in retaits)
		{
			Liberer(retait);
		}
		repertoire.Clear();
		quantites.Clear();
		retaits.Clear();
	}

	private static int RandomInt(int max)
	{
		if (max <= 0)
		{
			return 0;
		}
		float f = (float)((double)Random.value * (1.0 * (double)max));
		int num = Mathf.FloorToInt(f);
		if (num == max)
		{
			num = RandomInt(max);
		}
		return num;
	}

	public static GameObject Get(string s)
	{
		if (repertoire.ContainsKey(s))
		{
			return repertoire[s];
		}
		return null;
	}
}
