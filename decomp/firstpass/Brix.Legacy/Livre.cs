using System;
using System.Collections.Generic;
using System.IO;
using Brix.Engine.Abstract;
using Brix.Game.Bricktron;
using Brix.Util;
using UnityEngine;

namespace Brix.Legacy;

public class Livre
{
	public class Patron
	{
		public string emplacement;

		public string separateur;

		public Action<Phrase, PhraseCompileInfo> Compile;

		public Patron(Phrase p)
		{
			emplacement = p.terme[0];
			separateur = p.terme[1];
			if (p.terme.Length >= 3)
			{
				Compile = GetCompilateur(p.terme[2]);
			}
		}
	}

	public class Chapitre : List<Page>
	{
		public string entete;

		public void Compile()
		{
			for (int i = 0; i < base.Count; i++)
			{
				base[i].Compile(this, i);
			}
		}
	}

	public class Page : List<Paragraphe>
	{
		public string entete;

		public void Compile(Chapitre chapitre, int cIndex)
		{
			for (int i = 0; i < base.Count; i++)
			{
				base[i].Compile(chapitre, cIndex, this, i);
			}
		}

		public void Ajout(Paragraphe item)
		{
			Add(item);
			if (base.Count == 1)
			{
				if (base[0].entete.StartsWith(patronActuel.separateur))
				{
					base[0].entete = base[0].entete.Substring(patronActuel.separateur.Length + 1);
				}
				base[0].entete.Trim();
				entete = base[0].entete;
			}
		}
	}

	public class Paragraphe : List<Phrase>
	{
		public string entete;

		public void Compile(Chapitre chapitre, int cIndex, Page page, int pIndex)
		{
			if (patronActuel.Compile != null)
			{
				for (int i = 0; i < base.Count; i++)
				{
					patronActuel.Compile(base[i], new PhraseCompileInfo(chapitre, cIndex, page, pIndex, this, i));
				}
			}
			else
			{
				for (int j = 0; j < base.Count; j++)
				{
					Phrase.CompilePhrase(base[j], new PhraseCompileInfo(chapitre, cIndex, page, pIndex, this, j));
				}
			}
		}

		public void Ajout(Phrase item)
		{
			Add(item);
			if (base.Count == 1)
			{
				entete = base[0].entete;
			}
		}
	}

	public class Phrase
	{
		public string entete;

		public string[] terme;

		public Phrase(string ligne)
		{
			string[] array = ligne.Split(new string[1] { ":" }, StringSplitOptions.RemoveEmptyEntries);
			entete = array[0].Trim();
			if (array.Length > 1)
			{
				terme = Trier(array[1]);
				for (int i = 0; i < terme.Length; i++)
				{
					terme[i] = terme[i].Trim();
				}
			}
			else
			{
				terme = new string[0];
			}
		}

		public static void CompilePhrase(Phrase phrase, PhraseCompileInfo info)
		{
			throw new NotImplementedException();
		}

		public string[] Trier(string s)
		{
			char c = '"';
			string[] array = s.Split(new string[1] { string.Empty + c }, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length == 1)
			{
				terme = s.Split(new string[1] { "/" }, StringSplitOptions.RemoveEmptyEntries);
				string[] array2 = terme;
				foreach (string text in array2)
				{
					text.Trim();
				}
				return terme;
			}
			List<string> list = new List<string>();
			string[] array3;
			for (int j = 0; j < array.Length; j++)
			{
				array3 = (((j & 1) != 0) ? new string[1] { array[j] } : array[j].Split(new string[1] { "/" }, StringSplitOptions.RemoveEmptyEntries));
				string[] array4 = array3;
				foreach (string text2 in array4)
				{
					text2.Trim();
					if (text2 != string.Empty && text2 != " ")
					{
						list.Add(text2);
					}
				}
			}
			array3 = new string[list.Count];
			list.CopyTo(array3);
			return array3;
		}
	}

	public static Dictionary<string, Patron> patrons;

	public static Dictionary<string, Chapitre> dico;

	public static Patron patronActuel;

	private static Dictionary<string, Chapitre> cPatronEmplacementChapitreMap = new Dictionary<string, Chapitre>();

	public static Chapitre Lire(string patron)
	{
		return Lire(patron, null);
	}

