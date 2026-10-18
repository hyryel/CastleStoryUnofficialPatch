using System.Threading;

namespace Brix.Engine.FlatWorld;

public class FlatWorldThread
{
	public Thread Thread = null;

	public FlatWorldSoftRenderer Renderer = new FlatWorldSoftRenderer();

	public ThreadStart InitFullFromLayersAction;

	public ThreadStart InitFromLayersAction;

	public ThreadStart InitPreRenderAction;

	public ThreadStart InitRenderAction;

	public bool IsAlive => Thread != null && Thread.IsAlive;

	public FlatWorldThread()
	{
		InitFullFromLayersAction = InitFullFromLayers;
		InitFromLayersAction = InitFromLayers;
		InitPreRenderAction = InitPreRender;
		InitRenderAction = InitRender;
	}

	private void InitFullFromLayers()
	{
		Thread.Sleep(0);
		Renderer.InitFullFromLayers();
	}

	private void InitFromLayers()
	{
		Thread.Sleep(0);
		Renderer.InitFromLayers();
	}

	private void InitPreRender()
	{
		Thread.Sleep(0);
		Renderer.InitPreRender();
	}

	private void InitRender()
	{
		Thread.Sleep(0);
		Renderer.InitRender();
	}
}
