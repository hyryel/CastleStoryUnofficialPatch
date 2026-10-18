using System.Collections.Generic;
using System.Threading;
using Brix.Engine.FlatWorld.Layers;
using Brix.Util;

namespace Brix.Engine.FlatWorld;

internal class ThreadedHeavyWorker
{
	private enum Request
	{
		GetDirtyMegaVoxel,
		RenderMegaVoxel,
		RefreshCollider,
		CleanupPostRender
	}

	public TestWorldConfig3 Test;

	public ThreadedWorkList WorkList;

	public Thread Thread;

	public FlatWorldSoftRenderer Renderer;

	public int Width;

	public int Height;

	public XYZ CurrentMv;

	private readonly Queue<Request> _requests = new Queue<Request>();

	public ThreadedHeavyWorker(TestWorldConfig3 test, ThreadedWorkList workList)
	{
		Test = test;
		WorkList = workList;
		Renderer = new FlatWorldSoftRenderer();
		Thread = new Thread(ThreadedWork);
		XYZ xYZ = Test.MvSize + Test.Border * 2;
		Width = 1 << Calcule.GetBestPower(xYZ.x);
		Height = 1 << Calcule.GetBestPower(xYZ.z);
		Thread.Start();
	}

	public void Destroy()
	{
		Thread.Abort();
		lock (_requests)
		{
			_requests.Clear();
		}
	}

	public void ThreadedWork()
	{
		Thread.Sleep(0);
		while (true)
		{
			QueueRequest(Request.GetDirtyMegaVoxel);
			WaitForRequests();
			Renderer.InitFullFromLayers();
			QueueRequest(Request.RenderMegaVoxel);
			if (Test.useColliders)
			{
				QueueRequest(Request.RefreshCollider);
			}
			QueueRequest(Request.CleanupPostRender);
		}
	}

	public bool MainThreadWork()
	{
		lock (_requests)
		{
			if (_requests.Count == 0)
			{
				return false;
			}
			do
			{
				bool flag = false;
				switch (_requests.Peek())
				{
				case Request.GetDirtyMegaVoxel:
					if (!GetDirtyMegaVoxel())
					{
						return false;
					}
					break;
				case Request.RenderMegaVoxel:
					RenderMegaVoxel();
					flag = Test.threadWaitForCollider;
					break;
				case Request.RefreshCollider:
					RefreshCollider();
					flag = true;
					break;
				case Request.CleanupPostRender:
					CleanupPostRender();
					break;
				}
				_requests.Dequeue();
				if (flag)
				{
					return true;
				}
			}
			while (_requests.Count > 0);
		}
		Thread.Interrupt();
		return false;
	}

	private bool GetDirtyMegaVoxel()
	{
		if (!WorkList.TryReserve(out CurrentMv))
		{
			return false;
		}
		IFlatWorldColors cachedWorld = Test.drawer.GetCachedWorld(CurrentMv);
		Test.InitSoftRenderer(Renderer, cachedWorld, CurrentMv, Width, Height);
		return true;
	}

	private void RenderMegaVoxel()
	{
		Renderer.MegaVoxelGeo = Test.RenderMegaVoxel(Renderer.MeshInfo, Renderer.Start);
	}

	private void RefreshCollider()
	{
		Test.RefreshCollider(Renderer.MegaVoxelGeo, Renderer.ColliderMeshInfo);
	}

	private void CleanupPostRender()
	{
		if (Test.lodUpdater != null)
		{
			Test.lodUpdater.SetDirty(CurrentMv);
		}
		WorkList.Release(CurrentMv);
		WorkList.TotalVoxels += Renderer.World.CountOffsets;
		Renderer.World.SwapPostRender(Renderer.TempWorld);
	}

	private void QueueRequest(Request request)
	{
		lock (_requests)
		{
			_requests.Enqueue(request);
		}
	}

	private void WaitForRequests()
	{
		try
		{
			Thread.Sleep(-1);
		}
		catch (ThreadInterruptedException)
		{
		}
	}
}
