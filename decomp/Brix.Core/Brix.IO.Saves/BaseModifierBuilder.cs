using System;
using System.Collections.Generic;
using Brix.IO.Saves.Modifiers;
using Newtonsoft.Json.Linq;

namespace Brix.IO.Saves;

public abstract class BaseModifierBuilder<BLDR, MODIFIER> where BLDR : BaseModifierBuilder<BLDR, MODIFIER> where MODIFIER : AbstractModifier, new()
{
	protected List<IJObjectLocalModifier> localModifiers = new List<IJObjectLocalModifier>();

	public BLDR Id(string fieldValue)
	{
		localModifiers.Add(new IdModifier(fieldValue));
		return (BLDR)this;
	}

	public BLDR ChangeFieldValue<T>(string fieldName, T fieldValue)
	{
		localModifiers.Add(new FieldValueModifier<T>(fieldName, fieldValue));
		return (BLDR)this;
	}

	public BLDR ChangeFieldValue(string fieldName, string fieldValue)
	{
		localModifiers.Add(new FieldValueModifier<string>(fieldName, fieldValue));
		return (BLDR)this;
	}

	public BLDR ChangeFieldName(string currentFieldName, string newFieldname)
	{
		localModifiers.Add(new FieldNameModifier(currentFieldName, newFieldname));
		return (BLDR)this;
	}

	public BLDR RemoveField(string fieldToDelete)
	{
		localModifiers.Add(new FieldRemover(fieldToDelete));
		return (BLDR)this;
	}

	public BLDR InjectJObject(string fieldToInjectValue, JObjectFetcher injectionFetcher)
	{
		localModifiers.Add(new JObjectFieldInjector(fieldToInjectValue, injectionFetcher));
		return (BLDR)this;
	}

	public BLDR InjectJObject(string fieldToInjectValue, JContainer jObjectToInject)
	{
		localModifiers.Add(new RawJObjectFieldInjector(fieldToInjectValue, jObjectToInject));
		return (BLDR)this;
	}

	public BLDR InjectJGameObject(string fieldToInjectValue, JGameObjectFetcher injectionFetcher)
	{
		localModifiers.Add(new JGameObjectFieldInjector(fieldToInjectValue, injectionFetcher));
		return (BLDR)this;
	}

	public BLDR InjectJGameObject(string fieldToInjectValue, JContainer jObjectToInject)
	{
		localModifiers.Add(new RawJGameObjectFieldInjector(fieldToInjectValue, jObjectToInject));
		return (BLDR)this;
	}

	public BLDR Modify(Action<JObject, JContainer> conversionFunction)
	{
		localModifiers.Add(new JObjectCustomModifier(conversionFunction));
		return (BLDR)this;
	}

	public MODIFIER Make()
	{
		return new MODIFIER
		{
			localModifiers = localModifiers
		};
	}
}
