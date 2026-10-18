using UnityEngine;

namespace Brix.Skybox;

public class MimicMaterialColor : MonoBehaviour
{
	public GameObject SourceObject;

	public string SourcePropertyName;

	public Material DestinationMaterial;

	public string DestinationPropertyName;

	private MaterialPropertyBlock _block;

	private Renderer _sourceRenderer;

	private int _sourceProperty;

	private int _destinationProperty;

	private void Start()
	{
		_block = new MaterialPropertyBlock();
		_sourceRenderer = SourceObject.GetComponent<Renderer>();
		_sourceProperty = Shader.PropertyToID(SourcePropertyName);
		_destinationProperty = Shader.PropertyToID(DestinationPropertyName);
	}

	private void Update()
	{
		_sourceRenderer.GetPropertyBlock(_block);
		Vector4 vector = _block.GetVector(_sourceProperty);
		Color gamma = new Color(vector.x, vector.y, vector.z, vector.w).gamma;
		DestinationMaterial.SetVector(_destinationProperty, gamma);
	}
}
