using UnityEngine;

public class GA_ExampleHighScore : MonoBehaviour
{
	public GameObject BallPrefab;

	private static bool GAMEOVER;

	private static int SCORE;

	private static int HIGHSCORE;

	private int _genderIndex;

	private int _birthYear;

	private int _friendCount;

	private GUIContent[] genderComboBoxList = new GUIContent[3]
	{
		new GUIContent("-"),
		new GUIContent("Female"),
		new GUIContent("Male")
	};

	private ComboBox genderComboBoxControl = new ComboBox();

	private Vector3 _startingPosition;

	private void Start()
	{
		_startingPosition = base.transform.position;
	}

	private void OnGUI()
	{
		if (GAMEOVER)
		{
			GUI.Window(0, new Rect(Screen.width / 2 - 100, Screen.height / 2 - 110, 200f, 220f), HighScoreWindow, "GAME OVER");
		}
		GUI.Label(new Rect(10f, Screen.height - 70, 200f, 20f), "SCORE: " + SCORE);
	}

	public static void GameOver(Vector3 position)
	{
		if (PlayerPrefs.HasKey("GAExampleScore"))
		{
			HIGHSCORE = PlayerPrefs.GetInt("GAExampleScore", 0);
		}
		GAMEOVER = true;
		GA.API.Design.NewEvent("GameOver");
		GA_Queue.ForceSubmit();
	}

	public static void AddScore(int score, string scoreName, Vector3 position)
	{
		SCORE += score;
		GA.API.Design.NewEvent("Score:" + scoreName, 10f);
	}

	private void HighScoreWindow(int windowID)
	{
		GUILayout.BeginHorizontal();
		if (SCORE > HIGHSCORE)
		{
			GUILayout.Label("OLD HIGHSCORE:");
		}
		else
		{
			GUILayout.Label("HIGHSCORE:");
		}
		GUILayout.Label(HIGHSCORE.ToString());
		GUILayout.EndHorizontal();
		GUILayout.Space(10f);
		GUILayout.BeginHorizontal();
		GUILayout.Label("NEW SCORE:");
		GUILayout.Label(SCORE.ToString());
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("BIRTH YEAR:");
		int result = 0;
		if (int.TryParse(GUILayout.TextField((_birthYear == 0) ? string.Empty : _birthYear.ToString(), GUILayout.Width(50f)), out result))
		{
			_birthYear = result;
		}
		else
		{
			_birthYear = 0;
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("FRIEND COUNT:");
		int result2 = 0;
		if (int.TryParse(GUILayout.TextField((_friendCount == 0) ? string.Empty : _friendCount.ToString(), GUILayout.Width(50f)), out result2))
		{
			_friendCount = result2;
		}
		else
		{
			_friendCount = 0;
		}
		GUILayout.EndHorizontal();
		GUILayout.Label("GENDER:");
		_genderIndex = genderComboBoxControl.List(new Rect(115f, 130f, 75f, 20f), genderComboBoxList[_genderIndex], genderComboBoxList, GUI.skin.button);
		GUILayout.Space(10f);
		GUI.enabled = SCORE > HIGHSCORE;
		if (GUILayout.Button("SAVE", GUILayout.Width(100f)))
		{
			HIGHSCORE = SCORE;
			PlayerPrefs.SetInt("GAExampleScore", SCORE);
			PlayerPrefs.Save();
			GA_User.Gender gender = GA_User.Gender.Unknown;
			if (_genderIndex == 1)
			{
				gender = GA_User.Gender.Female;
			}
			else if (_genderIndex == 2)
			{
				gender = GA_User.Gender.Male;
			}
			GA.API.User.NewUser(gender, _birthYear, _friendCount);
			GA_Queue.ForceSubmit();
		}
		GUI.enabled = true;
		if (GUILayout.Button("RESTART", GUILayout.Width(100f)))
		{
			GAMEOVER = false;
			SCORE = 0;
			GameObject gameObject = Object.Instantiate(BallPrefab, new Vector3(0f, 3f, 0f), Quaternion.identity);
			gameObject.name = "Ball";
			base.transform.position = _startingPosition;
			GetComponent<Rigidbody>().velocity = Vector3.zero;
		}
		if (!genderComboBoxControl.IsShowingList())
		{
			GUI.DrawTexture(new Rect(126f, 156f, 55f, 55f), GA.SettingsGA.Logo);
		}
		GUI.FocusWindow(0);
	}
}
