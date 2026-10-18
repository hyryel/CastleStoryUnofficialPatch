using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Brix.Engine;
using UnityEngine;

namespace Brix.Pathfinding.Map;

public class MapTexture : Map
{
	public Texture2D heightmap;

	public XYZ sectionSize = new XYZ(16, 16, 16);

	public bool autoRender;

	private Color32[] _colors;

	private IEnumerator renderSections;

	public override IEnumerable<XYZ> SurfaceVoxels => GetSurface(new XYZ(1, 0, 1), new XYZ(heightmap.width - 2, 256, heightmap.height - 2));

	private void Augment(ref byte bottom, ref byte top, Color32 c)
	{
		bottom = (byte)Mathf.Max(bottom, c.r);
		top = (byte)Mathf.Min(top, c.g);
	}

	public IEnumerable<XYZ> GetSurface(XYZ start, XYZ size)
	{
		XYZ end = start + size;
		XYZ p = start;
		p.z = start.z;
		while (p.z < end.z)
		{
			p.x = start.x;
			while (p.x < end.x)
			{
				int i = p.x + p.z * heightmap.width;
				Color32 c = _colors[i];
				if (c.r < c.g)
				{
					c.r = (byte)Mathf.Max(c.r, start.y);
					c.g = (byte)Mathf.Min(c.g, end.y);
					c.b = (byte)(c.r + 1);
					c.a = (byte)(c.g - 1);
					Augment(ref c.b, ref c.a, _colors[i - 1]);
					Augment(ref c.b, ref c.a, _colors[i + 1]);
					Augment(ref c.b, ref c.a, _colors[i - heightmap.width]);
					Augment(ref c.b, ref c.a, _colors[i + heightmap.width]);
					c.b = (byte)Mathf.Min(c.b, c.g);
					c.a = (byte)Mathf.Max(c.a, c.r);
					p.y = c.r;
					while (p.y < c.b)
					{
						yield return p;
						p.y++;
					}
					p.y = c.a;
					while (p.y < c.g)
					{
						yield return p;
						p.y++;
					}
				}
				p.x++;
			}
			p.z++;
		}
	}

	public override bool Contains(XYZ p)
	{
		Color32 color = _colors[p.x + heightmap.width * p.z];
		return p.y >= color.r && p.y < color.g;
	}

	[ContextMenu("Init Voxels")]
	public void InitVoxels()
	{
		start = XYZ.zero;
		size = new XYZ(heightmap.width, 256, heightmap.height);
	}

	[ContextMenu("Init Sections")]
	public void InitSections()
	{
		XYZ zero = XYZ.zero;
		XYZ xYZ = XYZ.Div(size, sectionSize);
		Material sharedMaterial = new Material(Shader.Find("Diffuse"));
		zero.z = 0;
		while (zero.z < xYZ.z)
		{
			zero.x = 0;
			while (zero.x < xYZ.x)
			{
				zero.y = 0;
				while (zero.y < xYZ.y)
				{
					if (GetSurface(XYZ.Mul(zero, sectionSize), sectionSize).Any())
					{
						MapSection component = new GameObject("Section", typeof(MapSection), typeof(MeshRenderer), typeof(MeshFilter)).GetComponent<MapSection>();
						component.start = XYZ.Mul(zero, sectionSize);
						component.size = sectionSize;
						component.map = this;
						component.transform.SetParent(base.transform);
						component.GetComponent<MeshRenderer>().sharedMaterial = sharedMaterial;
					}
					zero.y++;
				}
				zero.x++;
			}
			zero.z++;
		}
		renderSections = RenderSections().GetEnumerator();
	}

	private IEnumerable RenderSections()
	{
		MapSection[] array = Object.FindObjectsOfType<MapSection>();
		foreach (MapSection section in array)
		{
			section.GetComponent<MeshFilter>().mesh = MapRenderer.GetRenderedMesh(section);
			yield return null;
		}
	}

	private void Start()
	{
		base.name = heightmap.name;
		_colors = heightmap.GetPixels32();
		InitVoxels();
		if (autoRender)
		{
			InitSections();
		}
	}

	private void Update()
	{
		if (renderSections != null && !renderSections.MoveNext())
		{
			renderSections = null;
		}
	}
}
