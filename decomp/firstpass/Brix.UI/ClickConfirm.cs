using Brix.Audio;
using Brix.Legacy.Rendering;
using Brix.Lifecycle.Pooling;
using UnityEngine;

namespace Brix.UI;

public class ClickConfirm : MonoBehaviour
{
	public CercleRenderer circle;

	public float radiusStart;

	public float radiusEnd;

	public AnimationCurve radiusCurve;

	public float thicknessStart;

	public float thicknessEnd;

	public AnimationCurve thicknessCurve;

	public float lifeSpan;

	public string sound;

	private float _progress;

	private void Start()
	{
		circle.rayon = radiusStart;
		circle.epaisseur = thicknessStart;
		SoundEngine.Play(sound);
	}

	private void Update()
	{
		_progress += Time.deltaTime / lifeSpan;
		circle.rayon = Mathf.Lerp(radiusStart, radiusEnd, radiusCurve.Evaluate(_progress));
		circle.epaisseur = Mathf.Lerp(thicknessStart, thicknessEnd, thicknessCurve.Evaluate(_progress));
		if (_progress >= 1f)
		{
			ObjectPoolSingleton.Release(base.gameObject);
		}
	}
}
