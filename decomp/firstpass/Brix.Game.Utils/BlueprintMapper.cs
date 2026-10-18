using System.Collections.Generic;
using Brix.Components;
using Brix.External.Factories;
using Brix.External.Factories.Templates;
using Brix.Game.Components;
using Smooth.Slinq;

namespace Brix.Game.Utils;

public class BlueprintMapper : BrixSingleton<BlueprintMapper>
{
	private Dictionary<Factory.AssetKey, Factory.AssetKey> blueprint2Concrete = new Dictionary<Factory.AssetKey, Factory.AssetKey>();

	private Dictionary<Factory.AssetKey, Factory.AssetKey> concrete2Blueprint = new Dictionary<Factory.AssetKey, Factory.AssetKey>();

	public Factory.AssetKey GetMatchingBlueprintAssetKey(Factory.AssetKey concreteKey)
	{
		Factory.AssetKey value = null;
		if (concreteKey != null && !concrete2Blueprint.TryGetValue(concreteKey, out value))
		{
			value = concreteKey;
		}
		return value;
	}

	public Factory.AssetKey GetMatchingConcreteAssetKey(Factory.AssetKey blueprintKey)
	{
		Factory.AssetKey value = null;
		if (blueprintKey != null && !blueprint2Concrete.TryGetValue(blueprintKey, out value))
		{
			value = blueprintKey;
		}
		return value;
	}

	protected void GenerateAssetKeyMappings()
	{
		Factory.GetFactories().Slinq().SelectMany((Factory f) => from t in f.Templates.Slinq()
			select t.GetComponent<Blueprint>())
			.NotNull()
			.ForEach(delegate(Blueprint bp)
			{
				AddMappings(bp);
			});
	}

	private void AddMappings(Blueprint bp)
	{
		blueprint2Concrete.Add(bp.AssetKey, bp.BlueprintResult);
		if (!concrete2Blueprint.ContainsKey(bp.BlueprintResult))
		{
			concrete2Blueprint.Add(bp.BlueprintResult, bp.AssetKey);
		}
	}
}
