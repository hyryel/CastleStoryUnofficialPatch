using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Brix.Engine;
using Brix.Engine.Nature;
using Brix.Game.AI;
using Brix.Game.Components;
using Brix.Game.Semantique;
using Brix.Input;
using Brix.UI.GameSelector;
using Brix.UI.Icons;
using Brix.Utils;
using Brix.Utils.Collections;
using Brix.Utils.Factories;
using Brix.Utils.Logging;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Legacy;

public class PieContenu
{
	public class Menu
	{
		public string nom;

		public string titre;

		public Classeur<Instruction> instructions = new Classeur<Instruction>();

		public int depth;

		public Classeur<Bouton> boutons = new Classeur<Bouton>();

		public MonoBehaviour[] sujets;

		public object objet;

		public int rep;

		public bool Test(string op, string tag)
		{
			try
			{
				return op switch
				{
					"Sel" => TestSel(tag), 
					"Hover" => TestHover(tag), 
					"If" => TestIf(tag), 
					_ => UnityEngine.Random.value < 0.5f, 
				};
			}
			catch (Exception message)
			{
				logger.Error(message);
			}
			return false;
		}

		public bool TestIf(string tag)
		{
			object obj = Instructions.Evaluate(tag);
			if (obj is bool)
			{
				return (bool)obj;
			}
			if (obj is int)
			{
				return (int)obj != 0;
			}
			return obj != null;
		}

		public bool TestSel(string tag)
		{
			if ((bool)UIGameObserver.projects.CurrentSelected && UIGameObserver.projects.CurrentSelected.CheckTag(tag))
			{
				return true;
			}
			if (InputModeController.GetCurrentMode() == InputMode.command && TestArray(tag, sujets))
			{
				return true;
			}
			return false;
		}

		public bool TestHover(string tag)
		{
			if (UIGameObserver.placedBlocks.AllHovered.Slinq().Any() && UIGameObserver.placedBlocks.AllHovered.Slinq().First().GetComponent<UISelectable>()
				.CheckTag(tag))
			{
				return true;
			}
			if (tag == "Tree" && UIGameObserver.dynamicObjects.IsHoveringTree)
			{
				return true;
			}
			if (tag == "Plant" && objet is PlantData)
			{
				return true;
			}
			if (tag == "Minerals" && objet is BoulderData)
			{
				return true;
			}
			if (tag == "Crystals" && objet is MineralData)
			{
				return true;
			}
			if (objet is GameObject)
			{
				GameObject gameObject = objet as GameObject;
				if (gameObject.IsNullOrReleased())
				{
					return false;
				}
				if (tag == "ResourceGranulaire" && Apparence.Is<Encombrement>(gameObject))
				{
					return true;
				}
				if (tag == "Ward" && Apparence.Is<Ward>(gameObject))
				{
					return true;
				}
				if (tag == "ResourceChunk" && (Apparence.Is<Outil>(gameObject) || Apparence.Is<Iron>(gameObject)))
				{
					return true;
				}
				if (gameObject.WithComponent((CraftingStation f, string s) => f.PieMenuTag == s, _default: false, tag))
				{
					return true;
				}
				if (tag == "Letter" && (bool)gameObject.GetComponent<LetterDropOnDestroy>())
				{
					return true;
				}
				RopeBridgeConfigurator component = gameObject.GetComponent<RopeBridgeConfigurator>();
				if ((bool)component && tag == "RopeBridge")
				{
					return true;
				}
			}
			return TestArray(tag, new object[1] { objet });
		}

		public bool TestArray(string tag, object[] objs)
		{
			return Oracle.TrouverTag(objs, tag) != null;
		}

		public string GetTypeArray(object[] objs)
		{
			string text = string.Empty;
			for (int i = 0; i < objs.Length; i++)
			{
				string text2 = ((objs[i] != null) ? objs[i].GetType().Name : string.Empty);
				text = text + ((!(text == string.Empty)) ? string.Empty : ", ") + text2;
			}
			return text;
		}

		public string Op(int i)
		{
			return instructions[i].entete;
		}

		public int Depth(int i)
		{
			return instructions[i].depth;
		}

