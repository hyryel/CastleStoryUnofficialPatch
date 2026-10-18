using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

[ExecuteInEditMode]
public class PrefabPopulator : MonoBehaviour
{
	private class Accessor<T>
	{
		public Func<T> fnGet;

		public Action<T> fnSet;
	}

	public PrefabTemplate template;

	[TaggedList("GetPropertyTag")]
	public string[] properties;

	[TaggedList("GetLinkTag")]
	public UnityEngine.Object[] links;

	private GameObject instance;

	[Conditional("editor")]
	public bool preview;

	[Conditional("preview")]
	public bool deepPreview;

	private static bool tempDeepPreview = false;

	[Conditional("canImport")]
	public bool reimport = true;

	private bool exPreview;

	private bool exDeepPreview;

	private bool editor => !Application.isPlaying;

	private bool canImport => instance != null || base.transform.Find(prefab.name) != null;

	private GameObject prefab => (template.prefab != null) ? template.prefab : template.gameObject;

	public string GetPropertyTag(int i)
	{
		return template.properties[i];
	}

	public string GetLinkTag(int i)
	{
		return template.links[i];
	}

	private void Awake()
	{
		if (editor)
		{
			if (preview || tempDeepPreview)
			{
				EditorInit();
			}
		}
		else
		{
			Init();
		}
	}

	private void Update()
	{
		if (reimport)
		{
			ReImport();
			reimport = false;
		}
		if (editor && (exPreview != preview || exDeepPreview != deepPreview))
		{
			exPreview = preview;
			exDeepPreview = deepPreview;
			if (preview)
			{
				EditorInit();
			}
			else
			{
				Destroy();
			}
		}
	}

	private void EditorInit()
	{
		if (deepPreview && !tempDeepPreview)
		{
			tempDeepPreview = true;
			Init();
			tempDeepPreview = false;
		}
		else
		{
			Init();
		}
	}

	private void Init()
	{
		Instantiate();
		Populate();
		instance.SetActive(value: true);
	}

