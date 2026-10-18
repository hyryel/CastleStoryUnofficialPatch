using Brix.Engine.Nature;

namespace Brix.Input;

public class HarvestProjectPlacementOptions : IPickerComponentOptions
{
	public NatureType AcceptedTypes;

	public static HarvestProjectPlacementOptions TreesOnly = new HarvestProjectPlacementOptions(NatureType.tree);

	public static HarvestProjectPlacementOptions PlantsOnly = new HarvestProjectPlacementOptions(NatureType.plant);

	public static HarvestProjectPlacementOptions BouldersOnly = new HarvestProjectPlacementOptions(NatureType.boulder);

	public static HarvestProjectPlacementOptions IronOnly = new HarvestProjectPlacementOptions(NatureType.iron);

	public static HarvestProjectPlacementOptions BrimstoneOnly = new HarvestProjectPlacementOptions(NatureType.pyrite);

	public static HarvestProjectPlacementOptions Everything = new HarvestProjectPlacementOptions(NatureType.everything);

	public HarvestProjectPlacementOptions(NatureType acceptedTypes)
	{
		AcceptedTypes = acceptedTypes;
	}
}
