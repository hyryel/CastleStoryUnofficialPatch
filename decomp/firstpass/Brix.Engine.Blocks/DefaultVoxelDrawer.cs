namespace Brix.Engine.Blocks;

public class DefaultVoxelDrawer
{
	protected readonly IVoxelDisplayDataGenerator VoxelDisplayDataGenerator;

	public DefaultVoxelDrawer(IVoxelDisplayDataGenerator voxelDisplayDataGenerator)
	{
		VoxelDisplayDataGenerator = voxelDisplayDataGenerator;
	}

	public void RedrawVoxel(BlockData bd, MaterialCombineDispatcher materialCombineDispatcher)
	{
		VoxelDisplayDataGenerator.GenerateVoxelDisplayData(bd, materialCombineDispatcher);
	}
}
