using UnityEngine;

namespace Brix.Engine.FlatWorld;

public class LodRangeDriver : MonoBehaviour
{
	public enum QualityLevel
	{
		Fastest = 0,
		Default = 1,
		Best = 2,
		Custom = -1
	}

	public QualityLevel qualityLevel = QualityLevel.Custom;

	public BouleDeLod3 bouleDeLod3;

	public int screenHeight;

	public int minScreenHeight = 300;

	public bool autoUpdateScreenHeight = true;

	private QualityLevel exQualityLevel;

	private int exScreenHeight;

	private void Awake()
	{
		if (bouleDeLod3 == null)
		{
			bouleDeLod3 = GetComponent<BouleDeLod3>();
		}
	}

	private void Update()
	{
		if (autoUpdateScreenHeight || screenHeight == 0)
		{
			screenHeight = Screen.height;
		}
		if (exQualityLevel != qualityLevel || exScreenHeight != screenHeight)
		{
			exQualityLevel = qualityLevel;
			exScreenHeight = screenHeight;
			Refresh();
		}
	}

	private void Refresh()
	{
		switch (qualityLevel)
		{
		case QualityLevel.Fastest:
			SetRanges(100f, 200f, 400f);
			break;
		case QualityLevel.Default:
			SetRanges(200f, 400f, 800f);
			break;
		case QualityLevel.Best:
			SetRanges(400f, 700f, 1300f);
			break;
		}
	}

	private void SetRanges(float r1, float r2, float r3)
	{
		float num = (float)Mathf.Max(screenHeight, minScreenHeight) / 1080f;
		bouleDeLod3.R1 = Mathf.RoundToInt(r1 * num);
		bouleDeLod3.R2 = Mathf.RoundToInt(r2 * num);
		bouleDeLod3.R3 = Mathf.RoundToInt(r3 * num);
	}

	public void SetQualityLevel(QualityLevel level)
	{
		qualityLevel = level;
		Refresh();
	}

	public void SetCustomRanges(float r1, float r2, float r3)
	{
		qualityLevel = QualityLevel.Custom;
		SetRanges(r1, r2, r3);
	}
}
