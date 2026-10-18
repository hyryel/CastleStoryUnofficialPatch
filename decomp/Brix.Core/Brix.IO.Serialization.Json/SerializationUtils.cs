using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Text;
using Brix.IO.Serialization.ReferenceResolvers;
using Brix.Util;
using Brix.Utils.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Brix.IO.Serialization.Json;

public static class SerializationUtils
{
	private delegate void SplitFullyQualifiedTypeName(string fullyQualifiedTypeName, out string typeName, out string assemblyName);

	private static BrixLogger logger;

	private static DefaultSerializationBinder defaultBinder;

	private static Type reflectionUtilsType;

	private static SplitFullyQualifiedTypeName _splitFullyQualifiedTypeName;

	static SerializationUtils()
	{
		logger = new BrixLogger();
		defaultBinder = (DefaultSerializationBinder)RearViewMirror.GetField(typeof(DefaultSerializationBinder), null, "Instance");
		reflectionUtilsType = RearViewMirror.GetType("Newtonsoft.Json.Utilities.ReflectionUtils");
		MethodInfo method = reflectionUtilsType.GetMethod("SplitFullyQualifiedTypeName", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		Delegate obj = Delegate.CreateDelegate(typeof(SplitFullyQualifiedTypeName), method);
		_splitFullyQualifiedTypeName = (SplitFullyQualifiedTypeName)obj;
	}

	public static bool IsJGameObject(this JToken jgo)
	{
		JToken jToken = ((jgo is JObject jObject) ? jObject["$type"] : null);
		return jToken != null && jToken.Value<string>() == "UnityEngine.GameObject, UnityEngine";
	}

	public static void Delete(this JToken token)
	{
		if (token != null)
		{
			if (token.Parent.Type == JTokenType.Property)
			{
				token.Parent.Remove();
			}
			token.Remove();
		}
	}

	public static string Id(this JContainer jobject)
	{
		return (jobject["$id"] == null) ? null : jobject["$id"].Value<string>();
	}

	public static string Ref(this JContainer jobject)
	{
		return (jobject["$ref"] == null) ? null : jobject["$ref"].Value<string>();
	}

	public static string Parent(this JContainer jobject)
	{
		JToken jToken = jobject["$parent"];
		return (jToken == null || jToken.Type != JTokenType.String) ? null : jobject["$parent"].Value<string>();
	}

	public static string RefOrParent(this JContainer jobject)
	{
		JToken jToken = jobject["$ref"];
		if (jToken != null)
		{
			return jToken.Value<string>();
		}
		JToken jToken2 = jobject["$parent"];
		return (jToken2 != null && jToken2.Type == JTokenType.String) ? jToken2.Value<string>() : null;
	}

	public static bool FieldExists(string fieldName, JObject json)
	{
		return json[fieldName] != null;
	}

	public static T TryDeserializeField<T>(string fieldName, JObject jObject)
	{
		T result = default(T);
		if (FieldExists(fieldName, jObject))
		{
			try
			{
				return jObject[fieldName].Value<T>();
			}
			catch (Exception innerException)
			{
				string message = string.Concat("Deserialization of the field '", fieldName, "' failed. Could not create object of type '", typeof(T), "' with the value '", jObject[fieldName], "'. This was done in a TryDeserializeField.");
				logger.Warn(message);
				throw new SerializationException(message, innerException);
			}
		}
		return result;
	}

	public static T DeserializeField<T>(string fieldName, JObject jObject)
	{
		if (!FieldExists(fieldName, jObject))
		{
			string message = "The fieldname '" + fieldName + "' was not found in the provided JObject: \n" + jObject.ToString();
			logger.Error(message);
			throw new ArgumentException(message);
		}
		try
		{
			return jObject[fieldName].Value<T>();
		}
		catch (Exception innerException)
		{
			string message2 = string.Concat("Deserialization of the field '", fieldName, "' failed. Could not create object of type '", typeof(T), "' with the value '", jObject[fieldName], "'");
			logger.Error(message2);
			throw new SerializationException(message2, innerException);
		}
	}

	public static string GetNewtonsoftTypeName(Type t, JsonSerializer serializer)
	{
		string assemblyQualifiedName = t.AssemblyQualifiedName;
		return serializer.TypeNameAssemblyFormat switch
		{
			FormatterAssemblyStyle.Simple => RemoveAssemblyDetails(assemblyQualifiedName), 
			FormatterAssemblyStyle.Full => t.AssemblyQualifiedName, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public static string RemoveAssemblyDetails(string fullyQualifiedTypeName)
	{
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = false;
		bool flag2 = false;
		foreach (char c in fullyQualifiedTypeName)
		{
			switch (c)
			{
			case '[':
				flag = false;
				flag2 = false;
				stringBuilder.Append(c);
				break;
			case ']':
				flag = false;
				flag2 = false;
				stringBuilder.Append(c);
				break;
			case ',':
				if (!flag)
				{
					flag = true;
					stringBuilder.Append(c);
				}
				else
				{
					flag2 = true;
				}
				break;
			default:
				if (!flag2)
				{
					stringBuilder.Append(c);
				}
				break;
			}
		}
		return stringBuilder.ToString();
	}

	public static Type GetMemberUnderlyingType(MemberInfo member)
	{
		ValidationUtils.ArgumentNotNull(member, "member");
		return member.MemberType switch
		{
			MemberTypes.Field => ((FieldInfo)member).FieldType, 
			MemberTypes.Property => ((PropertyInfo)member).PropertyType, 
			MemberTypes.Event => ((EventInfo)member).EventHandlerType, 
			_ => throw new ArgumentException("MemberInfo must be of type FieldInfo, PropertyInfo or EventInfo", "member"), 
		};
	}

	public static Type BindToType(string strTypeAndAssembly, JsonSerializer serializer)
	{
		try
		{
			_splitFullyQualifiedTypeName(strTypeAndAssembly, out var typeName, out var assemblyName);
			return serializer.Binder.BindToType(assemblyName, typeName);
		}
		catch (Exception)
		{
			throw new InvalidDataException("The component's Type/Assembly '" + strTypeAndAssembly + "' is invalid in an invalid format following Json: ");
		}
	}

	public static Type BindToType(string strTypeAndAssembly)
	{
		try
		{
			_splitFullyQualifiedTypeName(strTypeAndAssembly, out var typeName, out var assemblyName);
			return defaultBinder.BindToType(assemblyName, typeName);
		}
		catch (Exception)
		{
			throw new InvalidDataException("The component's Type/Assembly '" + strTypeAndAssembly + "' is invalid in an invalid format following Json: ");
		}
	}

	public static bool IsPropertySerializable(this JsonProperty p, object propertyValue = null)
	{
		return !p.Ignored && p.Readable && ShouldSerialize(p, propertyValue) && IsSpecified(p, propertyValue);
	}

	private static bool ShouldSerialize(JsonProperty property, object target)
	{
		if (property.ShouldSerialize == null)
		{
			return true;
		}
		return property.ShouldSerialize(target);
	}

	private static bool IsSpecified(JsonProperty property, object target)
	{
		if (property.GetIsSpecified == null)
		{
			return true;
		}
		return property.GetIsSpecified(target);
	}

	public static DefaultGameReferenceResolver BrixReferenceResolver(this JsonSerializer serializer)
	{
		return serializer.ReferenceResolver as DefaultGameReferenceResolver;
	}

	public static MemberInfo GetUnderlyingMemberInfo(this JsonProperty property)
	{
		Type type = property.ValueProvider.GetType();
		FieldInfo field = type.GetField("_memberInfo", BindingFlags.Instance | BindingFlags.NonPublic);
		return field.GetValue(property.ValueProvider) as MemberInfo;
	}
}
