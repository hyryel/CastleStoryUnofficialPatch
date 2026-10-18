using System.Collections.Generic;
using Brix.Engine;
using Brix.Network;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TwitchChat))]
public class TwitchChatExample : MonoBehaviour
{
	public int maxMessages = 100;

	private LinkedList<GameObject> messages = new LinkedList<GameObject>();

	public InputField inputField;

	public Button submitButton;

	public RectTransform chatBox;

	public Text outputText;

	public ScrollRect scrollRect;

	private TwitchChat twitchChat;

	private void OnChatMsgRecieved(string msg)
	{
		int num = msg.IndexOf("PRIVMSG #");
		string msgString = msg.Substring(num + twitchChat.chanName.Length + 11);
		string userName = msg.Substring(1, msg.IndexOf('!') - 1);
		if (messages.Count > maxMessages)
		{
			Object.Destroy(messages.First.Value);
			messages.RemoveFirst();
		}
		CreateUIMessage(userName, msgString);
	}

	private void CreateUIMessage(string userName, string msgString)
	{
		Color32 color = ColorFromUsername(userName);
		string text = $"#{color.r * 255:X2}{color.g * 255:X2}{color.b * 255:X2}";
		GameSignals.Invoke(UNetManager.LobbyNewChatMessage, "<color=" + text + ">" + userName + "</color> : " + msgString);
	}

	public void OnSubmit()
	{
		if (inputField.text.Length > 0)
		{
			twitchChat.SendMsg(inputField.text);
			CreateUIMessage(twitchChat.userName, inputField.text);
			inputField.text = string.Empty;
		}
	}

	private static Color ColorFromUsername(string username)
	{
		Random.InitState(username.Length + username[0] + username[username.Length - 1]);
		return new Color(Random.Range(0.25f, 0.55f), Random.Range(0.2f, 0.55f), Random.Range(0.25f, 0.55f));
	}

	private void Start()
	{
		twitchChat = GetComponent<TwitchChat>();
		twitchChat.messageRecievedEvent.AddListener(OnChatMsgRecieved);
	}
}
