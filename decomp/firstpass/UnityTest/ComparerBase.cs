using System;
using UnityEngine;

namespace UnityTest;

public abstract class ComparerBase : ActionBase
{
	public enum CompareToType
	{
		CompareToObject,
		CompareToConstantValue,
		CompareToNull
	}

	public CompareToType compareToType;

	public GameObject other;

	protected object m_ObjOtherVal;

	public string otherPropertyPath = string.Empty;

	private MemberResolver m_MemberResolverB;

	public virtual object ConstValue { get; set; }

	protected abstract bool Compare(object a, object b);

	protected override bool Compare(object objValue)
	{
		if (compareToType == CompareToType.CompareToConstantValue)
		{
			m_ObjOtherVal = ConstValue;
		}
		else if (compareToType == CompareToType.CompareToNull)
		{
			m_ObjOtherVal = null;
		}
		else if (other == null)
		{
			m_ObjOtherVal = null;
		}
		else
		{
			if (m_MemberResolverB == null)
			{
				m_MemberResolverB = new MemberResolver(other, otherPropertyPath);
			}
			m_ObjOtherVal = m_MemberResolverB.GetValue(UseCache);
		}
		return Compare(objValue, m_ObjOtherVal);
	}

	public virtual Type[] GetAccepatbleTypesForB()
	{
		return null;
	}

	public virtual object GetDefaultConstValue()
	{
		throw new NotImplementedException();
	}

	public override string GetFailureMessage()
	{
		string text = GetType().Name + " assertion failed.\n" + go.name + "." + thisPropertyPath + " " + compareToType;
		string text2;
		switch (compareToType)
		{
		case CompareToType.CompareToObject:
			text2 = text;
			text = string.Concat(text2, " (", other, ").", otherPropertyPath, " failed.");
			break;
		case CompareToType.CompareToConstantValue:
			text2 = text;
			text = string.Concat(text2, " ", ConstValue, " failed.");
			break;
		case CompareToType.CompareToNull:
			text += " failed.";
			break;
		}
		text2 = text;
		return string.Concat(text2, " Expected: ", m_ObjOtherVal, " Actual: ", m_ObjVal);
	}
}
