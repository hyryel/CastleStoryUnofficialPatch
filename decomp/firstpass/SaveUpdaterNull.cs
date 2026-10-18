using Brix.Assets;
using Brix.Utils.Logging;

public class SaveUpdaterNull : SaveUpdater
{
	private static BrixLogger logger = new BrixLogger(typeof(SaveUpdaterNull).FullName);

	public SaveUpdaterNull(Asset_Map save)
	{
		logger.Info("Returned a null updater object for: " + save.path);
		saveFile = save;
	}

	public override void UpdateSave()
	{
	}

	public override void Write(string _basePath)
	{
		logger.Info("Nothing to update, no changes made to save file");
	}

	public override bool IsUpdatable()
	{
		return false;
	}

	public override int StepCount()
	{
		return 0;
	}

	public override void Init()
	{
	}

	public override string CreateBackUp()
	{
		return null;
	}
}
