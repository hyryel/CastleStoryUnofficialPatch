using Brix.Input;
using UnityEngine;

namespace Brix.Engine.Loop;

public class CinematiqueLoop : MonoBehaviour
{
	public Transform cam;

	public Vector3 center = new Vector3(512f, 132f, 512f);

	public Vector3 startPosition = new Vector3(312f, 202f, 512f);

	public float angularSpeed = 3.6f;

	public bool playing;

	private void Start()
	{
		GameSignals.Once(GameSignals.TerrainReady, PlayCinematique);
		GameSignals.Once(GameSignals.LevelReady, StopCinematique);
		GameSignals.Connect(GameSignals.StartError, PlayCinematique);
		GameSignals.Connect(GameSignals.LeaveGame, StopCinematique);
	}

	private void Update()
	{
		if (playing)
		{
			cam.RotateAround(center, Vector3.up, Time.deltaTime * angularSpeed);
			cam.LookAt(center);
		}
	}

	public void PlayCinematique(string error)
	{
		PlayCinematique();
	}

	public void PlayCinematique()
	{
		playing = true;
		cam = Camera.main.transform;
		cam.GetComponent<CameraScript>().Cinematique = true;
		cam.position = startPosition;
	}

	public void StopCinematique()
	{
		playing = false;
		cam.GetComponent<CameraScript>().Cinematique = false;
	}
}
