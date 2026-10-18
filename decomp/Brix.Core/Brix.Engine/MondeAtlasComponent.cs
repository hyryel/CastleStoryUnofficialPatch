using System.Collections.Generic;
using UnityEngine;

namespace Brix.Engine;

[ExecuteInEditMode]
public class MondeAtlasComponent : MonoBehaviour
{
	internal static Dictionary<string, MondeAtlas> dicoAtlas = new Dictionary<string, MondeAtlas>();

	public MondeAtlas atlas = new MondeAtlas();

	private void Start()
	{
		Init();
	}

	private void OnEnable()
	{
		Init();
	}

	private void Awake()
	{
		Init();
	}

	private void OnDestroy()
	{
		Clear();
	}

	[ContextMenu("Init")]
	public void Init()
	{
		dicoAtlas[atlas.name] = atlas;
	}

	[ContextMenu("Clear")]
	public void Clear()
	{
		dicoAtlas[atlas.name].Clear();
	}
}
