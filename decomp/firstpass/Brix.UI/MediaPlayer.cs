using UnityEngine.Events;

namespace Brix.UI;

public class MediaPlayer
{
	public class EventsHandle
	{
		public void AddListener(UnityAction<BrixMediaPlayer, MediaPlayer, MediaPlayerEvent.EventType, RenderHeads.Media.AVProVideo.ErrorCode> unityAction)
		{
		}

		public void RemoveAllListeners()
		{
		}
	}

	public enum FileLocation
	{
		RelativeToStreamingAssetsFolder
	}

	public string m_VideoPath = string.Empty;

	public bool m_Loop;

	public bool m_Muted;

	public bool m_AutoOpen;

	public bool m_AutoStart;

	public float m_Volume;

	public EventsHandle Events = new EventsHandle();

	public void Play()
	{
	}

	public void Stop()
	{
	}

	public void OpenVideoFromFile(FileLocation fileLocation, string path, bool autoStart)
	{
	}
}
