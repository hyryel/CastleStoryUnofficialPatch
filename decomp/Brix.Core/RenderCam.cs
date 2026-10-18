using System.Collections;
using System.Collections.Generic;
using Brix.Engine.FlatWorld;
using UnityEngine;

public class RenderCam : MonoBehaviour
{
	public class Job
	{
		public Texture2D resultTex;

		public Texture sourceTex;

		public Rect sourceRect;

		public bool done = false;
	}

	public static Queue<Job> jobs = new Queue<Job>();

	public Material copyMat;

	public Texture2D resultTex;

	public Texture sourceTex;

	public Rect sourceRect;

	public int count;

	public static IEnumerator WaitForReadPixels(Texture2D resultTex, Texture sourceTex, Rect sourceRect)
	{
		Job job = new Job
		{
			resultTex = resultTex,
			sourceTex = sourceTex,
			sourceRect = sourceRect
		};
		jobs.Enqueue(job);
		while (!job.done)
		{
			yield return new WaitForEndOfFrame();
		}
	}

	private void Start()
	{
	}

	private void OnPostRender()
	{
		if (jobs.Count != 0)
		{
			Job job = jobs.Peek();
			Texture texture = job.sourceTex;
			RenderTexture renderTex = GetComponent<Camera>().targetTexture;
			GetRenderTex(ref renderTex, texture.width, texture.height);
			GetComponent<Camera>().targetTexture = renderTex;
			copyMat.SetPass(0);
			Graphics.Blit(texture, renderTex, copyMat, 0);
		}
	}

	private void OnRenderImage(RenderTexture src, RenderTexture dst)
	{
		Graphics.Blit(src, dst);
		if (jobs.Count != 0)
		{
			Job job = jobs.Dequeue();
			resultTex = job.resultTex;
			sourceTex = job.sourceTex;
			sourceRect = job.sourceRect;
			count = jobs.Count;
			RenderTexture active = RenderTexture.active;
			RenderTexture.active = GetComponent<Camera>().targetTexture;
			resultTex.ReadPixels(sourceRect, 0, 0, recalculateMipMaps: false);
			resultTex.Apply();
			job.done = true;
			RenderTexture.active = active;
		}
	}

	private static void ReadPixels(ref Texture2D tex, Rect rect)
	{
		Tex.GetTex2D(ref tex, (int)rect.width, (int)rect.height);
		tex.ReadPixels(rect, 0, 0, recalculateMipMaps: false);
		tex.Apply();
	}

	private static void GetRenderTex(ref RenderTexture renderTex, int inWidth, int inHeight, bool setAsActive = true)
	{
		if (renderTex == null || renderTex.width != inWidth || renderTex.height != inHeight)
		{
			if (renderTex != null)
			{
				RenderTexture.ReleaseTemporary(renderTex);
			}
			renderTex = RenderTexture.GetTemporary(inWidth, inHeight, 0, RenderTextureFormat.ARGB32, Tex.ColorSpace);
		}
		renderTex.filterMode = FilterMode.Point;
		if (setAsActive)
		{
			RenderTexture.active = renderTex;
		}
	}
}
