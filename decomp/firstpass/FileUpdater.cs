using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

public class FileUpdater
{
	public delegate JContainer updateJson(JContainer inputJson, IdGenerator idGen);

	public string name;

	private bool isArray;

	public JContainer original;

	public JContainer updated;

	private List<updateJson> steps;

	private IdGenerator idGen;

	public FileUpdater(string _name, bool _isArray, string _original, bool _useGenId)
	{
		name = _name;
		isArray = _isArray;
		if (_isArray)
		{
			original = JToken.Parse(_original) as JContainer;
		}
		else
		{
			original = JToken.Parse(UpdateUtils.EncloseInSqrBracket(_original)) as JContainer;
		}
		steps = new List<updateJson>();
		if (_useGenId)
		{
			idGen = new IdGenerator(original.ToString());
		}
		Reset();
	}

	public IdGenerator GetIdGen()
	{
		return idGen;
	}

	public int StepCount()
	{
		return steps.Count;
	}

	public void Reset()
	{
		updated = original.DeepClone() as JContainer;
	}

	public void AddStep(updateJson func)
	{
		steps.Add(func);
	}

	public void Write(string basePath)
	{
		if (isArray)
		{
			File.WriteAllText(Path.Combine(basePath, name), updated.ToString());
		}
		else
		{
			File.WriteAllText(Path.Combine(basePath, name), UpdateUtils.RemoveSquareBrackets(updated.ToString()));
		}
	}

	public void AddSteps(List<updateJson> _steps)
	{
		steps.AddRange(_steps);
	}

	public List<updateJson> GetSteps()
	{
		return steps;
	}

	public void RunSteps()
	{
		foreach (updateJson step in steps)
		{
			updated = step(updated, idGen);
		}
	}
}
