using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Brix.Core;
using Brix.Util;
using Brix.Utils.Logging;
using Newtonsoft.Json;
using UnityEngine;

namespace Brix.Engine.Abstract;

public class Neo : MonoBehaviour, INeo
{
	public class ProcessInfo
	{
		public Process _process;

		private Action<string> outputCallback;

		public StreamWriter StandardInput => _process.StandardInput;

		public StreamReader StandardOutput => _process.StandardOutput;

		public StreamReader StandardError => _process.StandardError;

		public int ExitCode => _process.ExitCode;

		public Action WaitForExit => _process.WaitForExit;

		private void HandleOutput(object sendingProcess, DataReceivedEventArgs outLine)
		{
			outputCallback(outLine.Data);
		}

		public ProcessInfo RedirectOutput(Action<string> callback)
		{
			_process.StartInfo.RedirectStandardOutput = true;
			_process.StartInfo.RedirectStandardError = false;
			_process.OutputDataReceived += HandleOutput;
			outputCallback = callback;
			return this;
		}

		public ProcessInfo RedirectError(Action<string> callback)
		{
			_process.StartInfo.RedirectStandardError = true;
			_process.StartInfo.RedirectStandardOutput = false;
			_process.ErrorDataReceived += HandleOutput;
			outputCallback = callback;
			return this;
		}

		public ProcessInfo Start()
		{
			_process.Start();
			if (_process.StartInfo.RedirectStandardOutput)
			{
				_process.BeginOutputReadLine();
			}
			if (_process.StartInfo.RedirectStandardError)
			{
				_process.BeginErrorReadLine();
			}
			return this;
		}
	}

	public class Profilage
	{
		public string nom;

		public float duree;

		public int appels;

		public Hashtable prop = new Hashtable();

		public Profilage(string s)
		{
			nom = s;
			duree = 0f;
			appels = 0;
		}
	}

	private static BrixLogger logger = new BrixLogger();

	public static bool profilageActif = true;

	public static Dictionary<string, Profilage> profilages = new Dictionary<string, Profilage>();

	private static Classeur<Profilage> profilStack = new Classeur<Profilage>();

	private static Classeur<float> profilDebutStack = new Classeur<float>();

	public static IEnumerator AppelDynamique(Action f, float d)
	{
		yield return new WaitForSeconds(d);
		f();
	}

	public static IEnumerator AppelDynamique(Action f)
	{
		yield return new WaitForEndOfFrame();
		f();
	}

	public static IEnumerator EnumAll(params IEnumerator[] enumerators)
	{
		foreach (IEnumerator ie in enumerators)
		{
			IEnumerator all = EnumAll(ie);
			while (all.MoveNext())
			{
				yield return all.Current;
			}
		}
	}

	public static IEnumerator EnumAll(IEnumerator enumerator)
	{
		while (enumerator.MoveNext())
		{
			if (enumerator.Current is IEnumerator recur)
			{
				IEnumerator recur2 = EnumAll(recur);
				while (recur2.MoveNext())
				{
					yield return recur2.Current;
				}
			}
			else
			{
				yield return enumerator.Current;
			}
		}
	}

	public static IEnumerator TryEnum(IEnumerator e, Action<Exception> handleError, bool stopOnError = true)
	{
		while (true)
		{
			try
			{
				if (!e.MoveNext())
				{
					break;
				}
			}
			catch (Exception ex)
			{
				Exception ex2 = ex;
				handleError(ex2);
				if (stopOnError)
				{
					break;
				}
			}
			yield return e.Current;
		}
	}

	public static void DoAll(IEnumerator enumerator)
	{
		while (enumerator.MoveNext())
		{
			if (enumerator.Current is IEnumerator)
			{
				DoAll(enumerator.Current as IEnumerator);
			}
		}
	}

	public static IEnumerator DoNothing()
	{
		yield break;
	}

	public static object GetFieldOrProperty(object obj, string name)
	{
		Type type = obj.GetType();
		FieldInfo field = type.GetField(name);
		if (field != null)
		{
			return field.GetValue(obj);
		}
		PropertyInfo property = type.GetProperty(name);
		return ((property != null) ? property.GetGetMethod() : type.GetMethod(name))?.Invoke(obj, new object[0]);
	}

	public static void ExecuteProcess(string cmd, params string[] args)
	{
		CreateProcess(cmd, args).Start();
	}