		public void FinirDeCompiler()
		{
			for (int i = 0; i < instructions.Count; i++)
			{
				switch (Op(i))
				{
				case "Sel":
				case "Hover":
				case "If":
				case "Else":
				case "Submenu":
				{
					for (int j = i + 1; j < instructions.Count; j++)
					{
						if (Depth(j) != Depth(i))
						{
							continue;
						}
						if (Op(j) == "Else")
						{
							if (instructions[i].nextAt < 0)
							{
								instructions[i].nextAt = j;
							}
						}
						else if (Op(j) == "End")
						{
							instructions[i].endAt = j;
							break;
						}
					}
					break;
				}
				}
			}
		}

		public void PrintMenu()
		{
			string text = string.Empty;
			for (int i = 0; i < instructions.Count; i++)
			{
				string text2 = text;
				text = text2 + string.Empty + instructions[i].ligne / 10 + instructions[i].ligne % 10 + ". ";
				for (int j = 0; j < Depth(i); j++)
				{
					text += " ";
				}
				text = text + Op(i) + ((instructions[i].args.Length == 0) ? string.Empty : (" : " + instructions[i].args[0]));
				text2 = text;
				text = text2 + " (" + instructions[i].nextAt + ")";
				text += "\n";
			}
			logger.Warn("PrintMenu() : \n" + text);
		}

		public bool BatirPartiel(int start, int end)
		{
			if (start == 0)
			{
				rep = 0;
			}
			else
			{
				rep++;
			}
			if (rep > 100)
			{
				logger.Debug("BatirPartiel() : " + start + ", " + end);
				PrintMenu();
				return true;
			}
			int num = 0;
			bool flag = false;
			for (num = start; num < end; num++)
			{
				if (flag)
				{
					break;
				}
				switch (Op(num))
				{
				case "Sel":
				case "Hover":
				case "If":
				{
					int num2 = num;
					while (num2 >= 0)
					{
						int nextAt = instructions[num2].nextAt;
						if (instructions[num2].args.Length == 0 || Test(Op(num), instructions[num2].args[0]))
						{
							if (BatirPartiel(num2 + 1, (nextAt < 0) ? instructions[num2].endAt : nextAt))
							{
								flag = true;
							}
							break;
						}
						num2 = nextAt;
					}
					if (!flag)
					{
						num = instructions[num].endAt;
						if (num <= start)
						{
							num = end;
						}
					}
					break;
				}
				case "Submenu":
				{
					Bouton bouton = PieContenu.boutons[instructions[num].args[0]];
					bouton.submenu = new Classeur<Bouton>();
					boutons.Add(bouton);
					Classeur<Bouton> classeur = boutons;
					boutons = bouton.submenu;
					BatirPartiel(num + 1, instructions[num].endAt);
					num = instructions[num].endAt;
					if (num <= start)
					{
						num = end;
					}
					boutons = classeur;
					break;
				}
				case "File":
				{
					string emplacement = "Info/Livre/" + instructions[num].args[0].Replace(".", "/") + ".txt";
					Menu value = menus["Start"];
					instructions = new Classeur<Instruction>();
					depth = 0;
					Livre.Lire("PieMenu_Menus", emplacement);
					Livre.Compile("PieMenu_Menus", emplacement);
					Menu menu = menus["Start"];
					menu.objet = objet;
					menu.sujets = sujets;
					menu.Batir();
					boutons = menu.boutons;
					titre = menu.titre;
					menus["Start"] = value;
					flag = true;
					break;
				}
				case "Menu":
				{
					Menu menu = menus[instructions[num].args[0]];
					menu.objet = objet;
					menu.sujets = sujets;
					menu.Batir();
					boutons = menu.boutons;
					titre = menu.titre;
					flag = true;
					break;
				}
				case "Done":
					flag = true;
					break;
				default:
					instructions[num].Modifier(this);
					break;
				case "Else":
				case "End":
					break;
				}
			}
			return flag ? true : false;
		}

		public void Batir()
		{
			if (depth != -3627)
			{
				FinirDeCompiler();
				depth = -3627;
			}
			boutons.Clear();
			BatirPartiel(0, instructions.Count);
		}
	}

	public class Instruction
	{
		public string entete;

		public string[] args;

		public int ligne;

		public int depth;

		public int nextAt = -1;

		public int endAt = -1;

