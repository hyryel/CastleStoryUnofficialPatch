using System.Collections.Generic;
using Brix.Utils;
using UnityEngine;

namespace Brix.UI.GameSelector;

public class UIGameSelectorConfig : MonoBehaviour
{
	[SerializeField]
	private List<SelectionTrack> tracks;

	[SerializeField]
	private List<SelectionConfig> configs;

	[SerializeField]
	private List<SelectionExclusivity> exclusivities;

	private Dictionary<SelectionTrackID, SelectionTrack> compiledTracks = new Dictionary<SelectionTrackID, SelectionTrack>();

	private Dictionary<SelectionType, SelectionConfig> compiledConfigs = new Dictionary<SelectionType, SelectionConfig>();

	private Dictionary<SelectionType, List<SelectionType>> compiledExclusivities = new Dictionary<SelectionType, List<SelectionType>>();

	private void CompileTracks()
	{
		foreach (SelectionTrack track in tracks)
		{
			compiledTracks.Add(track.track, track);
		}
	}

	private void CompileConfig()
	{
		foreach (SelectionConfig config in configs)
		{
			compiledConfigs.Add(config.type, config);
		}
	}

	private void CompileExclusions()
	{
		foreach (SelectionExclusivity exclusivity in exclusivities)
		{
			foreach (SelectionType type in exclusivity.types)
			{
				List<SelectionType> list = new List<SelectionType>(exclusivity.types);
				list.Remove(type);
				compiledExclusivities.Add(type, list);
			}
		}
	}

	private void Start()
	{
		CompileTracks();
		CompileConfig();
		CompileExclusions();
	}

	public List<SelectionType> TypesIn(SelectionTrackID track)
	{
		return compiledTracks[track].types;
	}

	public List<SelectionType> TypesExclusiveTo(SelectionType t)
	{
		return compiledExclusivities.GetValueOrCreate(t, Empty.List<SelectionType>());
	}

	public bool IsIndependent(SelectionType type)
	{
		if (!compiledConfigs.ContainsKey(type))
		{
			return false;
		}
		return compiledConfigs[type].IsIndepedant;
	}
}
