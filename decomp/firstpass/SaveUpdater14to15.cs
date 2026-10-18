using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Brix.Assets;
using Brix.IO.Saves;
using Brix.Utils.Logging;
using Newtonsoft.Json.Linq;

public class SaveUpdater14to15 : SaveUpdater
{
	private static BrixLogger logger = new BrixLogger(typeof(SaveUpdater14to15).FullName);

	private List<string> projects;

	private List<string> Projects
	{
		get
		{
			if (projects == null)
			{
				projects = new List<string> { "Brix.Game.AI.BuildProject, Assembly-CSharp-firstpass", "Brix.Game.AI.CleanupProject, Assembly-CSharp-firstpass", "Brix.Game.AI.IdleProject, Assembly-CSharp-firstpass", "Brix.Game.AI.LandscapeProject, Assembly-CSharp-firstpass", "Brix.Game.AI.LumberProject, Assembly-CSharp-firstpass", "Brix.Game.AI.MineProject, Assembly-CSharp-firstpass", "Brix.Game.AI.PhaseProject, Assembly-CSharp-firstpass", "Brix.Game.AI.RepairProject, Assembly-CSharp-firstpass", "TunnelProject, Assembly-CSharp-firstpass", "Brix.Game.AI.RaidProject, Assembly-CSharp-firstpass" };
			}
			return projects;
		}
	}

	public SaveUpdater14to15(Asset_Map save)
	{
		saveFile = save;
	}

	public override void Init()
	{
		logger.Info("Trying to load Saveupdater for: " + saveFile.path);
		dataFiles = new List<FileUpdater>();
		FileUpdater fileUpdater = AddFile("gameobjects.json", isArray: true);
		fileUpdater.AddStep(ParentBricktronsToProject);
		fileUpdater.AddStep(RemoveWorkforces);
		FileUpdater fileUpdater2 = AddFile("meta.json", isArray: false);
		fileUpdater2.AddStep(FixVersion);
	}

	private IEnumerable<JComponentFetcher> ProjectFetchers()
	{
		foreach (string p in Projects)
		{
			yield return JComponentFetcher.New().Type(p).Make();
		}
	}

	private JContainer RemoveWorkforces(JContainer json, IdGenerator idGen)
	{
		foreach (JComponentFetcher item in ProjectFetchers())
		{
			JObjectModifier jObjectModifier = JObjectModifier.New().RemoveField("workForce").Make();
			json = jObjectModifier.Apply(item, json);
		}
		return json;
	}

	public override bool IsUpdatable()
	{
		string json = File.ReadAllText(Path.Combine(saveFile.path, "meta.json"));
		JObject jObject = JObject.Parse(json);
		int num = jObject["__ASSETVERSION"].Value<int>();
		return num == 14;
	}

	private string GetProjectPath(JContainer project)
	{
		string text = project.Value<string>("name");
		string text2 = project.Value<string>("$path");
		return text2 + "/" + text;
	}

	private void GetProjectComponents()
	{
	}

	private JContainer ParentBricktronsToProject(JContainer json, IdGenerator idGen)
	{
		JGameObjectFetcher jGameObjectFetcher = JGameObjectFetcher.New().FieldEqualsTo("$path", "/Jeu/Groupes").Make();
		List<JContainer> list = jGameObjectFetcher.Fetch(json).ToList();
		foreach (JContainer item in list)
		{
			string projectPath = GetProjectPath(item);
			string text = item.Value<string>("$id");
			List<string> crew = GetCrew(item);
			foreach (string item2 in crew)
			{
				try
				{
					json = ReparentTo(json, item2, projectPath, text);
				}
				catch (Exception message)
				{
					logger.Error(message);
					string message2 = "Project was " + text + "/nPath was " + projectPath + "/nLabor was " + item2;
					logger.Error(message2);
				}
			}
		}
		return json;
	}

	private JContainer ReparentTo(JContainer json, string laborID, string path, string parent)
	{
		JGameObjectFetcher jGameObjectFetcher = JGameObjectFetcher.New().Id(laborID).Make();
		JGameObjectModifier jGameObjectModifier = JGameObjectModifier.New().Path(path).Make();
		JGameObjectFetcher injectionFetcher = JGameObjectFetcher.New().Id(parent).Make();
		JGameObjectModifier jGameObjectModifier2 = JGameObjectModifier.New().InjectJGameObject("$parent", injectionFetcher).Make();
		JContainer jContainer = jGameObjectFetcher.Fetch(json).First();
		logger.Info("Reparenting Labor :" + jContainer.Value<string>("name") + " To Project :" + path);
		json = jGameObjectModifier.Apply(jGameObjectFetcher, json);
		json = jGameObjectModifier2.Apply(jGameObjectFetcher, json);
		return json;
	}

	private JContainer GetProject(JContainer components, string name)
	{
		foreach (JContainer item2 in (IEnumerable<JToken>)components)
		{
			string item = item2.Value<string>("$type");
			if (Projects.Contains(item))
			{
				return item2;
			}
		}
		logger.Error("Could not find project in " + name);
		throw new Exception("Should be a project component in a project");
	}

	private List<string> GetCrew(JContainer projectGO)
	{
		JContainer components = projectGO.Value<JContainer>("components");
		JContainer project = GetProject(components, projectGO.Value<string>("name"));
		JContainer jContainer = project.Value<JContainer>("workForce");
		JContainer jContainer2 = jContainer.Value<JContainer>("crew");
		List<string> list = new List<string>();
		foreach (JToken item in (IEnumerable<JToken>)jContainer2)
		{
			string text = item.Value<string>("$ref");
			if (text != null)
			{
				list.Add(text);
				continue;
			}
			string text2 = item.Value<string>("$id");
			if (text2 != null)
			{
				list.Add(text2);
			}
		}
		return list;
	}

	private JContainer FixVersion(JContainer inputJson, IdGenerator idGen)
	{
		JObjectFetcher fetcher = JObjectFetcher.New().FieldEqualsTo("__ASSETVERSION", 14).Make();
		JObjectModifier jObjectModifier = JObjectModifier.New().ChangeFieldValue("__ASSETVERSION", 15).Make();
		logger.Info("Updated version file sucessfully");
		return jObjectModifier.Apply(fetcher, inputJson);
	}
}
