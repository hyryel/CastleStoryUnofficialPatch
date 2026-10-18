using System.Collections.Generic;
using System.Linq;

public class CombinedSaveUpdater : SaveUpdater
{
	private List<SaveUpdater> updaters;

	public CombinedSaveUpdater(List<SaveUpdater> _updaters = null)
	{
		saveFile = _updaters.Select((SaveUpdater x) => x.saveFile).First();
		if (_updaters == null)
		{
			updaters = new List<SaveUpdater>();
		}
		else
		{
			updaters = _updaters;
		}
	}

	public void AddUpdaters(SaveUpdater su)
	{
		updaters.Add(su);
	}

	public override void Init()
	{
		dataFiles = new List<FileUpdater>();
		foreach (SaveUpdater updater in updaters)
		{
			updater.Init();
			foreach (FileUpdater fu in updater.dataFiles)
			{
				FileUpdater fileUpdater = dataFiles.FirstOrDefault((FileUpdater x) => x.name == fu.name);
				if (fileUpdater == null)
				{
					dataFiles.Add(fu);
				}
				else
				{
					fileUpdater.AddSteps(fu.GetSteps());
				}
			}
		}
	}

	public override bool IsUpdatable()
	{
		return updaters.First().IsUpdatable();
	}
}