		public void Modifier(Menu menu)
		{
			switch (entete)
			{
			case "Title":
				if (args.Length > 0)
				{
					menu.titre = args[0];
				}
				break;
			case "Button":
				if (args.Length > 0 && boutons.ContainsKey(args[0]))
				{
					menu.boutons.Add(boutons[args[0]]);
				}
				else
				{
					menu.boutons.Add(boutonAbsent);
				}
				break;
			case "ButtonOff":
				if (args.Length > 0 && boutons.ContainsKey(args[0]))
				{
					menu.boutons.Add(boutons[args[0]]);
					menu.boutons[menu.boutons.Count - 1].check = () => false;
				}
				else
				{
					menu.boutons.Add(boutonAbsent);
				}
				break;
			case "Hole":
				menu.boutons.Add(boutonAbsent);
				break;
			case "Select Hovered":
				if (menu.objet is GameObject)
				{
					menu.sujets = new MonoBehaviour[1] { (menu.objet as GameObject).GetComponent<UISelectable>() };
				}
				else
				{
					menu.sujets = new MonoBehaviour[0];
				}
				Instructions.sujets = menu.sujets;
				break;
			case "Debug":
				logger.Info(args[0]);
				break;
			default:
				logger.Warn("Unknown instruction: \"" + entete + "\".");
				break;
			}
		}
	}

	public class Bouton
	{
		public string nom;

		public string titre;

		public Texture icone;

		public Texture iconeGris;

		public Sprite[] iconesToggle;

		public Sprite sprite;

		public Func<bool> check;

		public Action accept;

		public Action update;

		public Action onHover;

		public Action onToggle;

		public Func<int> checkToggled;

		public Func<string> checkLabel;

		public int size;

		public Color color = Color.white;

		public string tooltip;

		public bool sonNegatif;

		public Classeur<Bouton> submenu;

		public string testTerme;
	}

	public class MenuContenu
	{
		public static void CompilePhrase(Livre.Phrase phrase, PhraseCompileInfo compileInfo)
		{
			string entete = compileInfo.page.entete;
			if (phrase.entete.Contains("MENU"))
			{
				Menu menu = new Menu();
				menu.titre = (menu.nom = entete);
				menus[menu.nom] = menu;
				return;
			}
			try
			{
				Menu menu = menus[entete];
				Instruction instruction = new Instruction();
				instruction.entete = phrase.entete;
				instruction.args = phrase.terme;
				instruction.ligne = menu.instructions.Count;
				if (phrase.entete == "End" || phrase.entete == "Else")
				{
					menu.depth--;
				}
				instruction.depth = menu.depth;
				if (phrase.entete == "Sel" || phrase.entete == "Hover" || phrase.entete == "If" || phrase.entete == "Else" || phrase.entete == "Submenu")
				{
					menu.depth++;
				}
				menu.instructions.Add(instruction);
			}
			catch (Exception e)
			{
				logger.WarnFormat(e, "Menu parsing error: {0}/{1}.\n", entete, phrase.entete);
			}
		}
	}