	public static Chapitre Lire(string patron, string emplacement)
	{
		string key = patron + "|" + emplacement;
		if (cPatronEmplacementChapitreMap.ContainsKey(key))
		{
			return cPatronEmplacementChapitreMap[key];
		}
		string contenu;
		Phrase[] array;
		if (patrons == null)
		{
			patrons = new Dictionary<string, Patron>();
			dico = new Dictionary<string, Chapitre>();
			contenu = File.ReadAllText("Info/Livre/Index.txt");
			array = Split(contenu);
			int num = 0;
			Phrase[] array2 = array;
			foreach (Phrase phrase in array2)
			{
				if (phrase != null)
				{
					patrons.Add(phrase.entete, new Patron(phrase));
					num++;
				}
			}
		}
		patronActuel = patrons[patron];
		if (emplacement == null)
		{
			emplacement = patronActuel.emplacement;
		}
		contenu = File.ReadAllText(emplacement);
		array = Split(contenu);
		Paragraphe[] paragraphe = Regroupe(array);
		Chapitre chapitre = Organise(paragraphe);
		chapitre.entete = patron;
		dico[emplacement] = chapitre;
		cPatronEmplacementChapitreMap.Add(key, chapitre);
		return chapitre;
	}

	public static void Compile(string patron)
	{
		Compile(patron, null);
	}

	public static void Compile(string patron, string emplacement)
	{
		patronActuel = patrons[patron];
		if (emplacement == null)
		{
			emplacement = patronActuel.emplacement;
		}
		if (dico.ContainsKey(emplacement))
		{
			Compile(dico[emplacement]);
		}
	}

	public static void Compile(Chapitre chapitre)
	{
		chapitre.Compile();
	}

	public static Chapitre Organise(Paragraphe[] paragraphe)
	{
		Chapitre chapitre = new Chapitre();
		foreach (Paragraphe paragraphe2 in paragraphe)
		{
			if (paragraphe2 == paragraphe[0] || paragraphe2[0].entete == patronActuel.separateur || paragraphe2[0].entete.Split(new string[1] { " " }, StringSplitOptions.None)[0] == patronActuel.separateur)
			{
				chapitre.Add(new Page());
			}
			chapitre[chapitre.Count - 1].Ajout(paragraphe2);
		}
		return chapitre;
	}

	public static Paragraphe[] Regroupe(Phrase[] lignes)
	{
		Page page = new Page();
		for (int i = 0; i < lignes.Length; i++)
		{
			if (i == 0 || (lignes[i] != null && lignes[i - 1] == null))
			{
				page.Add(new Paragraphe());
			}
			if (lignes[i] != null)
			{
				page[page.Count - 1].Ajout(lignes[i]);
			}
		}
		Paragraphe[] array = new Paragraphe[page.Count];
		page.CopyTo(array);
		return array;
	}

	public static Phrase[] Split(string contenu)
	{
		string[] array = contenu.Split(new string[2] { "\n\r", "\n" }, StringSplitOptions.None);
		Phrase[] array2 = new Phrase[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = array[i].Trim();
			if (array[i] == string.Empty || array[i].StartsWith("--"))
			{
				array[i] = null;
			}
			else
			{
				array2[i] = new Phrase(array[i]);
			}
		}
		return array2;
	}

	public static Vector3 GetVecteur3(string s)
	{
		Vector3 result = Vector3.zero;
		string[] array = s.Split(new string[1] { "," }, StringSplitOptions.RemoveEmptyEntries);
		if (array.Length == 3)
		{
			result = new Vector3(Calcule.ParseFloat(array[0]), Calcule.ParseFloat(array[1]), Calcule.ParseFloat(array[2]));
		}
		else if (array.Length == 2)
		{
			result = new Vector3(Calcule.ParseFloat(array[0]), 0f, Calcule.ParseFloat(array[1]));
		}
		else if (array.Length == 1)
		{
			result = Calcule.GetVecteur(int.Parse(array[0]));
		}
		return result;
	}

	public static Vector2 GetVecteur2(string s)
	{
		Vector3 vector = Vector3.zero;
		string[] array = s.Split(new string[1] { "," }, StringSplitOptions.RemoveEmptyEntries);
		if (array.Length == 2)
		{
			vector = new Vector3(Calcule.ParseFloat(array[0]), Calcule.ParseFloat(array[1]));
		}
		else if (array.Length == 1)
		{
			vector = Voxel.GetVecteur(int.Parse(array[0]));
		}
		return vector;
	}

	public static Action<Phrase, PhraseCompileInfo> GetCompilateur(string s)
	{
		return s switch
		{
			"GestionnaireDeNom" => GestionnaireDeNom.CompilePhrase, 
			"PieContenu_Menu" => PieContenu.MenuContenu.CompilePhrase, 
			"PieContenu_Bouton" => PieContenu.BoutonContenu.CompilePhrase, 
			_ => Phrase.CompilePhrase, 
		};
	}
}
