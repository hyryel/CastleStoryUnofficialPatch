using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Brix.Shell;

[Serializable]
public class Data
{
	public string language = "JS";

	public string input = "";

	public string output = "";

	public string richOutput = "";

	public string lastOutput = "";

	public History history = new History();

	public QuickRefs quickRefs = new QuickRefs();

	public Options options = new Options();

	public CodeDatabase database = new CodeDatabase();

	public static Data LoadFromXML(string xmlString)
	{
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(Data));
		XmlReaderSettings settings = new XmlReaderSettings
		{
			ValidationType = ValidationType.None
		};
		using MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(xmlString));
		return (Data)xmlSerializer.Deserialize(XmlReader.Create(stream, settings));
	}

	public string ToXML()
	{
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(Data));
		MemoryStream memoryStream = new MemoryStream();
		xmlSerializer.Serialize(memoryStream, this);
		memoryStream.Flush();
		memoryStream.Seek(0L, SeekOrigin.Begin);
		using StreamReader streamReader = new StreamReader(memoryStream, Encoding.UTF8);
		return streamReader.ReadToEnd().Replace("&#x0;", "");
	}
}
