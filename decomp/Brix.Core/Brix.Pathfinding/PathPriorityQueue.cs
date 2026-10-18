using Brix.Util.BinaryHeap;

namespace Brix.Pathfinding;

public abstract class PathPriorityQueue : PathQueue
{
	protected BinaryHeap<PathLink> Heap = new BinaryHeap<PathLink>(4);

	public override bool Empty => Heap.qte == 0;

	public override void Clear()
	{
		Heap.Reinitialiser();
	}

	public override PathLink Dequeue()
	{
		return Heap.TakeValue();
	}

	public override void Enqueue(PathLink link)
	{
		Heap.MakeRoomForMore();
		Heap.Add(Evaluate(link), link);
	}

	public abstract int Evaluate(PathLink link);
}
