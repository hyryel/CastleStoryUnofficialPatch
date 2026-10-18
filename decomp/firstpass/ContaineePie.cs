using UnityEngine.UI;

public class ContaineePie : Containee
{
	public Image pieImage;

	public RawImage pieRawImage;

	public bool isIconEnabled;

	public bool isActionNegative;

	public bool isActivatedOnClick;

	public override bool playSounds => false;
}
