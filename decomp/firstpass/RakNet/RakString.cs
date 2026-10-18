using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class RakString : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	public char this[int index]
	{
		get
		{
			return (char)OpArray((uint)index);
		}
		set
		{
			Replace((uint)index, 1u, (byte)value);
		}
	}

	internal RakString(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	public RakString()
		: this(RakNetPINVOKE.new_RakString__SWIG_0(), cMemoryOwn: true)
	{
	}

	public RakString(char input)
		: this(RakNetPINVOKE.new_RakString__SWIG_1(input), cMemoryOwn: true)
	{
	}

	public RakString(byte input)
		: this(RakNetPINVOKE.new_RakString__SWIG_2(input), cMemoryOwn: true)
	{
	}

	public RakString(string format)
		: this(RakNetPINVOKE.new_RakString__SWIG_3(format), cMemoryOwn: true)
	{
	}

	public RakString(RakString rhs)
		: this(RakNetPINVOKE.new_RakString__SWIG_4(getCPtr(rhs)), cMemoryOwn: true)
	{
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	internal static HandleRef getCPtr(RakString obj)
	{
		return (!(obj == null)) ? obj.swigCPtr : new HandleRef(null, IntPtr.Zero);
	}

	~RakString()
	{
		Dispose();
	}

	public virtual void Dispose()
	{
		lock (this)
		{
			if (swigCPtr.Handle != IntPtr.Zero)
			{
				if (swigCMemOwn)
				{
					swigCMemOwn = false;
					RakNetPINVOKE.delete_RakString(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
		}
	}

	public override int GetHashCode()
	{
		return C_String().GetHashCode();
	}

	public static bool operator ==(RakString a, RakString b)
	{
		if (object.ReferenceEquals(a, b))
		{
			return true;
		}
		if ((object)a == null || (object)b == null)
		{
			return false;
		}
		return a.Equals(b);
	}

	public static bool operator ==(RakString a, string b)
	{
		if (object.ReferenceEquals(a, b))
		{
			return true;
		}
		if ((object)a == null || b == null)
		{
			return false;
		}
		return a.Equals(b);
	}

	public static bool operator ==(RakString a, char b)
	{
		if (object.ReferenceEquals(a, b))
		{
			return true;
		}
		if ((object)a == null || (object)b == null)
		{
			return false;
		}
		return a.Equals(b);
	}

	public static bool operator !=(RakString a, char b)
	{
		return !(a == b);
	}

	public static bool operator !=(RakString a, RakString b)
	{
		return a.OpNotEqual(b);
	}

	public static bool operator !=(RakString a, string b)
	{
		return a.OpNotEqual(b);
	}

	public static bool operator <(RakString a, RakString b)
	{
		return a.OpLess(b);
	}

	public static bool operator >(RakString a, RakString b)
	{
		return a.OpGreater(b);
	}

	public static bool operator <=(RakString a, RakString b)
	{
		return a.OpLessEquals(b);
	}

	public static bool operator >=(RakString a, RakString b)
	{
		return a.OpGreaterEquals(b);
	}

	public static RakString operator +(RakString a, RakString b)
	{
		return RakNet.OpPlus(a, b);
	}

	public static implicit operator RakString(string s)
	{
		return new RakString(s);
	}

	public static implicit operator RakString(char c)
	{
		return new RakString(c);
	}

	public static implicit operator RakString(byte c)
	{
		return new RakString(c);
	}

	public override string ToString()
	{
		return C_String();
	}

	public void SetChar(uint index, char inChar)
	{
		SetChar(index, (byte)inChar);
	}

	public void Replace(uint index, uint count, char inChar)
	{
		Replace(index, count, (byte)inChar);
	}

	public string C_String()
	{
		return RakNetPINVOKE.RakString_C_String(swigCPtr);
	}

	public string C_StringUnsafe()
	{
		return RakNetPINVOKE.RakString_C_StringUnsafe(swigCPtr);
	}

	public RakString CopyData(RakString rhs)
	{
		RakString result = new RakString(RakNetPINVOKE.RakString_CopyData__SWIG_0(swigCPtr, getCPtr(rhs)), cMemoryOwn: false);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public RakString CopyData(string str)
	{
		return new RakString(RakNetPINVOKE.RakString_CopyData__SWIG_1(swigCPtr, str), cMemoryOwn: false);
	}

	public RakString CopyData(SWIGTYPE_p_unsigned_char str)
	{
		return new RakString(RakNetPINVOKE.RakString_CopyData__SWIG_2(swigCPtr, SWIGTYPE_p_unsigned_char.getCPtr(str)), cMemoryOwn: false);
	}

	public RakString CopyData(char c)
	{
		return new RakString(RakNetPINVOKE.RakString_CopyData__SWIG_4(swigCPtr, c), cMemoryOwn: false);
	}

	private byte OpArray(uint position)
	{
		return RakNetPINVOKE.RakString_OpArray(swigCPtr, position);
	}

	public uint Find(string stringToFind, uint pos)
	{
		return RakNetPINVOKE.RakString_Find__SWIG_0(swigCPtr, stringToFind, pos);
	}

	public uint Find(string stringToFind)
	{
		return RakNetPINVOKE.RakString_Find__SWIG_1(swigCPtr, stringToFind);
	}

	public bool Equals(RakString rhs)
	{
		bool result = RakNetPINVOKE.RakString_Equals__SWIG_0(swigCPtr, getCPtr(rhs));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool Equals(string str)
	{
		return RakNetPINVOKE.RakString_Equals__SWIG_1(swigCPtr, str);
	}

	private bool OpLess(RakString right)
	{
		bool result = RakNetPINVOKE.RakString_OpLess(swigCPtr, getCPtr(right));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private bool OpLessEquals(RakString right)
	{
		bool result = RakNetPINVOKE.RakString_OpLessEquals(swigCPtr, getCPtr(right));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private bool OpGreater(RakString right)
	{
		bool result = RakNetPINVOKE.RakString_OpGreater(swigCPtr, getCPtr(right));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private bool OpGreaterEquals(RakString right)
	{
		bool result = RakNetPINVOKE.RakString_OpGreaterEquals(swigCPtr, getCPtr(right));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private bool OpNotEqual(RakString rhs)
	{
		bool result = RakNetPINVOKE.RakString_OpNotEqual__SWIG_0(swigCPtr, getCPtr(rhs));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private bool OpNotEqual(string str)
	{
		return RakNetPINVOKE.RakString_OpNotEqual__SWIG_1(swigCPtr, str);
	}

	public string ToLower()
	{
		return RakNetPINVOKE.RakString_ToLower(swigCPtr);
	}

	public string ToUpper()
	{
		return RakNetPINVOKE.RakString_ToUpper(swigCPtr);
	}

	public void Set(string format)
	{
		RakNetPINVOKE.RakString_Set(swigCPtr, format);
	}

	public RakString Assign(string str, uint pos, uint n)
	{
		return new RakString(RakNetPINVOKE.RakString_Assign(swigCPtr, str, pos, n), cMemoryOwn: true);
	}

	public bool IsEmpty()
	{
		return RakNetPINVOKE.RakString_IsEmpty(swigCPtr);
	}

	public uint GetLength()
	{
		return RakNetPINVOKE.RakString_GetLength(swigCPtr);
	}

	public uint GetLengthUTF8()
	{
		return RakNetPINVOKE.RakString_GetLengthUTF8(swigCPtr);
	}

	public void Replace(uint index, uint count, byte c)
	{
		RakNetPINVOKE.RakString_Replace(swigCPtr, index, count, c);
	}

	public void SetChar(uint index, byte c)
	{
		RakNetPINVOKE.RakString_SetChar__SWIG_0(swigCPtr, index, c);
	}

	public void SetChar(uint index, RakString s)
	{
		RakNetPINVOKE.RakString_SetChar__SWIG_1(swigCPtr, index, getCPtr(s));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void Truncate(uint length)
	{
		RakNetPINVOKE.RakString_Truncate(swigCPtr, length);
	}

	public void TruncateUTF8(uint length)
	{
		RakNetPINVOKE.RakString_TruncateUTF8(swigCPtr, length);
	}

	public RakString SubStr(uint index, uint count)
	{
		return new RakString(RakNetPINVOKE.RakString_SubStr(swigCPtr, index, count), cMemoryOwn: true);
	}

	public void Erase(uint index, uint count)
	{
		RakNetPINVOKE.RakString_Erase(swigCPtr, index, count);
	}

	public void TerminateAtFirstCharacter(char c)
	{
		RakNetPINVOKE.RakString_TerminateAtFirstCharacter(swigCPtr, c);
	}

	public void TerminateAtLastCharacter(char c)
	{
		RakNetPINVOKE.RakString_TerminateAtLastCharacter(swigCPtr, c);
	}

	public void StartAfterFirstCharacter(char c)
	{
		RakNetPINVOKE.RakString_StartAfterFirstCharacter(swigCPtr, c);
	}

	public void StartAfterLastCharacter(char c)
	{
		RakNetPINVOKE.RakString_StartAfterLastCharacter(swigCPtr, c);
	}

	public int GetCharacterCount(char c)
	{
		return RakNetPINVOKE.RakString_GetCharacterCount(swigCPtr, c);
	}

	public void RemoveCharacter(char c)
	{
		RakNetPINVOKE.RakString_RemoveCharacter(swigCPtr, c);
	}

	public static RakString NonVariadic(string str)
	{
		return new RakString(RakNetPINVOKE.RakString_NonVariadic(str), cMemoryOwn: true);
	}

	public static uint ToInteger(string str)
	{
		return RakNetPINVOKE.RakString_ToInteger__SWIG_0(str);
	}

	public static uint ToInteger(RakString rs)
	{
		uint result = RakNetPINVOKE.RakString_ToInteger__SWIG_1(getCPtr(rs));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public static int ReadIntFromSubstring(string str, uint pos, uint n)
	{
		return RakNetPINVOKE.RakString_ReadIntFromSubstring(str, pos, n);
	}

	public int StrCmp(RakString rhs)
	{
		int result = RakNetPINVOKE.RakString_StrCmp(swigCPtr, getCPtr(rhs));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public int StrNCmp(RakString rhs, uint num)
	{
		int result = RakNetPINVOKE.RakString_StrNCmp(swigCPtr, getCPtr(rhs), num);
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public int StrICmp(RakString rhs)
	{
		int result = RakNetPINVOKE.RakString_StrICmp(swigCPtr, getCPtr(rhs));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void Clear()
	{
		RakNetPINVOKE.RakString_Clear(swigCPtr);
	}

	public void Printf()
	{
		RakNetPINVOKE.RakString_Printf(swigCPtr);
	}

	public bool IPAddressMatch(string IP)
	{
		return RakNetPINVOKE.RakString_IPAddressMatch(swigCPtr, IP);
	}

	public bool ContainsNonprintableExceptSpaces()
	{
		return RakNetPINVOKE.RakString_ContainsNonprintableExceptSpaces(swigCPtr);
	}

	public bool IsEmailAddress()
	{
		return RakNetPINVOKE.RakString_IsEmailAddress(swigCPtr);
	}

	public RakString URLEncode()
	{
		return new RakString(RakNetPINVOKE.RakString_URLEncode(swigCPtr), cMemoryOwn: false);
	}

	public RakString URLDecode()
	{
		return new RakString(RakNetPINVOKE.RakString_URLDecode(swigCPtr), cMemoryOwn: false);
	}

	public void SplitURI(RakString header, RakString domain, RakString path)
	{
		RakNetPINVOKE.RakString_SplitURI(swigCPtr, getCPtr(header), getCPtr(domain), getCPtr(path));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public RakString SQLEscape()
	{
		return new RakString(RakNetPINVOKE.RakString_SQLEscape(swigCPtr), cMemoryOwn: false);
	}

	public static RakString FormatForPOST(string uri, string contentType, string body, string extraHeaders)
	{
		return new RakString(RakNetPINVOKE.RakString_FormatForPOST__SWIG_0(uri, contentType, body, extraHeaders), cMemoryOwn: true);
	}

	public static RakString FormatForPOST(string uri, string contentType, string body)
	{
		return new RakString(RakNetPINVOKE.RakString_FormatForPOST__SWIG_1(uri, contentType, body), cMemoryOwn: true);
	}

	public static RakString FormatForPUT(string uri, string contentType, string body, string extraHeaders)
	{
		return new RakString(RakNetPINVOKE.RakString_FormatForPUT__SWIG_0(uri, contentType, body, extraHeaders), cMemoryOwn: true);
	}

	public static RakString FormatForPUT(string uri, string contentType, string body)
	{
		return new RakString(RakNetPINVOKE.RakString_FormatForPUT__SWIG_1(uri, contentType, body), cMemoryOwn: true);
	}

	public static RakString FormatForGET(string uri, string extraHeaders)
	{
		return new RakString(RakNetPINVOKE.RakString_FormatForGET__SWIG_0(uri, extraHeaders), cMemoryOwn: true);
	}

	public static RakString FormatForGET(string uri)
	{
		return new RakString(RakNetPINVOKE.RakString_FormatForGET__SWIG_1(uri), cMemoryOwn: true);
	}

	public static RakString FormatForDELETE(string uri, string extraHeaders)
	{
		return new RakString(RakNetPINVOKE.RakString_FormatForDELETE__SWIG_0(uri, extraHeaders), cMemoryOwn: true);
	}

	public static RakString FormatForDELETE(string uri)
	{
		return new RakString(RakNetPINVOKE.RakString_FormatForDELETE__SWIG_1(uri), cMemoryOwn: true);
	}

	public RakString MakeFilePath()
	{
		return new RakString(RakNetPINVOKE.RakString_MakeFilePath(swigCPtr), cMemoryOwn: false);
	}

	public static void FreeMemory()
	{
		RakNetPINVOKE.RakString_FreeMemory();
	}

	public static void FreeMemoryNoMutex()
	{
		RakNetPINVOKE.RakString_FreeMemoryNoMutex();
	}

	public void Serialize(BitStream bs)
	{
		RakNetPINVOKE.RakString_Serialize__SWIG_0(swigCPtr, BitStream.getCPtr(bs));
	}

	public static void Serialize(string str, BitStream bs)
	{
		RakNetPINVOKE.RakString_Serialize__SWIG_1(str, BitStream.getCPtr(bs));
	}

	public void SerializeCompressed(BitStream bs, byte languageId, bool writeLanguageId)
	{
		RakNetPINVOKE.RakString_SerializeCompressed__SWIG_0(swigCPtr, BitStream.getCPtr(bs), languageId, writeLanguageId);
	}

	public void SerializeCompressed(BitStream bs, byte languageId)
	{
		RakNetPINVOKE.RakString_SerializeCompressed__SWIG_1(swigCPtr, BitStream.getCPtr(bs), languageId);
	}

	public void SerializeCompressed(BitStream bs)
	{
		RakNetPINVOKE.RakString_SerializeCompressed__SWIG_2(swigCPtr, BitStream.getCPtr(bs));
	}

	public static void SerializeCompressed(string str, BitStream bs, byte languageId, bool writeLanguageId)
	{
		RakNetPINVOKE.RakString_SerializeCompressed__SWIG_3(str, BitStream.getCPtr(bs), languageId, writeLanguageId);
	}

	public static void SerializeCompressed(string str, BitStream bs, byte languageId)
	{
		RakNetPINVOKE.RakString_SerializeCompressed__SWIG_4(str, BitStream.getCPtr(bs), languageId);
	}

	public static void SerializeCompressed(string str, BitStream bs)
	{
		RakNetPINVOKE.RakString_SerializeCompressed__SWIG_5(str, BitStream.getCPtr(bs));
	}

	public bool Deserialize(BitStream bs)
	{
		return RakNetPINVOKE.RakString_Deserialize__SWIG_0(swigCPtr, BitStream.getCPtr(bs));
	}

	public static bool Deserialize(string str, BitStream bs)
	{
		return RakNetPINVOKE.RakString_Deserialize__SWIG_1(str, BitStream.getCPtr(bs));
	}

	public bool DeserializeCompressed(BitStream bs, bool readLanguageId)
	{
		return RakNetPINVOKE.RakString_DeserializeCompressed__SWIG_0(swigCPtr, BitStream.getCPtr(bs), readLanguageId);
	}

	public bool DeserializeCompressed(BitStream bs)
	{
		return RakNetPINVOKE.RakString_DeserializeCompressed__SWIG_1(swigCPtr, BitStream.getCPtr(bs));
	}

	public static bool DeserializeCompressed(string str, BitStream bs, bool readLanguageId)
	{
		return RakNetPINVOKE.RakString_DeserializeCompressed__SWIG_2(str, BitStream.getCPtr(bs), readLanguageId);
	}

	public static bool DeserializeCompressed(string str, BitStream bs)
	{
		return RakNetPINVOKE.RakString_DeserializeCompressed__SWIG_3(str, BitStream.getCPtr(bs));
	}

	public static string ToString(long i)
	{
		return RakNetPINVOKE.RakString_ToString__SWIG_0(i);
	}

	public static string ToString(ulong i)
	{
		return RakNetPINVOKE.RakString_ToString__SWIG_1(i);
	}

	public static uint GetSizeToAllocate(uint bytes)
	{
		return RakNetPINVOKE.RakString_GetSizeToAllocate(bytes);
	}

	public static int RakStringComp(RakString key, RakString data)
	{
		int result = RakNetPINVOKE.RakString_RakStringComp(getCPtr(key), getCPtr(data));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public static void LockMutex()
	{
		RakNetPINVOKE.RakString_LockMutex();
	}

	public static void UnlockMutex()
	{
		RakNetPINVOKE.RakString_UnlockMutex();
	}

	public void AppendBytes(byte[] inByteArray, uint count)
	{
		RakNetPINVOKE.RakString_AppendBytes(swigCPtr, inByteArray, count);
	}
}