	public static ProcessInfo CreateProcess(string cmd, params string[] args)
	{
		Process process = new Process();
		process.StartInfo.FileName = cmd;
		process.StartInfo.Arguments = string.Join(" ", args);
		process.StartInfo.CreateNoWindow = true;
		process.StartInfo.UseShellExecute = false;
		return new ProcessInfo
		{
			_process = process
		};
	}

	private static void ClearProfilage()
	{
		profilages.Clear();
		profilStack.Clear();
		profilDebutStack.Clear();
	}

	private static Profilage GetProfil(string s)
	{
		if (!profilages.TryGetValue(s, out var value))
		{
			value = new Profilage(s);
			profilages.Add(s, value);
		}
		return value;
	}

	public static object ConfigProfilage(string s, string prop)
	{
		if (!profilageActif)
		{
			return null;
		}
		Profilage profil = GetProfil(s);
		return profil.prop[prop];
	}

	public static object ConfigProfilage(string s, string prop, object val)
	{
		if (!profilageActif)
		{
			return null;
		}
		Profilage profil = GetProfil(s);
		profil.prop[prop] = val;
		return val;
	}

	public static void AccumulerProfilage(string s, int nAppels, float dureeTotale)
	{
		if (profilageActif)
		{
			Profilage profil = GetProfil(s);
			profil.duree += dureeTotale;
			profil.appels += nAppels;
		}
	}

	public static void BeginSample(string s)
	{
		if (profilageActif)
		{
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			Profilage profil = GetProfil(s);
			profilStack.Push(profil);
			profilDebutStack.Push(realtimeSinceStartup);
		}
	}

	public static void EndSample()
	{
		if (profilageActif && profilStack.Count != 0)
		{
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			Profilage profilage = profilStack[profilStack.Count - 1];
			float num = profilDebutStack[profilDebutStack.Count - 1];
			profilage.duree += realtimeSinceStartup - num;
			profilage.appels++;
			profilStack.Pop();
			profilDebutStack.Pop();
		}
	}

	public static T DeserializeObject<T>(string text)
	{
		try
		{
			return JsonConvert.DeserializeObject<T>(text);
		}
		catch (Exception ex)
		{
			Donnees.LogWarning("Deserialization failed: " + ((text == null) ? "null" : ("'" + ((text.Length < 10) ? text : (text.Substring(0, 10) + "...")) + "'")) + "\n" + ex);
		}
		return default(T);
	}

	public static byte[] ReadAllBytes(string path)
	{
		Fichier fichier = Fichier(path);
		if (fichier.Exists())
		{
			return fichier.ReadAllBytes();
		}
		return new byte[0];
	}

	public static string ReadAllText(string path)
	{
		Fichier fichier = Fichier(path);
		if (fichier.Exists())
		{
			return fichier.ReadAllText();
		}
		return "";
	}

	public static void WriteAllBytes(string path, byte[] bytes)
	{
		Fichier(path).WriteAllBytes(bytes);
	}

	public static void WriteAllText(string path, string text)
	{
		Fichier(path).WriteAllText(text);
	}

	public static bool FileExists(string path)
	{
		return Fichier(path).Exists();
	}

	public static IEnumerator ReadAllBytesAsync(string path, Action<byte[]> callback)
	{
		yield return FichierAsync(path, delegate(Fichier fichier)
		{
			callback(fichier.ReadAllBytes());
		}, delegate
		{
		});
	}

	public static IEnumerator ReadAllBytesAsync(string path, Action<byte[]> callback, Action fail)
	{
		yield return FichierAsync(path, delegate(Fichier fichier)
		{
			callback(fichier.ReadAllBytes());
		}, fail);
	}

	public static IEnumerator ReadAllTextAsync(string path, Action<string> callback)
	{
		yield return FichierAsync(path, delegate(Fichier fichier)
		{
			callback(fichier.ReadAllText());
		}, delegate
		{
		});
	}

	public static IEnumerator ReadAllTextAsync(string path, Action<string> callback, Action fail)
	{
		yield return FichierAsync(path, delegate(Fichier fichier)
		{
			callback(fichier.ReadAllText());
		}, fail);
	}

	public static IEnumerable<Fichier> FichiersMultiples(IEnumerable<string> paths)
	{
		return paths.Select((string path) => Fichier(path));
	}