	public class BoutonContenu
	{
		public static void CompilePhrase(Livre.Phrase phrase, PhraseCompileInfo compileInfo)
		{
			string entete = compileInfo.page.entete;
			if (phrase.entete.Contains("BUTTON"))
			{
				Bouton bouton = new Bouton();
				bouton.titre = (bouton.nom = entete);
				bouton.testTerme = string.Empty;
				boutons[bouton.nom] = bouton;
				return;
			}
			try
			{
				Bouton bouton = boutons[entete];
				switch (phrase.entete)
				{
				case "Name":
					if (phrase.terme.Length != 0)
					{
						bouton.titre = phrase.terme[0];
					}
					break;
				case "Effect":
					bouton.accept = delegate
					{
						try
						{
							Instructions.Execute(phrase.terme[0]);
						}
						catch (Exception e3)
						{
							logger.ErrorFormat(e3, "PieContenu.Effect failed: '{0}'. ", phrase.terme[0]);
						}
					};
					break;
				case "Action":
					bouton.accept = delegate
					{
						try
						{
							Instructions.Act(phrase.terme[0]);
						}
						catch (Exception e3)
						{
							logger.ErrorFormat(e3, "PieContenu.Accept failed: '{0}'. ", phrase.terme[0]);
						}
					};
					break;
				case "Toggle":
				{
					bouton.onToggle = delegate
					{
						try
						{
							Instructions.Execute(phrase.terme[0]);
						}
						catch (Exception e3)
						{
							logger.ErrorFormat(e3, "PieContenu.OnToggle failed: '{0}'. ", phrase.terme[0]);
						}
					};
					if (phrase.terme.Length > 1)
					{
						bouton.checkToggled = delegate
						{
							try
							{
								object obj = Instructions.Evaluate(phrase.terme[1]);
								if (!(obj is int result))
								{
									if (obj is bool)
									{
										return ((bool)obj) ? 1 : 0;
									}
									if (obj != null)
									{
										return 1;
									}
									return 0;
								}
								return result;
							}
							catch (Exception e3)
							{
								logger.ErrorFormat(e3, "PieContenu.CheckToggled failed: '{0}'. ", phrase.terme[1]);
								return 0;
							}
						};
					}
					Sprite[] array = new Sprite[3];
					for (int num = 2; num < phrase.terme.Length; num++)
					{
						MetaIcon metaIcon = IconDB._Unsafe_GetIcon_NEVERUSE(null, phrase.terme[num]);
						if (metaIcon != null)
						{
							if (metaIcon.Image64 != null)
							{
								array[num - 2] = metaIcon.Image64;
							}
							else
							{
								array[num - 2] = metaIcon.Image32;
							}
						}
					}
					bouton.iconesToggle = array;
					break;
				}
				case "Hover":
					bouton.onHover = delegate
					{
						try
						{
							Instructions.Execute(phrase.terme[0]);
						}
						catch (Exception e3)
						{
							logger.ErrorFormat(e3, "PieContenu.Hover failed: '{0}'. ", phrase.terme[0]);
						}
					};
					break;
				case "Update":
					bouton.update = delegate
					{
						try
						{
							Instructions.Execute(phrase.terme[0]);
						}
						catch (Exception e3)
						{
							logger.ErrorFormat(e3, "PieContenu.Update failed: '{0}'. ", phrase.terme[0]);
						}
					};
					break;
				case "Condition":
					bouton.testTerme = phrase.terme[0];
					bouton.check = delegate
					{
						try
						{
							return Convert.ToBoolean(Instructions.Evaluate(phrase.terme[0]));
						}
						catch (Exception e3)
						{
							logger.ErrorFormat(e3, "PieContenu.Check failed: '{0}'. ", phrase.terme[0]);
							return false;
						}
					};
					break;
				case "Label":
					bouton.checkLabel = delegate
					{
						try
						{
							return Convert.ToString(Instructions.Evaluate(phrase.terme[0]));
						}
						catch (Exception e3)
						{
							logger.ErrorFormat(e3, "PieContenu.Label failed: '{0}'. ", phrase.terme[0]);
							return "!!";
						}
					};
					break;
				case "Icon":
				{
					if (phrase.terme.Length == 0)
					{
						bouton.icone = (Texture)Resources.Load("Textures/UI/GroupIconEmpty");
						break;
					}
					string name = phrase.terme[0].Replace("."[0], "/"[0]);
					MetaIcon metaIcon2 = IconDB._Unsafe_GetIcon_NEVERUSE(null, name);
					if (metaIcon2 != null)
					{
						if (metaIcon2.Image64 != null)
						{
							bouton.sprite = metaIcon2.Image64;
						}
						else
						{
							bouton.sprite = metaIcon2.Image32;
						}
					}
					break;
				}
				case "Size":
					if (phrase.terme.Length != 0)
					{
						if (phrase.terme[0] == "Native")
						{
							bouton.size = -1;
						}
						else
						{
							bouton.size = int.Parse(phrase.terme[0]);
						}
					}
					break;
				case "Color":
					if (phrase.terme.Length == 0)
					{
						break;
					}
					try
					{
						int num2 = int.Parse(phrase.terme[0], NumberStyles.HexNumber);
						if (phrase.terme[0].Length == 8)
						{
							bouton.color = new Color32((byte)(num2 >> 24), (byte)(num2 >> 16), (byte)(num2 >> 8), (byte)num2);
						}
						else if (phrase.terme[0].Length == 6)
						{
							bouton.color = new Color32((byte)(num2 >> 16), (byte)(num2 >> 8), (byte)num2, byte.MaxValue);
						}
						else
						{
							logger.Error("PieContenu.Color failed: '" + phrase.terme[0] + "'. ");
						}
						break;
					}
					catch (Exception e)
					{
						logger.ErrorFormat(e, "PieContenu.Color failed: '{0}'. ", phrase.terme[0]);
						break;
					}
				case "Tooltip":
					if (phrase.terme.Length != 0)
					{
						bouton.tooltip = phrase.terme[0];
					}
					break;
				case "Sound":
					if (phrase.terme.Length > 0 && phrase.terme[0] == "Negative")
					{
						bouton.sonNegatif = true;
					}
					break;
				}
			}
			catch (Exception e2)
			{
				logger.WarnFormat(e2, "Button parsing error: {0}/{1}.\n", entete, phrase.entete);
			}
		}
	}

