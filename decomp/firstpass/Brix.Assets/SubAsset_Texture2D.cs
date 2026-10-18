using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Brix.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Brix.Assets;

public class SubAsset_Texture2D
{
	private struct ItemToLoad
	{
		public Action<Texture2D> callback;

		public string path;

		public bool clearOnSceneChange;
	}

	private struct ItemLoaded
	{
		public long timestamp;

		public Texture2D texture;

		public bool clearOnSceneChange;
	}

	private static Dictionary<string, ItemLoaded> _loadedAssets;

	private static Queue<ItemToLoad> _loadingQueue;

	private static IEnumerator coroutine;

	private static readonly WaitForEndOfFrame TmpWaitForEndOfFrame;

	static SubAsset_Texture2D()
	{
		_loadedAssets = new Dictionary<string, ItemLoaded>(32);
		_loadingQueue = new Queue<ItemToLoad>(32);
		coroutine = null;
		TmpWaitForEndOfFrame = new WaitForEndOfFrame();
		SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
	}

	private static void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
	{
		_loadingQueue.Clear();
		coroutine = null;
		List<string> list = null;
		foreach (KeyValuePair<string, ItemLoaded> loadedAsset in _loadedAssets)
		{
			if (loadedAsset.Value.clearOnSceneChange)
			{
				UnityEngine.Object.Destroy(loadedAsset.Value.texture);
				if (list == null)
				{
					list = CollectionPool.RequestList<string>();
				}
				list.Add(loadedAsset.Key);
			}
		}
		if (list == null)
		{
			return;
		}
		foreach (string item in list)
		{
			_loadedAssets.Remove(item);
		}
		list.Dispose();
	}

	public static bool IsToLoadOrUpdate(string path)
	{
		if (File.Exists(path))
		{
			if (_loadedAssets.TryGetValue(path, out var value))
			{
				return File.GetLastWriteTimeUtc(path).ToFileTimeUtc() > value.timestamp;
			}
			return true;
		}
		return false;
	}

	public static Texture2D GetAssetSync(string path, bool clearOnSceneChanged = false)
	{
		Texture2D texture2D = null;
		ItemLoaded value2;
		if (IsToLoadOrUpdate(path))
		{
			texture2D = new Texture2D(0, 0);
			texture2D.LoadImage(File.ReadAllBytes(path));
			if (_loadedAssets.TryGetValue(path, out var value))
			{
				UnityEngine.Object.Destroy(value.texture);
			}
			_loadedAssets[path] = new ItemLoaded
			{
				timestamp = File.GetLastWriteTimeUtc(path).ToFileTimeUtc(),
				texture = texture2D,
				clearOnSceneChange = clearOnSceneChanged
			};
		}
		else if (_loadedAssets.TryGetValue(path, out value2))
		{
			texture2D = value2.texture;
		}
		return texture2D;
	}

	public static void GetAssetAsync(string path, Action<Texture2D, Action<Texture2D>, Asset_Map> onLoaded, Action<Texture2D> onLoaded2, Asset_Map assetMap, bool clearOnSceneChanged = false)
	{
		if (!IsToLoadOrUpdate(path) && _loadedAssets.TryGetValue(path, out var value))
		{
			onLoaded(value.texture, onLoaded2, assetMap);
			return;
		}
		_loadingQueue.Enqueue(new ItemToLoad
		{
			path = path,
			callback = onLoaded2,
			clearOnSceneChange = clearOnSceneChanged
		});
		if (coroutine == null)
		{
			coroutine = AsyncEnumerator();
			PersistentCoroutineRunner.Instance.StartCoroutine(coroutine);
		}
	}

	private static IEnumerator AsyncEnumerator()
	{
		yield return TmpWaitForEndOfFrame;
		while (_loadingQueue.Count != 0)
		{
			ItemToLoad itl = _loadingQueue.Dequeue();
			itl.callback(GetAssetSync(itl.path, itl.clearOnSceneChange));
			yield return TmpWaitForEndOfFrame;
		}
		coroutine = null;
	}
}