	public static IEnumerator FichiersMultiplesAsync(IEnumerable<string> paths, Action<Dictionary<string, Fichier>> callback)
	{
		return FichiersMultiplesAsync(paths, callback, (string s) => false);
	}

	public static IEnumerator FichiersMultiplesAsync(IEnumerable<string> paths, Action<Dictionary<string, Fichier>> callback, Func<string, bool> fail)
	{
		return FichiersMultiplesAsync(paths, (string s) => s, callback, fail);
	}

	public static IEnumerator FichiersMultiplesAsync<T>(IEnumerable<T> keys, Func<T, string> makePath, Action<Dictionary<T, Fichier>> callback)
	{
		return FichiersMultiplesAsync(keys, makePath, callback, (T key) => false);
	}

	private static IEnumerable<IEnumerator> GetRequetesAsync<T>(IEnumerable<T> keys, Func<T, string> makePath, Action<T, Fichier> add, Action<T> fail)
	{
		return keys.Select((T key) => FichierAsync(makePath(key), delegate(Fichier f)
		{
			add(key, f);
		}, delegate
		{
			fail(key);
		}));
	}

	public static IEnumerator FichiersMultiplesAsync_Parallele<T>(IEnumerable<T> keys, Func<T, string> makePath, Action<Dictionary<T, Fichier>> callback, Func<T, bool> fail)
	{
		Dictionary<T, Fichier> results = new Dictionary<T, Fichier>();
		List<Coroutine> coroutines = new List<Coroutine>();
		EmptyComponent instance = new GameObject().AddComponent<EmptyComponent>();
		int fails = 0;
		bool stop = false;
		foreach (IEnumerator requete in GetRequetesAsync(keys, makePath, results.Add, delegate(T key)
		{
			fails++;
			if (fail(key))
			{
				stop = true;
			}
		}))
		{
			coroutines.Add(instance.StartCoroutine(requete));
		}
		while (results.Count + fails < coroutines.Count)
		{
			if (stop)
			{
				instance.StopAllCoroutines();
				yield break;
			}
			yield return new WaitForSeconds(0.1f);
		}
		UnityEngine.Object.Destroy(instance);
		callback(results);
	}

	public static IEnumerator FichiersMultiplesAsync_EnSerie<T>(IEnumerable<T> keys, Func<T, string> makePath, Action<Dictionary<T, Fichier>> callback, Func<T, bool> fail)
	{
		Dictionary<T, Fichier> resultats = new Dictionary<T, Fichier>();
		bool stop = false;
		foreach (IEnumerator item in GetRequetesAsync(keys, makePath, resultats.Add, delegate(T key)
		{
			stop = fail(key);
		}))
		{
			yield return item;
			if (stop)
			{
				yield break;
			}
		}
		callback(resultats);
	}

	public static Fichier Fichier(string path)
	{
		return Brix.Util.Fichier.FromFileInfo(new FileInfo(path));
	}

	public static IEnumerator FichierAsync(string path, Action<Fichier> callback, Action fail)
	{
		logger.Info("Neo.FichierAsync() : " + path);
		Fichier fichier = Brix.Util.Fichier.FromFileInfo(new FileInfo(path));
		if (fichier.Exists())
		{
			callback(fichier);
		}
		else
		{
			fail();
		}
		yield break;
	}

	public static IEnumerator FichiersMultiplesAsync<T>(IEnumerable<T> keys, Func<T, string> makePath, Action<Dictionary<T, Fichier>> callback, Func<T, bool> fail)
	{
		return FichiersMultiplesAsync_EnSerie(keys, makePath, callback, fail);
	}

	public static IEnumerator ReadArchiveAsync(string path, Action<Archive> callback, Action fail)
	{
		if (path.EndsWith(".unity3d") && File.Exists(path))
		{
			AssetBundle bundle = AssetBundle.LoadFromFile(path);
			Archive archive = Archive.FromBundle(bundle);
			callback(archive);
		}
		else if (Directory.Exists(path))
		{
			Archive archive2 = Archive.FromDirectory(path);
			callback(archive2);
		}
		else
		{
			fail();
		}
		yield break;
	}

	public static IEnumerator ReadArchiveAsync(string path, Action<Archive> callback)
	{
		return ReadArchiveAsync(path, callback, delegate
		{
		});
	}
}