	private static BrixLogger logger = new BrixLogger(typeof(PieContenu).FullName);

	private static Dictionary<string, Menu> menus = new Dictionary<string, Menu>();

	private static Dictionary<string, Bouton> boutons = new Dictionary<string, Bouton>();

	private static Bouton boutonAbsent = new Bouton();

	private static void GenerateButtonData(Menu contenu, Vector3 mouse)
	{
		Livre.Lire("PieMenu_Buttons");
		Livre.Compile("PieMenu_Buttons");
		Livre.Lire("PieMenu_Menus");
		Livre.Compile("PieMenu_Menus");
		contenu.boutons.Clear();
		contenu.objet = RadialLayoutGroup.pieMenuTarget;
		if (contenu.objet is XYZ)
		{
			contenu.objet = ((XYZ)contenu.objet).ToScalaire();
		}
		CollectionPool.Request<GameObject>(out List<GameObject> list);
		UIGameSelector.GetSelectedEverything(ref list);
		List<UISelectable> list2 = (from s in list.Slinq()
			select s.GetComponent<UISelectable>() into s
			where !string.IsNullOrEmpty(s.PieMenuTag)
			select s).ToPooledList();
		list.Dispose();
		contenu.sujets = list2.Cast<MonoBehaviour>().ToArray();
		list2.Dispose();
		Instructions.sujets = contenu.sujets;
		Instructions.objet = contenu.objet;
		Instructions.ray = Camera.main.ScreenPointToRay(mouse);
		contenu.Batir();
	}

	private static Hashtable GetButtonData(Bouton bouton)
	{
		Hashtable hashtable = new Hashtable();
		hashtable["titre"] = bouton.titre;
		hashtable["tooltip"] = bouton.tooltip;
		hashtable["icone"] = bouton.icone;
		hashtable["iconeGris"] = bouton.iconeGris;
		hashtable["iconesToggle"] = bouton.iconesToggle;
		hashtable["sprite"] = bouton.sprite;
		hashtable["size"] = bouton.size;
		hashtable["color"] = bouton.color;
		Action accept = bouton.accept;
		hashtable["accept"] = ((bouton.accept != null) ? accept : null);
		Action onToggle = bouton.onToggle;
		hashtable["onToggle"] = ((bouton.onToggle != null) ? onToggle : null);
		Func<bool> func = () => bouton.check == null || bouton.check();
		Func<bool> value = func;
		hashtable["check"] = value;
		Func<int> func2 = () => (bouton.checkToggled != null) ? bouton.checkToggled() : (-1);
		Func<int> value2 = func2;
		hashtable["checkToggled"] = value2;
		hashtable["checkLabel"] = ((bouton.checkLabel != null) ? bouton.checkLabel() : string.Empty);
		hashtable["sonNegatif"] = bouton.sonNegatif;
		if (bouton.submenu != null)
		{
			List<Hashtable> list = new List<Hashtable>();
			Hashtable hashtable2 = new Hashtable();
			hashtable2["titre"] = bouton.titre;
			list.Add(hashtable2);
			foreach (Bouton item in bouton.submenu)
			{
				list.Add(GetButtonData(item));
			}
			hashtable["submenu"] = list;
		}
		hashtable["testTerme"] = bouton.testTerme;
		return hashtable;
	}

	public static List<Hashtable> GetGeneratedButtonData(string mode, Vector3 mouse)
	{
		Menu value = null;
		if (menus.TryGetValue(mode, out value))
		{
			GenerateButtonData(value, mouse);
		}
		List<Hashtable> list = new List<Hashtable>();
		Hashtable hashtable = new Hashtable();
		hashtable["titre"] = value.titre;
		list.Add(hashtable);
		foreach (Bouton bouton in value.boutons)
		{
			list.Add(GetButtonData(bouton));
		}
		return list;
	}

	public static void SetCurrentTooltip(string tooltip)
	{
	}
}
