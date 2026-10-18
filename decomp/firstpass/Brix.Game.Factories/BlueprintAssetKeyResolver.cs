using Brix.External.Factories;
using Brix.External.Factories.Templates;
using Brix.Game.Components;
using Smooth.Slinq;

namespace Brix.Game.Factories;

public class BlueprintAssetKeyResolver
{
	public static Factory.AssetKey Resolve(Factory.AssetKey assetKey)
	{
		Blueprint blueprint = (from t in Factory.GetFactory(typeof(Blueprints).Name).Templates.Slinq()
			select t.GetComponent<Blueprint>()).FirstOrDefault((Blueprint bp) => bp.BlueprintResult == assetKey);
		return (!(blueprint != null)) ? null : blueprint.GetComponent<AssetKeyHolder>().AssetKey;
	}
}