	[ContextMenu("Instantiate")]
	private void Instantiate()
	{
		if (instance != null)
		{
			Destroy();
		}
		GameObject gameObject = template.prefab;
		if (gameObject == null)
		{
			gameObject = template.gameObject;
		}
		gameObject.SetActive(value: false);
		GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject);
		gameObject.SetActive(value: true);
		gameObject2.name = gameObject.name;
		instance = gameObject2;
	}

	[ContextMenu("Populate")]
	private void Populate()
	{
		GameObject gameObject = instance;
		gameObject.transform.parent = base.transform;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = template.transform.localRotation;
		gameObject.transform.localScale = template.transform.localScale;
		for (int i = 0; i < properties.Length + links.Length; i++)
		{
			string text = ((i < properties.Length) ? template.properties[i] : template.links[i - properties.Length]);
			object obj = ((i < properties.Length) ? ((object)properties[i]) : ((object)links[i - properties.Length]));
			Type baseType = ((i < properties.Length) ? typeof(string) : typeof(UnityEngine.Object));
			Accessor<object> accessor = GetAccessor(gameObject.transform, text, baseType);
			if (accessor != null)
			{
				accessor.fnSet(obj);
				continue;
			}
			Debug.LogWarning("PrefabPopulator(" + template.name + ") failed to populate field " + text + " with value " + obj);
		}
	}

	[ContextMenu("ReImport")]
	private void ReImport()
	{
		Array.Resize(ref properties, template.properties.Length);
		Array.Resize(ref links, template.links.Length);
		if (!canImport)
		{
			return;
		}
		GameObject gameObject = instance;
		if (gameObject == null)
		{
			gameObject = base.transform.Find(prefab.name).gameObject;
		}
		for (int i = 0; i < properties.Length + links.Length; i++)
		{
			string text = ((i < properties.Length) ? template.properties[i] : template.links[i - properties.Length]);
			Type baseType = ((i < properties.Length) ? typeof(string) : typeof(UnityEngine.Object));
			Accessor<object> accessor = GetAccessor(gameObject.transform, text, baseType);
			if (accessor != null)
			{
				object obj = accessor.fnGet();
				if (i < properties.Length)
				{
					properties[i] = (string)obj;
				}
				else
				{
					links[i - properties.Length] = (UnityEngine.Object)obj;
				}
			}
			else
			{
				Debug.LogWarning("PrefabPopulator(" + template.name + ") failed to import field " + text);
			}
		}
	}

	[ContextMenu("Destroy")]
	private void Destroy()
	{
		if (!(instance == null))
		{
			if (editor)
			{
				UnityEngine.Object.DestroyImmediate(instance);
			}
			else
			{
				UnityEngine.Object.Destroy(instance);
			}
		}
	}

	private Accessor<object> GetAccessor(Transform root, string s, Type baseType)
	{
		Transform t = ExtractTransform(root, ref s);
		Component c = ExtractComponent(t, ref s);
		return ExtractAccessor(c, ref s, baseType);
	}

	private Transform ExtractTransform(Transform root, ref string s)
	{
		Transform transform = root;
		string[] array = s.Split('/');
		for (int i = 0; i < array.Length - 1; i++)
		{
			if (!(transform != null))
			{
				break;
			}
			transform = transform.Find(array[i]);
			if (transform == null)
			{
				Debug.LogWarning("PrefabPopulator can't find Transform " + array[i] + " in " + s);
			}
		}
		s = array[array.Length - 1];
		return transform;
	}

	private Component ExtractComponent(Transform t, ref string s)
	{
		if (t == null)
		{
			return null;
		}
		string text = s;
		string text2 = text;
		s = "";
		if (text.IndexOf(".") >= 0)
		{
			text2 = text.Substring(0, text.IndexOf("."));
			s = text.Substring(text.IndexOf(".") + 1);
		}
		if (text2.IndexOf("[") >= 0)
		{
			string className = text2.Substring(0, text2.IndexOf("["));
			Type type = Donnees.GetType(className);
			Component[] components = t.GetComponents(type);
			string text3 = text2.Substring(text2.IndexOf("["));
			text3 = text3.Substring(1, text3.Length - 2);
			return components[int.Parse(text3)];
		}
		return t.GetComponent(text2);
	}

	private Accessor<object> ExtractAccessor(Component c, ref string s, Type baseType)
	{
		if (c == null)
		{
			return null;
		}
		string[] array = s.Split('.', '[');
		int num = 0;
		object obj = c;
		Func<object> fnGet = null;
		Action<object> fnSet = null;
		for (num = 0; num < array.Length; num++)
		{
			if (array[num].IndexOf("]") >= 0)
			{
				string s2 = array[num].Substring(0, array[num].IndexOf("]"));
				int j = int.Parse(s2);
				IList list = obj as IList;
				if (list == null)
				{
					break;
				}
				Type type = obj.GetType();
				Type wantType = type.GetElementType();
				fnGet = () => ChangeType(list[j], baseType);
				fnSet = delegate(object value)
				{
					list[j] = ChangeType(value, wantType);
				};
				obj = list[j];
				continue;
			}
			FieldInfo f = obj.GetType().GetField(array[num]);
			if (f != null)
			{
				object o2 = obj;
				fnGet = () => ChangeType(f.GetValue(o2), baseType);
				fnSet = delegate(object value)
				{
					f.SetValue(o2, ChangeType(value, f.FieldType));
				};
				obj = f.GetValue(obj);
				continue;
			}
			PropertyInfo p = obj.GetType().GetProperty(array[num]);
			if (p == null)
			{
				break;
			}
			object o3 = obj;
			fnGet = () => ChangeType(p.GetGetMethod().Invoke(o3, new object[0]), baseType);
			fnSet = delegate(object value)
			{
				p.GetSetMethod().Invoke(o3, new object[1] { ChangeType(value, p.PropertyType) });
			};
			obj = p.GetGetMethod().Invoke(obj, new object[0]);
		}
		if (num < array.Length)
		{
			Debug.LogWarning("PrefabPopulator can't find field " + array[num] + " in " + s + " (" + c.name + "[" + c.GetType().Name + "])");
		}
		s = "";
		return new Accessor<object>
		{
			fnGet = fnGet,
			fnSet = fnSet
		};
	}

	private object ChangeType(object value, Type wantType)
	{
		if (wantType != null && value != null && !wantType.IsAssignableFrom(value.GetType()))
		{
			try
			{
				value = Convert.ChangeType(value, wantType);
			}
			catch (Exception)
			{
				value = (((!(value is Component) && !(value is GameObject)) || (!typeof(Component).IsAssignableFrom(wantType) && !typeof(GameObject).IsAssignableFrom(wantType))) ? null : ((!(value is Component)) ? (value as GameObject).GetComponent(wantType) : (value as Component).GetComponent(wantType)));
			}
		}
		return value;
	}
}
