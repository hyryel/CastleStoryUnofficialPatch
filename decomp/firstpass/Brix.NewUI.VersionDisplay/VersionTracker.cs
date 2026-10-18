using Brix.Utils.Logging;
using Newtonsoft.Json;
using UnityEngine;

namespace Brix.NewUI.VersionDisplay;

public class VersionTracker : MonoBehaviour
{
	private class Version
	{
		public string data = string.Empty;

		public string date = string.Empty;

		public string major = string.Empty;

		public string minor = string.Empty;

		public string release = string.Empty;

		public string release_name = string.Empty;

		public string rev = string.Empty;

		public string protocol_version = string.Empty;

		public string save_version = string.Empty;

		public string GetSemanticVersion()
		{
			return $"{release}.{major}.{minor}.{rev}";
		}

		public string GetDisplayVersion()
		{
			return release_name + " " + date + " " + release + "." + major + "." + minor + "." + rev + " (" + data + ", " + save_version + ", " + protocol_version + ")";
		}

		public static Version CreateFromText(TextAsset textAsset)
		{
			logger.Info("version.txt : " + textAsset.text);
			return JsonConvert.DeserializeObject<Version>(textAsset.text);
		}
	}

	private static BrixLogger logger = new BrixLogger(typeof(VersionTracker).FullName);

	public TextAsset textAsset;

	private Version _version;

	public string SaveVersion => version.save_version;

	public string ProtocolVersion => version.protocol_version;

	public string DisplayVersion => version.GetDisplayVersion();

	public string SemanticVersion => version.GetSemanticVersion();

	private Version version => _version ?? (_version = Version.CreateFromText(textAsset));
}
