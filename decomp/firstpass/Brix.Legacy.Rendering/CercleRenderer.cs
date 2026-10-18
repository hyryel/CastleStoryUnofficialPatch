using UnityEngine;

namespace Brix.Legacy.Rendering;

[ExecuteInEditMode]
public class CercleRenderer : MonoBehaviour, IRadialRenderer
{
	public ArcRenderer arc;

	public Color couleur;

	public float rayon;

	public float epaisseur;

	public bool shareMaterial;

	public float Radius
	{
		get
		{
			return rayon;
		}
		set
		{
			rayon = value;
		}
	}

	private void Update()
	{
		arc.petitRayon = rayon - epaisseur / 2f;
		arc.grandRayon = rayon + epaisseur / 2f;
		((!shareMaterial) ? arc.obj.GetComponent<Renderer>().material : arc.obj.GetComponent<Renderer>().sharedMaterial).color = couleur;
	}

	public void SetRadius(float radius)
	{
		Radius = radius;
	}
}
