namespace Brix.Engine.Blocks;

public interface IVoxelDisplayDataGenerator
{
	void GenerateVoxelDisplayData(BlockData bd, IVoxelDispatcher dispatcher);
}
