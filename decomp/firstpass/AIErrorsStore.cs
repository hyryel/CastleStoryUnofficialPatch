using System.Collections.Generic;
using System.Linq;
using Brix.Game.AI;
using Brix.Game.Components;
using Brix.Utils;
using Motus.Behavior;
using Smooth.Slinq;

public class AIErrorsStore
{
	private readonly Dictionary<Labor, List<AIError>> _decisionErrors = new Dictionary<Labor, List<AIError>>();

	private readonly Dictionary<Labor, List<AIError>> _executionErrors = new Dictionary<Labor, List<AIError>>();

	private readonly Dictionary<AIError, Thought> _thoughtsOverride = new Dictionary<AIError, Thought>();

	private readonly Dictionary<AIError, int> _priorityOverrides = new Dictionary<AIError, int>();

	private HashSet<AIError> DirectErrors = new HashSet<AIError>();

	public void AddDecisionError(AIError error, Labor labor)
	{
		if (!(labor == null))
		{
			if (error == null)
			{
				error = SharedWorkException<Exceptions.UnknownError>.Get.Problem;
			}
			AIError error2 = ApplyOverrides(error);
			AddError(error2, labor, _decisionErrors);
		}
	}

	public void AddExecutionError(AIError error, Labor labor)
	{
		if (!(labor == null))
		{
			if (error == null)
			{
				error = SharedWorkException<Exceptions.UnknownError>.Get.Problem;
			}
			AIError aIError = ApplyOverrides(error);
			aIError.Peramption = 2f;
			aIError.priority += 100;
			AddError(aIError, labor, _executionErrors);
		}
	}

	public void AddError(AIError error, Labor labor, Dictionary<Labor, List<AIError>> errorDico)
	{
		if (DirectErrors.Contains(error))
		{
			labor.SetTemporaryThought(2f, error.thought);
		}
		List<AIError> valueOrCreate = errorDico.GetValueOrCreate(labor, () => new List<AIError>());
		valueOrCreate.Add(error);
	}

	public void AllowLaborToExpress(params AIError[] errors)
	{
		errors.Slinq().ForEach(delegate(AIError e)
		{
			DirectErrors.Add(e);
		});
	}

	public bool ContainsError(AIError error, Labor labor)
	{
		List<AIError> value = null;
		bool flag = _decisionErrors.TryGetValue(labor, out value) && value.Contains(error);
		bool flag2 = _executionErrors.TryGetValue(labor, out value) && value.Contains(error);
		return flag || flag2;
	}

	public AIError GetMostImportantError(Labor labor)
	{
		List<AIError> value = null;
		if (!_decisionErrors.TryGetValue(labor, out value))
		{
			value = Empty.List<AIError>();
		}
		List<AIError> value2 = null;
		if (!_executionErrors.TryGetValue(labor, out value2))
		{
			value2 = Empty.List<AIError>();
		}
		return value.Union(value2).Max((AIError aIError, AIError max) => aIError.priority > max.priority);
	}

	public bool HasErrorFor(Labor labor)
	{
		List<AIError> value;
		bool flag = _decisionErrors.TryGetValue(labor, out value) && value.Any();
		bool flag2 = _executionErrors.TryGetValue(labor, out value) && value.Any();
		return flag || flag2;
	}

	public void OverridePriority(AIError error, int priority)
	{
		_priorityOverrides.Add(error, priority);
	}

	public void OverrideThought(AIError error, Thought thought)
	{
		_thoughtsOverride.Add(error, thought);
	}

	public void Reset(Labor labor)
	{
		List<AIError> value = null;
		if (_decisionErrors.TryGetValue(labor, out value))
		{
			value.Clear();
		}
		if (_executionErrors.TryGetValue(labor, out value))
		{
			value.RemoveAll((AIError e) => e.Peramption <= 0f);
		}
	}

	private bool ThereAreOverrides()
	{
		return _priorityOverrides.Any() || _thoughtsOverride.Any();
	}

	private AIError ApplyOverrides(AIError error)
	{
		bool flag = false;
		AIError aIError = error.Copy();
		if (_priorityOverrides.ContainsKey(error))
		{
			aIError.priority = _priorityOverrides[error];
			flag = true;
		}
		if (_thoughtsOverride.ContainsKey(error))
		{
			aIError.thought = _thoughtsOverride[error];
			flag = true;
		}
		if (flag)
		{
			return aIError;
		}
		return error;
	}
}
