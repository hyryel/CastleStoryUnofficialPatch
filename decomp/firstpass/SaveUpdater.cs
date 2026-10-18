using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Brix.Assets;
using Brix.Utils.Logging;

public abstract class SaveUpdater
{
	private delegate SaveUpdater Construct(Asset_Map save);

	private static BrixLogger logger = new BrixLogger(typeof(SaveUpdater).FullName);

	private static List<Construct> updaters = new List<Construct>
	{
		(Asset_Map s) => new SaveUpdaterNull(s),
		(Asset_Map s) => new SaveUpdaterNull(s),
		(Asset_Map s) => new SaveUpdaterNull(s),
		(Asset_Map s) => new SaveUpdater3to4(s),
		(Asset_Map s) => new SaveUpdater4to5(s),
		(Asset_Map s) => new SaveUpdater5to6(s),
		(Asset_Map s) => new SaveUpdater6to7(s),
		(Asset_Map s) => new SaveUpdater7to8(s),
		(Asset_Map s) => new SaveUpdater8to9(s),
		(Asset_Map s) => new SaveUpdater9to10(s),
		(Asset_Map s) => new SaveUpdater10to11(s),
		(Asset_Map s) => new SaveUpdater11to12(s),
		(Asset_Map s) => new SaveUpdater12to13(s),
		(Asset_Map s) => new SaveUpdater13to14(s),
		(Asset_Map s) => new SaveUpdater14to15(s),
		(Asset_Map s) => new SaveUpdater15to16(s),
		(Asset_Map s) => new SaveUpdater16to17(s),
		(Asset_Map s) => new SaveUpdater17to18(s),
		(Asset_Map s) => new SaveUpdater18to19(s),
		(Asset_Map s) => new SaveUpdater19to20(s),
		(Asset_Map s) => new SaveUpdater20to21(s),
		(Asset_Map s) => new SaveUpdater21to22(s),
		(Asset_Map s) => new SaveUpdater22to23(s),
		(Asset_Map s) => new SaveUpdater23to24(s),
		(Asset_Map s) => new SaveUpdater24to25(s),
		(Asset_Map s) => new SaveUpdaterNull(s)
	};

	public List<FileUpdater> dataFiles = new List<FileUpdater>();

	public Asset_Map saveFile;

	protected bool backupCreated;

	private static bool UseGenId;

	public static bool IsUpdatable(Asset_Map save)
	{
		return GetInstance(save).IsUpdatable();
	}

	public static SaveUpdater GetInstance(Asset_Map save)
	{
		int version = save.version;
		int currentVersion = Asset_Base<Asset_Map>.GetCurrentVersion();
		if (currentVersion <= version)
		{
			return new SaveUpdaterNull(save);
		}
		List<SaveUpdater> list = new List<SaveUpdater>();
		for (int i = version; i < currentVersion; i++)
		{
			list.Add(updaters[i](save));
		}
		return new CombinedSaveUpdater(list);
	}

	public abstract void Init();

	public abstract bool IsUpdatable();

	public virtual void UpdateSave()
	{
		Init();
		foreach (FileUpdater dataFile in dataFiles)
		{
			dataFile.Reset();
			dataFile.RunSteps();
		}
	}

	public virtual void Write(string _basePath)
	{
		foreach (FileUpdater dataFile in dataFiles)
		{
			dataFile.Write(_basePath);
		}
		logger.Info("Wrote updated save files to disk with success");
	}

	public virtual int StepCount()
	{
		int num = 0;
		foreach (FileUpdater dataFile in dataFiles)
		{
			num += dataFile.StepCount();
		}
		return num;
	}

	public virtual string CreateBackUp()
	{
		string backUpPath = getBackUpPath();
		if (Directory.Exists(backUpPath))
		{
			Directory.Delete(backUpPath, recursive: true);
		}
		Directory.CreateDirectory(backUpPath);
		try
		{
			CopyFrom(saveFile.path, backUpPath);
			logger.Info("TempFolder Successfully Created at " + backUpPath);
			return backUpPath;
		}
		catch
		{
			logger.Error("Could Not Copy " + saveFile.path + " to " + backUpPath);
			backupCreated = false;
			throw;
		}
	}

	private static void CopyFrom(string srcFolder, string dstFolder)
	{
		if (!Directory.Exists(dstFolder))
		{
			Directory.CreateDirectory(dstFolder);
		}
		string[] files = Directory.GetFiles(srcFolder);
		foreach (string text in files)
		{
			File.Copy(text, Path.Combine(dstFolder, Path.GetFileName(text)));
		}
		string[] directories = Directory.GetDirectories(srcFolder);
		foreach (string text2 in directories)
		{
			string text3 = Path.Combine(dstFolder, new DirectoryInfo(text2).Name);
			if (!Directory.Exists(text3))
			{
				Directory.CreateDirectory(text3);
			}
			CopyFrom(text2, text3);
		}
	}

	protected FileUpdater AddFile(string name, bool isArray)
	{
		if (saveFile.version > 7)
		{
			UseGenId = false;
		}
		else
		{
			UseGenId = true;
		}
		string original = File.ReadAllText(Path.Combine(saveFile.path, name));
		FileUpdater fileUpdater = new FileUpdater(name, isArray, original, UseGenId);
		dataFiles.Add(fileUpdater);
		return fileUpdater;
	}

	private string getBackUpPath()
	{
		string text = saveFile.path.Split(new string[2] { "/", "\\" }, StringSplitOptions.RemoveEmptyEntries).Last();
		return Path.Combine(saveFile.path, "../BackUp-" + text);
	}
}
