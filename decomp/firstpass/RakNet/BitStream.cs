using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class BitStream : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	internal BitStream(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	public BitStream()
		: this(RakNetPINVOKE.new_BitStream__SWIG_0(), cMemoryOwn: true)
	{
	}

	public BitStream(uint initialBytesToAllocate)
		: this(RakNetPINVOKE.new_BitStream__SWIG_1(initialBytesToAllocate), cMemoryOwn: true)
	{
	}

	public BitStream(byte[] _data, uint lengthInBytes, bool _copyData)
		: this(RakNetPINVOKE.new_BitStream__SWIG_2(_data, lengthInBytes, _copyData), cMemoryOwn: true)
	{
	}

	internal static HandleRef getCPtr(BitStream obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~BitStream()
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
					RakNetPINVOKE.delete_BitStream(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
		}
	}

	public bool Read(out string varString)
	{
		string inString = new string('c', (int)GetNumberOfUnreadBits() / 8);
		varString = CSharpStringReader(inString);
		return varString != string.Empty;
	}

	public bool Read(byte[] inOutByteArray, uint numberOfBytes)
	{
		return CSharpByteReader(inOutByteArray, numberOfBytes);
	}

	public bool Read(out char outTemplateVar)
	{
		byte outTemplateVar2;
		bool result = Read(out outTemplateVar2);
		outTemplateVar = (char)outTemplateVar2;
		return result;
	}

	public bool ReadCompressed(out char outTemplateVar)
	{
		byte outTemplateVar2;
		bool result = ReadCompressed(out outTemplateVar2);
		outTemplateVar = (char)outTemplateVar2;
		return result;
	}

	public bool ReadCompressedDelta(out char outTemplateVar)
	{
		byte outTemplateVar2;
		bool result = ReadCompressedDelta(out outTemplateVar2);
		outTemplateVar = (char)outTemplateVar2;
		return result;
	}

	public bool ReadDelta(out char outTemplateVar)
	{
		byte outTemplateVar2;
		bool result = ReadDelta(out outTemplateVar2);
		outTemplateVar = (char)outTemplateVar2;
		return result;
	}

	public bool ReadCompressed(out string var)
	{
		string inString = new string('c', (int)GetNumberOfUnreadBits() / 8);
		var = CSharpStringReaderCompressed(inString);
		return var != string.Empty;
	}

	public bool ReadCompressedDelta(out string var)
	{
		string inString = new string('c', (int)GetNumberOfUnreadBits() / 8);
		var = CSharpStringReaderCompressedDelta(inString);
		return var != string.Empty;
	}

	public bool ReadDelta(out string var)
	{
		string inString = new string('c', (int)GetNumberOfUnreadBits() / 8);
		var = CSharpStringReaderDelta(inString);
		return var != string.Empty;
	}

	public uint CopyData(out byte[] outByteArray)
	{
		byte[] array = new byte[GetNumberOfBitsAllocated() / 8];
		uint result = CSharpCopyDataHelper(array);
		outByteArray = array;
		return result;
	}

	public byte[] GetData()
	{
		byte[] array = new byte[GetNumberOfBitsAllocated() / 8];
		CSharpCopyDataHelper(array);
		return array;
	}

	public void PrintBits(out string var)
	{
		string inString = new string('c', (int)(GetNumberOfBitsAllocated() + GetNumberOfBitsAllocated() / 8));
		var = CSharpPrintBitsHelper(inString);
	}

	public void PrintHex(out string var)
	{
		string inString = new string('c', (int)(GetNumberOfBitsAllocated() / 4 + GetNumberOfBitsAllocated() / 8));
		var = CSharpPrintHexHelper(inString);
	}

	public bool Serialize(bool WriteToBitstream, ref char inOutTemplateVar)
	{
		byte inOutTemplateVar2 = (byte)inOutTemplateVar;
		bool result = Serialize(WriteToBitstream, ref inOutTemplateVar2);
		inOutTemplateVar = (char)inOutTemplateVar2;
		return result;
	}

	public bool SerializeDelta(bool WriteToBitstream, ref char inOutTemplateVar)
	{
		byte inOutCurrentValue = (byte)inOutTemplateVar;
		bool result = SerializeDelta(WriteToBitstream, ref inOutCurrentValue);
		inOutTemplateVar = (char)inOutCurrentValue;
		return result;
	}

	public bool SerializeCompressed(bool WriteToBitstream, ref char inOutTemplateVar)
	{
		byte inOutTemplateVar2 = (byte)inOutTemplateVar;
		bool result = SerializeCompressed(WriteToBitstream, ref inOutTemplateVar2);
		inOutTemplateVar = (char)inOutTemplateVar2;
		return result;
	}

	public bool SerializeCompressedDelta(bool WriteToBitstream, ref char inOutTemplateVar)
	{
		byte inOutTemplateVar2 = (byte)inOutTemplateVar;
		bool result = SerializeCompressedDelta(WriteToBitstream, ref inOutTemplateVar2);
		inOutTemplateVar = (char)inOutTemplateVar2;
		return result;
	}

	public bool ReadAlignedBytesSafeAlloc(out byte[] outByteArray, int inputLength, int maxBytesToRead)
	{
		outByteArray = new byte[inputLength];
		return ReadAlignedBytesSafe(outByteArray, inputLength, maxBytesToRead);
	}

	public bool ReadAlignedBytesSafeAlloc(out byte[] outByteArray, uint inputLength, uint maxBytesToRead)
	{
		outByteArray = new byte[inputLength];
		return ReadAlignedBytesSafe(outByteArray, inputLength, maxBytesToRead);
	}

	public static BitStream GetInstance()
	{
		IntPtr intPtr = RakNetPINVOKE.BitStream_GetInstance();
		return (!(intPtr == IntPtr.Zero)) ? new BitStream(intPtr, cMemoryOwn: false) : null;
	}

	public static void DestroyInstance(BitStream i)
	{
		RakNetPINVOKE.BitStream_DestroyInstance(getCPtr(i));
	}

	public void Reset()
	{
		RakNetPINVOKE.BitStream_Reset(swigCPtr);
	}

	public bool SerializeFloat16(bool writeToBitstream, ref float inOutFloat, float floatMin, float floatMax)
	{
		return RakNetPINVOKE.BitStream_SerializeFloat16(swigCPtr, writeToBitstream, ref inOutFloat, floatMin, floatMax);
	}

	public bool SerializeBits(bool writeToBitstream, byte[] inOutByteArray, uint numberOfBitsToSerialize, bool rightAlignedBits)
	{
		return RakNetPINVOKE.BitStream_SerializeBits__SWIG_0(swigCPtr, writeToBitstream, inOutByteArray, numberOfBitsToSerialize, rightAlignedBits);
	}

	public bool SerializeBits(bool writeToBitstream, byte[] inOutByteArray, uint numberOfBitsToSerialize)
	{
		return RakNetPINVOKE.BitStream_SerializeBits__SWIG_1(swigCPtr, writeToBitstream, inOutByteArray, numberOfBitsToSerialize);
	}

	public bool Read(BitStream bitStream, uint numberOfBits)
	{
		return RakNetPINVOKE.BitStream_Read__SWIG_1(swigCPtr, getCPtr(bitStream), numberOfBits);
	}

	public bool Read(BitStream bitStream)
	{
		return RakNetPINVOKE.BitStream_Read__SWIG_2(swigCPtr, getCPtr(bitStream));
	}

	public void Write(BitStream bitStream, uint numberOfBits)
	{
		RakNetPINVOKE.BitStream_Write__SWIG_1(swigCPtr, getCPtr(bitStream), numberOfBits);
	}

	public void Write(BitStream bitStream)
	{
		RakNetPINVOKE.BitStream_Write__SWIG_2(swigCPtr, getCPtr(bitStream));
	}

	public void WriteFloat16(float x, float floatMin, float floatMax)
	{
		RakNetPINVOKE.BitStream_WriteFloat16(swigCPtr, x, floatMin, floatMax);
	}

	public bool ReadFloat16(out float outFloat, float floatMin, float floatMax)
	{
		return RakNetPINVOKE.BitStream_ReadFloat16(swigCPtr, out outFloat, floatMin, floatMax);
	}

	public void ResetReadPointer()
	{
		RakNetPINVOKE.BitStream_ResetReadPointer(swigCPtr);
	}

	public void ResetWritePointer()
	{
		RakNetPINVOKE.BitStream_ResetWritePointer(swigCPtr);
	}

	public void AssertStreamEmpty()
	{
		RakNetPINVOKE.BitStream_AssertStreamEmpty(swigCPtr);
	}

	public void PrintBits()
	{
		RakNetPINVOKE.BitStream_PrintBits(swigCPtr);
	}

	public void PrintHex()
	{
		RakNetPINVOKE.BitStream_PrintHex(swigCPtr);
	}

	public void IgnoreBits(uint numberOfBits)
	{
		RakNetPINVOKE.BitStream_IgnoreBits(swigCPtr, numberOfBits);
	}

	public void IgnoreBytes(uint numberOfBytes)
	{
		RakNetPINVOKE.BitStream_IgnoreBytes(swigCPtr, numberOfBytes);
	}

	public void SetWriteOffset(uint offset)
	{
		RakNetPINVOKE.BitStream_SetWriteOffset(swigCPtr, offset);
	}

	public uint GetNumberOfBitsUsed()
	{
		return RakNetPINVOKE.BitStream_GetNumberOfBitsUsed(swigCPtr);
	}

	public uint GetWriteOffset()
	{
		return RakNetPINVOKE.BitStream_GetWriteOffset(swigCPtr);
	}

	public uint GetNumberOfBytesUsed()
	{
		return RakNetPINVOKE.BitStream_GetNumberOfBytesUsed(swigCPtr);
	}

	public uint GetReadOffset()
	{
		return RakNetPINVOKE.BitStream_GetReadOffset(swigCPtr);
	}

	public void SetReadOffset(uint newReadOffset)
	{
		RakNetPINVOKE.BitStream_SetReadOffset(swigCPtr, newReadOffset);
	}

	public uint GetNumberOfUnreadBits()
	{
		return RakNetPINVOKE.BitStream_GetNumberOfUnreadBits(swigCPtr);
	}

	public void SetData(byte[] inByteArray)
	{
		RakNetPINVOKE.BitStream_SetData(swigCPtr, inByteArray);
	}

	public void WriteBits(byte[] inByteArray, uint numberOfBitsToWrite, bool rightAlignedBits)
	{
		RakNetPINVOKE.BitStream_WriteBits__SWIG_0(swigCPtr, inByteArray, numberOfBitsToWrite, rightAlignedBits);
	}

	public void WriteBits(byte[] inByteArray, uint numberOfBitsToWrite)
	{
		RakNetPINVOKE.BitStream_WriteBits__SWIG_1(swigCPtr, inByteArray, numberOfBitsToWrite);
	}

	public void WriteAlignedBytes(byte[] inByteArray, uint numberOfBytesToWrite)
	{
		RakNetPINVOKE.BitStream_WriteAlignedBytes(swigCPtr, inByteArray, numberOfBytesToWrite);
	}

	public void EndianSwapBytes(int byteOffset, int length)
	{
		RakNetPINVOKE.BitStream_EndianSwapBytes(swigCPtr, byteOffset, length);
	}

	public bool ReadAlignedBytes(byte[] inOutByteArray, uint numberOfBytesToRead)
	{
		return RakNetPINVOKE.BitStream_ReadAlignedBytes(swigCPtr, inOutByteArray, numberOfBytesToRead);
	}

	public void AlignWriteToByteBoundary()
	{
		RakNetPINVOKE.BitStream_AlignWriteToByteBoundary(swigCPtr);
	}

	public void AlignReadToByteBoundary()
	{
		RakNetPINVOKE.BitStream_AlignReadToByteBoundary(swigCPtr);
	}

	public bool ReadBits(byte[] inOutByteArray, uint numberOfBitsToRead, bool alignBitsToRight)
	{
		return RakNetPINVOKE.BitStream_ReadBits__SWIG_0(swigCPtr, inOutByteArray, numberOfBitsToRead, alignBitsToRight);
	}

	public bool ReadBits(byte[] inOutByteArray, uint numberOfBitsToRead)
	{
		return RakNetPINVOKE.BitStream_ReadBits__SWIG_1(swigCPtr, inOutByteArray, numberOfBitsToRead);
	}

	public void Write0()
	{
		RakNetPINVOKE.BitStream_Write0(swigCPtr);
	}

	public void Write1()
	{
		RakNetPINVOKE.BitStream_Write1(swigCPtr);
	}

	public bool ReadBit()
	{
		return RakNetPINVOKE.BitStream_ReadBit(swigCPtr);
	}

	public void AssertCopyData()
	{
		RakNetPINVOKE.BitStream_AssertCopyData(swigCPtr);
	}

	public void SetNumberOfBitsAllocated(uint lengthInBits)
	{
		RakNetPINVOKE.BitStream_SetNumberOfBitsAllocated(swigCPtr, lengthInBits);
	}

	public void AddBitsAndReallocate(uint numberOfBitsToWrite)
	{
		RakNetPINVOKE.BitStream_AddBitsAndReallocate(swigCPtr, numberOfBitsToWrite);
	}

	public uint GetNumberOfBitsAllocated()
	{
		return RakNetPINVOKE.BitStream_GetNumberOfBitsAllocated(swigCPtr);
	}

	public void PadWithZeroToByteLength(uint bytes)
	{
		RakNetPINVOKE.BitStream_PadWithZeroToByteLength(swigCPtr, bytes);
	}

	public static int NumberOfLeadingZeroes(byte x)
	{
		return RakNetPINVOKE.BitStream_NumberOfLeadingZeroes__SWIG_0(x);
	}

	public static int NumberOfLeadingZeroes(ushort x)
	{
		return RakNetPINVOKE.BitStream_NumberOfLeadingZeroes__SWIG_1(x);
	}

	public static int NumberOfLeadingZeroes(uint x)
	{
		return RakNetPINVOKE.BitStream_NumberOfLeadingZeroes__SWIG_2(x);
	}

	public static int NumberOfLeadingZeroes(ulong x)
	{
		return RakNetPINVOKE.BitStream_NumberOfLeadingZeroes__SWIG_3(x);
	}

	public static int NumberOfLeadingZeroes(sbyte x)
	{
		return RakNetPINVOKE.BitStream_NumberOfLeadingZeroes__SWIG_4(x);
	}

	public static int NumberOfLeadingZeroes(short x)
	{
		return RakNetPINVOKE.BitStream_NumberOfLeadingZeroes__SWIG_5(x);
	}

	public static int NumberOfLeadingZeroes(int x)
	{
		return RakNetPINVOKE.BitStream_NumberOfLeadingZeroes__SWIG_6(x);
	}

	public static int NumberOfLeadingZeroes(long x)
	{
		return RakNetPINVOKE.BitStream_NumberOfLeadingZeroes__SWIG_7(x);
	}

	public void Write(string inStringVar)
	{
		RakNetPINVOKE.BitStream_Write__SWIG_3(swigCPtr, inStringVar);
	}

	public void Write(SWIGTYPE_p_wchar_t inStringVar)
	{
		RakNetPINVOKE.BitStream_Write__SWIG_4(swigCPtr, SWIGTYPE_p_wchar_t.getCPtr(inStringVar));
	}

	public void WriteCompressed(string inStringVar)
	{
		RakNetPINVOKE.BitStream_WriteCompressed__SWIG_1(swigCPtr, inStringVar);
	}

	public void WriteCompressed(SWIGTYPE_p_wchar_t inStringVar)
	{
		RakNetPINVOKE.BitStream_WriteCompressed__SWIG_2(swigCPtr, SWIGTYPE_p_wchar_t.getCPtr(inStringVar));
	}

	public static bool DoEndianSwap()
	{
		return RakNetPINVOKE.BitStream_DoEndianSwap();
	}

	public static bool IsBigEndian()
	{
		return RakNetPINVOKE.BitStream_IsBigEndian();
	}

	public static bool IsNetworkOrder()
	{
		return RakNetPINVOKE.BitStream_IsNetworkOrder();
	}

	public static bool IsNetworkOrderInternal()
	{
		return RakNetPINVOKE.BitStream_IsNetworkOrderInternal();
	}

	public static void ReverseBytes(byte[] inByteArray, byte[] inOutByteArray, uint length)
	{
		RakNetPINVOKE.BitStream_ReverseBytes(inByteArray, inOutByteArray, length);
	}

	public static void ReverseBytesInPlace(byte[] inOutData, uint length)
	{
		RakNetPINVOKE.BitStream_ReverseBytesInPlace(inOutData, length);
	}

	private string CSharpStringReader(string inString)
	{
		return RakNetPINVOKE.BitStream_CSharpStringReader(swigCPtr, inString);
	}

	private bool CSharpByteReader(byte[] inOutByteArray, uint numberOfBytes)
	{
		return RakNetPINVOKE.BitStream_CSharpByteReader(swigCPtr, inOutByteArray, numberOfBytes);
	}

	private string CSharpStringReaderCompressedDelta(string inString)
	{
		return RakNetPINVOKE.BitStream_CSharpStringReaderCompressedDelta(swigCPtr, inString);
	}

	private string CSharpStringReaderDelta(string inString)
	{
		return RakNetPINVOKE.BitStream_CSharpStringReaderDelta(swigCPtr, inString);
	}

	private string CSharpStringReaderCompressed(string inString)
	{
		return RakNetPINVOKE.BitStream_CSharpStringReaderCompressed(swigCPtr, inString);
	}

	public void Write(byte[] inputByteArray, uint numberOfBytes)
	{
		RakNetPINVOKE.BitStream_Write__SWIG_6(swigCPtr, inputByteArray, numberOfBytes);
	}

	private uint CSharpCopyDataHelper(byte[] inOutByteArray)
	{
		return RakNetPINVOKE.BitStream_CSharpCopyDataHelper(swigCPtr, inOutByteArray);
	}

	private string CSharpPrintBitsHelper(string inString)
	{
		return RakNetPINVOKE.BitStream_CSharpPrintBitsHelper(swigCPtr, inString);
	}

	private string CSharpPrintHexHelper(string inString)
	{
		return RakNetPINVOKE.BitStream_CSharpPrintHexHelper(swigCPtr, inString);
	}

	public void Serialize(bool writeToBitstream, byte[] inputByteArray, uint numberOfBytes)
	{
		RakNetPINVOKE.BitStream_Serialize__SWIG_1(swigCPtr, writeToBitstream, inputByteArray, numberOfBytes);
	}

	public bool ReadAlignedBytesSafe(byte[] inOutByteArray, int inputLength, int maxBytesToRead)
	{
		return RakNetPINVOKE.BitStream_ReadAlignedBytesSafe__SWIG_0(swigCPtr, inOutByteArray, inputLength, maxBytesToRead);
	}

	public bool ReadAlignedBytesSafe(byte[] inOutByteArray, uint inputLength, uint maxBytesToRead)
	{
		return RakNetPINVOKE.BitStream_ReadAlignedBytesSafe__SWIG_1(swigCPtr, inOutByteArray, inputLength, maxBytesToRead);
	}

	public void WriteAlignedVar8(byte[] inByteArray)
	{
		RakNetPINVOKE.BitStream_WriteAlignedVar8(swigCPtr, inByteArray);
	}

	public bool ReadAlignedVar8(byte[] inOutByteArray)
	{
		return RakNetPINVOKE.BitStream_ReadAlignedVar8(swigCPtr, inOutByteArray);
	}

	public void WriteAlignedVar16(byte[] inByteArray)
	{
		RakNetPINVOKE.BitStream_WriteAlignedVar16(swigCPtr, inByteArray);
	}

	public bool ReadAlignedVar16(byte[] inOutByteArray)
	{
		return RakNetPINVOKE.BitStream_ReadAlignedVar16(swigCPtr, inOutByteArray);
	}

	public void WriteAlignedVar32(byte[] inByteArray)
	{
		RakNetPINVOKE.BitStream_WriteAlignedVar32(swigCPtr, inByteArray);
	}

	public bool ReadAlignedVar32(byte[] inOutByteArray)
	{
		return RakNetPINVOKE.BitStream_ReadAlignedVar32(swigCPtr, inOutByteArray);
	}

	public void WriteAlignedBytesSafe(byte[] inByteArray, uint inputLength, uint maxBytesToWrite)
	{
		RakNetPINVOKE.BitStream_WriteAlignedBytesSafe(swigCPtr, inByteArray, inputLength, maxBytesToWrite);
	}

	public bool Serialize(bool writeToBitstream, ref bool inOutTemplateVar)
	{
		return RakNetPINVOKE.BitStream_Serialize__SWIG_2(swigCPtr, writeToBitstream, ref inOutTemplateVar);
	}

	public bool Serialize(bool writeToBitstream, ref byte inOutTemplateVar)
	{
		return RakNetPINVOKE.BitStream_Serialize__SWIG_3(swigCPtr, writeToBitstream, ref inOutTemplateVar);
	}

	public bool Serialize(bool writeToBitstream, ref short inOutTemplateVar)
	{
		return RakNetPINVOKE.BitStream_Serialize__SWIG_4(swigCPtr, writeToBitstream, ref inOutTemplateVar);
	}

	public bool Serialize(bool writeToBitstream, ref ushort inOutTemplateVar)
	{
		return RakNetPINVOKE.BitStream_Serialize__SWIG_5(swigCPtr, writeToBitstream, ref inOutTemplateVar);
	}

	public bool Serialize(bool writeToBitstream, ref int inOutTemplateVar)
	{
		return RakNetPINVOKE.BitStream_Serialize__SWIG_6(swigCPtr, writeToBitstream, ref inOutTemplateVar);
	}

	public bool Serialize(bool writeToBitstream, ref long inOutTemplateVar)
	{
		return RakNetPINVOKE.BitStream_Serialize__SWIG_7(swigCPtr, writeToBitstream, ref inOutTemplateVar);
	}

	public bool Serialize(bool writeToBitstream, ref float inOutTemplateVar)
	{
		return RakNetPINVOKE.BitStream_Serialize__SWIG_8(swigCPtr, writeToBitstream, ref inOutTemplateVar);
	}

	public bool Serialize(bool writeToBitstream, RakString inOutTemplateVar)
	{
		bool result = RakNetPINVOKE.BitStream_Serialize__SWIG_9(swigCPtr, writeToBitstream, RakString.getCPtr(inOutTemplateVar));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool Serialize(bool writeToBitstream, RakNetGUID inOutTemplateVar)
	{
		bool result = RakNetPINVOKE.BitStream_Serialize__SWIG_10(swigCPtr, writeToBitstream, RakNetGUID.getCPtr(inOutTemplateVar));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool Serialize(bool writeToBitstream, uint24_t inOutTemplateVar)
	{
		bool result = RakNetPINVOKE.BitStream_Serialize__SWIG_11(swigCPtr, writeToBitstream, uint24_t.getCPtr(inOutTemplateVar));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool SerializeDelta(bool writeToBitstream, ref bool inOutCurrentValue, bool lastValue)
	{
		return RakNetPINVOKE.BitStream_SerializeDelta__SWIG_2(swigCPtr, writeToBitstream, ref inOutCurrentValue, lastValue);
	}

	public bool SerializeDelta(bool writeToBitstream, ref bool inOutCurrentValue)
	{
		return RakNetPINVOKE.BitStream_SerializeDelta__SWIG_3(swigCPtr, writeToBitstream, ref inOutCurrentValue);
	}

	public bool SerializeDelta(bool writeToBitstream, ref byte inOutCurrentValue, byte lastValue)
	{
		return RakNetPINVOKE.BitStream_SerializeDelta__SWIG_4(swigCPtr, writeToBitstream, ref inOutCurrentValue, lastValue);
	}

	public bool SerializeDelta(bool writeToBitstream, ref byte inOutCurrentValue)
	{
		return RakNetPINVOKE.BitStream_SerializeDelta__SWIG_5(swigCPtr, writeToBitstream, ref inOutCurrentValue);
	}

	public bool SerializeDelta(bool writeToBitstream, ref short inOutCurrentValue, short lastValue)
	{
		return RakNetPINVOKE.BitStream_SerializeDelta__SWIG_6(swigCPtr, writeToBitstream, ref inOutCurrentValue, lastValue);
	}

	public bool SerializeDelta(bool writeToBitstream, ref short inOutCurrentValue)
	{
		return RakNetPINVOKE.BitStream_SerializeDelta__SWIG_7(swigCPtr, writeToBitstream, ref inOutCurrentValue);
	}

	public bool SerializeDelta(bool writeToBitstream, ref ushort inOutCurrentValue, ushort lastValue)
	{
		return RakNetPINVOKE.BitStream_SerializeDelta__SWIG_8(swigCPtr, writeToBitstream, ref inOutCurrentValue, lastValue);
	}

	public bool SerializeDelta(bool writeToBitstream, ref ushort inOutCurrentValue)
	{
		return RakNetPINVOKE.BitStream_SerializeDelta__SWIG_9(swigCPtr, writeToBitstream, ref inOutCurrentValue);
	}

	public bool SerializeDelta(bool writeToBitstream, ref int inOutCurrentValue, int lastValue)
	{
		return RakNetPINVOKE.BitStream_SerializeDelta__SWIG_10(swigCPtr, writeToBitstream, ref inOutCurrentValue, lastValue);
	}

	public bool SerializeDelta(bool writeToBitstream, ref int inOutCurrentValue)
	{
		return RakNetPINVOKE.BitStream_SerializeDelta__SWIG_11(swigCPtr, writeToBitstream, ref inOutCurrentValue);
	}

	public bool SerializeDelta(bool writeToBitstream, ref long inOutCurrentValue, long lastValue)
	{
		return RakNetPINVOKE.BitStream_SerializeDelta__SWIG_12(swigCPtr, writeToBitstream, ref inOutCurrentValue, lastValue);
	}

	public bool SerializeDelta(bool writeToBitstream, ref long inOutCurrentValue)
	{
		return RakNetPINVOKE.BitStream_SerializeDelta__SWIG_13(swigCPtr, writeToBitstream, ref inOutCurrentValue);
	}

	public bool SerializeDelta(bool writeToBitstream, ref float inOutCurrentValue, float lastValue)
	{
		return RakNetPINVOKE.BitStream_SerializeDelta__SWIG_14(swigCPtr, writeToBitstream, ref inOutCurrentValue, lastValue);
	}

	public bool SerializeDelta(bool writeToBitstream, ref float inOutCurrentValue)
	{
		return RakNetPINVOKE.BitStream_SerializeDelta__SWIG_15(swigCPtr, writeToBitstream, ref inOutCurrentValue);
	}

	public bool SerializeDelta(bool writeToBitstream, RakString inOutCurrentValue, RakString lastValue)
	{
		bool result = RakNetPINVOKE.BitStream_SerializeDelta__SWIG_16(swigCPtr, writeToBitstream, RakString.getCPtr(inOutCurrentValue), RakString.getCPtr(lastValue));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool SerializeDelta(bool writeToBitstream, RakString inOutCurrentValue)
	{
		bool result = RakNetPINVOKE.BitStream_SerializeDelta__SWIG_17(swigCPtr, writeToBitstream, RakString.getCPtr(inOutCurrentValue));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool SerializeDelta(bool writeToBitstream, RakNetGUID inOutCurrentValue, RakNetGUID lastValue)
	{
		bool result = RakNetPINVOKE.BitStream_SerializeDelta__SWIG_18(swigCPtr, writeToBitstream, RakNetGUID.getCPtr(inOutCurrentValue), RakNetGUID.getCPtr(lastValue));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool SerializeDelta(bool writeToBitstream, RakNetGUID inOutCurrentValue)
	{
		bool result = RakNetPINVOKE.BitStream_SerializeDelta__SWIG_19(swigCPtr, writeToBitstream, RakNetGUID.getCPtr(inOutCurrentValue));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool SerializeDelta(bool writeToBitstream, uint24_t inOutCurrentValue, uint24_t lastValue)
	{
		bool result = RakNetPINVOKE.BitStream_SerializeDelta__SWIG_20(swigCPtr, writeToBitstream, uint24_t.getCPtr(inOutCurrentValue), uint24_t.getCPtr(lastValue));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool SerializeDelta(bool writeToBitstream, uint24_t inOutCurrentValue)
	{
		bool result = RakNetPINVOKE.BitStream_SerializeDelta__SWIG_21(swigCPtr, writeToBitstream, uint24_t.getCPtr(inOutCurrentValue));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool SerializeCompressed(bool writeToBitstream, ref bool inOutTemplateVar)
	{
		return RakNetPINVOKE.BitStream_SerializeCompressed__SWIG_1(swigCPtr, writeToBitstream, ref inOutTemplateVar);
	}

	public bool SerializeCompressed(bool writeToBitstream, ref byte inOutTemplateVar)
	{
		return RakNetPINVOKE.BitStream_SerializeCompressed__SWIG_2(swigCPtr, writeToBitstream, ref inOutTemplateVar);
	}

	public bool SerializeCompressed(bool writeToBitstream, ref short inOutTemplateVar)
	{
		return RakNetPINVOKE.BitStream_SerializeCompressed__SWIG_3(swigCPtr, writeToBitstream, ref inOutTemplateVar);
	}

	public bool SerializeCompressed(bool writeToBitstream, ref ushort inOutTemplateVar)
	{
		return RakNetPINVOKE.BitStream_SerializeCompressed__SWIG_4(swigCPtr, writeToBitstream, ref inOutTemplateVar);
	}

	public bool SerializeCompressed(bool writeToBitstream, ref int inOutTemplateVar)
	{
		return RakNetPINVOKE.BitStream_SerializeCompressed__SWIG_5(swigCPtr, writeToBitstream, ref inOutTemplateVar);
	}

	public bool SerializeCompressed(bool writeToBitstream, ref long inOutTemplateVar)
	{
		return RakNetPINVOKE.BitStream_SerializeCompressed__SWIG_6(swigCPtr, writeToBitstream, ref inOutTemplateVar);
	}

	public bool SerializeCompressed(bool writeToBitstream, ref float inOutTemplateVar)
	{
		return RakNetPINVOKE.BitStream_SerializeCompressed__SWIG_7(swigCPtr, writeToBitstream, ref inOutTemplateVar);
	}

	public bool SerializeCompressed(bool writeToBitstream, RakString inOutTemplateVar)
	{
		bool result = RakNetPINVOKE.BitStream_SerializeCompressed__SWIG_8(swigCPtr, writeToBitstream, RakString.getCPtr(inOutTemplateVar));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool SerializeCompressed(bool writeToBitstream, RakNetGUID inOutTemplateVar)
	{
		bool result = RakNetPINVOKE.BitStream_SerializeCompressed__SWIG_9(swigCPtr, writeToBitstream, RakNetGUID.getCPtr(inOutTemplateVar));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool SerializeCompressed(bool writeToBitstream, uint24_t inOutTemplateVar)
	{
		bool result = RakNetPINVOKE.BitStream_SerializeCompressed__SWIG_10(swigCPtr, writeToBitstream, uint24_t.getCPtr(inOutTemplateVar));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool SerializeCompressedDelta(bool writeToBitstream, ref bool inOutCurrentValue, bool lastValue)
	{
		return RakNetPINVOKE.BitStream_SerializeCompressedDelta__SWIG_2(swigCPtr, writeToBitstream, ref inOutCurrentValue, lastValue);
	}

	public bool SerializeCompressedDelta(bool writeToBitstream, ref bool inOutTemplateVar)
	{
		return RakNetPINVOKE.BitStream_SerializeCompressedDelta__SWIG_3(swigCPtr, writeToBitstream, ref inOutTemplateVar);
	}

	public bool SerializeCompressedDelta(bool writeToBitstream, ref byte inOutCurrentValue, byte lastValue)
	{
		return RakNetPINVOKE.BitStream_SerializeCompressedDelta__SWIG_4(swigCPtr, writeToBitstream, ref inOutCurrentValue, lastValue);
	}

	public bool SerializeCompressedDelta(bool writeToBitstream, ref byte inOutTemplateVar)
	{
		return RakNetPINVOKE.BitStream_SerializeCompressedDelta__SWIG_5(swigCPtr, writeToBitstream, ref inOutTemplateVar);
	}

	public bool SerializeCompressedDelta(bool writeToBitstream, ref short inOutCurrentValue, short lastValue)
	{
		return RakNetPINVOKE.BitStream_SerializeCompressedDelta__SWIG_6(swigCPtr, writeToBitstream, ref inOutCurrentValue, lastValue);
	}

	public bool SerializeCompressedDelta(bool writeToBitstream, ref short inOutTemplateVar)
	{
		return RakNetPINVOKE.BitStream_SerializeCompressedDelta__SWIG_7(swigCPtr, writeToBitstream, ref inOutTemplateVar);
	}

	public bool SerializeCompressedDelta(bool writeToBitstream, ref ushort inOutCurrentValue, ushort lastValue)
	{
		return RakNetPINVOKE.BitStream_SerializeCompressedDelta__SWIG_8(swigCPtr, writeToBitstream, ref inOutCurrentValue, lastValue);
	}

	public bool SerializeCompressedDelta(bool writeToBitstream, ref ushort inOutTemplateVar)
	{
		return RakNetPINVOKE.BitStream_SerializeCompressedDelta__SWIG_9(swigCPtr, writeToBitstream, ref inOutTemplateVar);
	}

	public bool SerializeCompressedDelta(bool writeToBitstream, ref int inOutCurrentValue, int lastValue)
	{
		return RakNetPINVOKE.BitStream_SerializeCompressedDelta__SWIG_10(swigCPtr, writeToBitstream, ref inOutCurrentValue, lastValue);
	}

	public bool SerializeCompressedDelta(bool writeToBitstream, ref int inOutTemplateVar)
	{
		return RakNetPINVOKE.BitStream_SerializeCompressedDelta__SWIG_11(swigCPtr, writeToBitstream, ref inOutTemplateVar);
	}

	public bool SerializeCompressedDelta(bool writeToBitstream, ref long inOutCurrentValue, long lastValue)
	{
		return RakNetPINVOKE.BitStream_SerializeCompressedDelta__SWIG_12(swigCPtr, writeToBitstream, ref inOutCurrentValue, lastValue);
	}

	public bool SerializeCompressedDelta(bool writeToBitstream, ref long inOutTemplateVar)
	{
		return RakNetPINVOKE.BitStream_SerializeCompressedDelta__SWIG_13(swigCPtr, writeToBitstream, ref inOutTemplateVar);
	}

	public bool SerializeCompressedDelta(bool writeToBitstream, ref float inOutCurrentValue, float lastValue)
	{
		return RakNetPINVOKE.BitStream_SerializeCompressedDelta__SWIG_14(swigCPtr, writeToBitstream, ref inOutCurrentValue, lastValue);
	}

	public bool SerializeCompressedDelta(bool writeToBitstream, ref float inOutTemplateVar)
	{
		return RakNetPINVOKE.BitStream_SerializeCompressedDelta__SWIG_15(swigCPtr, writeToBitstream, ref inOutTemplateVar);
	}

	public bool SerializeCompressedDelta(bool writeToBitstream, RakString inOutCurrentValue, RakString lastValue)
	{
		bool result = RakNetPINVOKE.BitStream_SerializeCompressedDelta__SWIG_16(swigCPtr, writeToBitstream, RakString.getCPtr(inOutCurrentValue), RakString.getCPtr(lastValue));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool SerializeCompressedDelta(bool writeToBitstream, RakString inOutTemplateVar)
	{
		bool result = RakNetPINVOKE.BitStream_SerializeCompressedDelta__SWIG_17(swigCPtr, writeToBitstream, RakString.getCPtr(inOutTemplateVar));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool SerializeCompressedDelta(bool writeToBitstream, RakNetGUID inOutCurrentValue, RakNetGUID lastValue)
	{
		bool result = RakNetPINVOKE.BitStream_SerializeCompressedDelta__SWIG_18(swigCPtr, writeToBitstream, RakNetGUID.getCPtr(inOutCurrentValue), RakNetGUID.getCPtr(lastValue));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool SerializeCompressedDelta(bool writeToBitstream, RakNetGUID inOutTemplateVar)
	{
		bool result = RakNetPINVOKE.BitStream_SerializeCompressedDelta__SWIG_19(swigCPtr, writeToBitstream, RakNetGUID.getCPtr(inOutTemplateVar));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool SerializeCompressedDelta(bool writeToBitstream, uint24_t inOutCurrentValue, uint24_t lastValue)
	{
		bool result = RakNetPINVOKE.BitStream_SerializeCompressedDelta__SWIG_20(swigCPtr, writeToBitstream, uint24_t.getCPtr(inOutCurrentValue), uint24_t.getCPtr(lastValue));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool SerializeCompressedDelta(bool writeToBitstream, uint24_t inOutTemplateVar)
	{
		bool result = RakNetPINVOKE.BitStream_SerializeCompressedDelta__SWIG_21(swigCPtr, writeToBitstream, uint24_t.getCPtr(inOutTemplateVar));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void Write(bool inTemplateVar)
	{
		RakNetPINVOKE.BitStream_Write__SWIG_8(swigCPtr, inTemplateVar);
	}

	public void Write(byte inTemplateVar)
	{
		RakNetPINVOKE.BitStream_Write__SWIG_9(swigCPtr, inTemplateVar);
	}

	public void Write(char inTemplateVar)
	{
		RakNetPINVOKE.BitStream_Write__SWIG_10(swigCPtr, inTemplateVar);
	}

	public void Write(short inTemplateVar)
	{
		RakNetPINVOKE.BitStream_Write__SWIG_11(swigCPtr, inTemplateVar);
	}

	public void Write(ushort inTemplateVar)
	{
		RakNetPINVOKE.BitStream_Write__SWIG_12(swigCPtr, inTemplateVar);
	}

	public void Write(int inTemplateVar)
	{
		RakNetPINVOKE.BitStream_Write__SWIG_13(swigCPtr, inTemplateVar);
	}

	public void Write(long inTemplateVar)
	{
		RakNetPINVOKE.BitStream_Write__SWIG_14(swigCPtr, inTemplateVar);
	}

	public void Write(float inTemplateVar)
	{
		RakNetPINVOKE.BitStream_Write__SWIG_15(swigCPtr, inTemplateVar);
	}

	public void Write(RakString inTemplateVar)
	{
		RakNetPINVOKE.BitStream_Write__SWIG_16(swigCPtr, RakString.getCPtr(inTemplateVar));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void Write(RakNetGUID inTemplateVar)
	{
		RakNetPINVOKE.BitStream_Write__SWIG_17(swigCPtr, RakNetGUID.getCPtr(inTemplateVar));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void Write(uint24_t inTemplateVar)
	{
		RakNetPINVOKE.BitStream_Write__SWIG_18(swigCPtr, uint24_t.getCPtr(inTemplateVar));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void WriteDelta(string currentValue, string lastValue)
	{
		RakNetPINVOKE.BitStream_WriteDelta__SWIG_2(swigCPtr, currentValue, lastValue);
	}

	public void WriteDelta(string currentValue)
	{
		RakNetPINVOKE.BitStream_WriteDelta__SWIG_3(swigCPtr, currentValue);
	}

	public void WriteDelta(bool currentValue, bool lastValue)
	{
		RakNetPINVOKE.BitStream_WriteDelta__SWIG_4(swigCPtr, currentValue, lastValue);
	}

	public void WriteDelta(bool currentValue)
	{
		RakNetPINVOKE.BitStream_WriteDelta__SWIG_5(swigCPtr, currentValue);
	}

	public void WriteDelta(byte currentValue, byte lastValue)
	{
		RakNetPINVOKE.BitStream_WriteDelta__SWIG_6(swigCPtr, currentValue, lastValue);
	}

	public void WriteDelta(byte currentValue)
	{
		RakNetPINVOKE.BitStream_WriteDelta__SWIG_7(swigCPtr, currentValue);
	}

	public void WriteDelta(char currentValue, char lastValue)
	{
		RakNetPINVOKE.BitStream_WriteDelta__SWIG_8(swigCPtr, currentValue, lastValue);
	}

	public void WriteDelta(char currentValue)
	{
		RakNetPINVOKE.BitStream_WriteDelta__SWIG_9(swigCPtr, currentValue);
	}

	public void WriteDelta(short currentValue, short lastValue)
	{
		RakNetPINVOKE.BitStream_WriteDelta__SWIG_10(swigCPtr, currentValue, lastValue);
	}

	public void WriteDelta(short currentValue)
	{
		RakNetPINVOKE.BitStream_WriteDelta__SWIG_11(swigCPtr, currentValue);
	}

	public void WriteDelta(ushort currentValue, ushort lastValue)
	{
		RakNetPINVOKE.BitStream_WriteDelta__SWIG_12(swigCPtr, currentValue, lastValue);
	}

	public void WriteDelta(ushort currentValue)
	{
		RakNetPINVOKE.BitStream_WriteDelta__SWIG_13(swigCPtr, currentValue);
	}

	public void WriteDelta(int currentValue, int lastValue)
	{
		RakNetPINVOKE.BitStream_WriteDelta__SWIG_14(swigCPtr, currentValue, lastValue);
	}

	public void WriteDelta(int currentValue)
	{
		RakNetPINVOKE.BitStream_WriteDelta__SWIG_15(swigCPtr, currentValue);
	}

	public void WriteDelta(long currentValue, long lastValue)
	{
		RakNetPINVOKE.BitStream_WriteDelta__SWIG_16(swigCPtr, currentValue, lastValue);
	}

	public void WriteDelta(long currentValue)
	{
		RakNetPINVOKE.BitStream_WriteDelta__SWIG_17(swigCPtr, currentValue);
	}

	public void WriteDelta(float currentValue, float lastValue)
	{
		RakNetPINVOKE.BitStream_WriteDelta__SWIG_18(swigCPtr, currentValue, lastValue);
	}

	public void WriteDelta(float currentValue)
	{
		RakNetPINVOKE.BitStream_WriteDelta__SWIG_19(swigCPtr, currentValue);
	}

	public void WriteDelta(RakString currentValue, RakString lastValue)
	{
		RakNetPINVOKE.BitStream_WriteDelta__SWIG_20(swigCPtr, RakString.getCPtr(currentValue), RakString.getCPtr(lastValue));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void WriteDelta(RakString currentValue)
	{
		RakNetPINVOKE.BitStream_WriteDelta__SWIG_21(swigCPtr, RakString.getCPtr(currentValue));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void WriteDelta(RakNetGUID currentValue, RakNetGUID lastValue)
	{
		RakNetPINVOKE.BitStream_WriteDelta__SWIG_22(swigCPtr, RakNetGUID.getCPtr(currentValue), RakNetGUID.getCPtr(lastValue));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void WriteDelta(RakNetGUID currentValue)
	{
		RakNetPINVOKE.BitStream_WriteDelta__SWIG_23(swigCPtr, RakNetGUID.getCPtr(currentValue));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void WriteDelta(uint24_t currentValue, uint24_t lastValue)
	{
		RakNetPINVOKE.BitStream_WriteDelta__SWIG_24(swigCPtr, uint24_t.getCPtr(currentValue), uint24_t.getCPtr(lastValue));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void WriteDelta(uint24_t currentValue)
	{
		RakNetPINVOKE.BitStream_WriteDelta__SWIG_25(swigCPtr, uint24_t.getCPtr(currentValue));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void WriteCompressed(bool inTemplateVar)
	{
		RakNetPINVOKE.BitStream_WriteCompressed__SWIG_5(swigCPtr, inTemplateVar);
	}

	public void WriteCompressed(byte inTemplateVar)
	{
		RakNetPINVOKE.BitStream_WriteCompressed__SWIG_6(swigCPtr, inTemplateVar);
	}

	public void WriteCompressed(char inTemplateVar)
	{
		RakNetPINVOKE.BitStream_WriteCompressed__SWIG_7(swigCPtr, inTemplateVar);
	}

	public void WriteCompressed(short inTemplateVar)
	{
		RakNetPINVOKE.BitStream_WriteCompressed__SWIG_8(swigCPtr, inTemplateVar);
	}

	public void WriteCompressed(ushort inTemplateVar)
	{
		RakNetPINVOKE.BitStream_WriteCompressed__SWIG_9(swigCPtr, inTemplateVar);
	}

	public void WriteCompressed(int inTemplateVar)
	{
		RakNetPINVOKE.BitStream_WriteCompressed__SWIG_10(swigCPtr, inTemplateVar);
	}

	public void WriteCompressed(long inTemplateVar)
	{
		RakNetPINVOKE.BitStream_WriteCompressed__SWIG_11(swigCPtr, inTemplateVar);
	}

	public void WriteCompressed(float inTemplateVar)
	{
		RakNetPINVOKE.BitStream_WriteCompressed__SWIG_12(swigCPtr, inTemplateVar);
	}

	public void WriteCompressed(RakString inTemplateVar)
	{
		RakNetPINVOKE.BitStream_WriteCompressed__SWIG_13(swigCPtr, RakString.getCPtr(inTemplateVar));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void WriteCompressed(RakNetGUID inTemplateVar)
	{
		RakNetPINVOKE.BitStream_WriteCompressed__SWIG_14(swigCPtr, RakNetGUID.getCPtr(inTemplateVar));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void WriteCompressed(uint24_t inTemplateVar)
	{
		RakNetPINVOKE.BitStream_WriteCompressed__SWIG_15(swigCPtr, uint24_t.getCPtr(inTemplateVar));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void WriteCompressedDelta(string currentValue, string lastValue)
	{
		RakNetPINVOKE.BitStream_WriteCompressedDelta__SWIG_2(swigCPtr, currentValue, lastValue);
	}

	public void WriteCompressedDelta(string currentValue)
	{
		RakNetPINVOKE.BitStream_WriteCompressedDelta__SWIG_3(swigCPtr, currentValue);
	}

	public void WriteCompressedDelta(bool currentValue, bool lastValue)
	{
		RakNetPINVOKE.BitStream_WriteCompressedDelta__SWIG_4(swigCPtr, currentValue, lastValue);
	}

	public void WriteCompressedDelta(bool currentValue)
	{
		RakNetPINVOKE.BitStream_WriteCompressedDelta__SWIG_5(swigCPtr, currentValue);
	}

	public void WriteCompressedDelta(byte currentValue, byte lastValue)
	{
		RakNetPINVOKE.BitStream_WriteCompressedDelta__SWIG_6(swigCPtr, currentValue, lastValue);
	}

	public void WriteCompressedDelta(byte currentValue)
	{
		RakNetPINVOKE.BitStream_WriteCompressedDelta__SWIG_7(swigCPtr, currentValue);
	}

	public void WriteCompressedDelta(char currentValue, char lastValue)
	{
		RakNetPINVOKE.BitStream_WriteCompressedDelta__SWIG_8(swigCPtr, currentValue, lastValue);
	}

	public void WriteCompressedDelta(char currentValue)
	{
		RakNetPINVOKE.BitStream_WriteCompressedDelta__SWIG_9(swigCPtr, currentValue);
	}

	public void WriteCompressedDelta(short currentValue, short lastValue)
	{
		RakNetPINVOKE.BitStream_WriteCompressedDelta__SWIG_10(swigCPtr, currentValue, lastValue);
	}

	public void WriteCompressedDelta(short currentValue)
	{
		RakNetPINVOKE.BitStream_WriteCompressedDelta__SWIG_11(swigCPtr, currentValue);
	}

	public void WriteCompressedDelta(ushort currentValue, ushort lastValue)
	{
		RakNetPINVOKE.BitStream_WriteCompressedDelta__SWIG_12(swigCPtr, currentValue, lastValue);
	}

	public void WriteCompressedDelta(ushort currentValue)
	{
		RakNetPINVOKE.BitStream_WriteCompressedDelta__SWIG_13(swigCPtr, currentValue);
	}

	public void WriteCompressedDelta(int currentValue, int lastValue)
	{
		RakNetPINVOKE.BitStream_WriteCompressedDelta__SWIG_14(swigCPtr, currentValue, lastValue);
	}

	public void WriteCompressedDelta(int currentValue)
	{
		RakNetPINVOKE.BitStream_WriteCompressedDelta__SWIG_15(swigCPtr, currentValue);
	}

	public void WriteCompressedDelta(long currentValue, long lastValue)
	{
		RakNetPINVOKE.BitStream_WriteCompressedDelta__SWIG_16(swigCPtr, currentValue, lastValue);
	}

	public void WriteCompressedDelta(long currentValue)
	{
		RakNetPINVOKE.BitStream_WriteCompressedDelta__SWIG_17(swigCPtr, currentValue);
	}

	public void WriteCompressedDelta(float currentValue, float lastValue)
	{
		RakNetPINVOKE.BitStream_WriteCompressedDelta__SWIG_18(swigCPtr, currentValue, lastValue);
	}

	public void WriteCompressedDelta(float currentValue)
	{
		RakNetPINVOKE.BitStream_WriteCompressedDelta__SWIG_19(swigCPtr, currentValue);
	}

	public void WriteCompressedDelta(RakString currentValue, RakString lastValue)
	{
		RakNetPINVOKE.BitStream_WriteCompressedDelta__SWIG_20(swigCPtr, RakString.getCPtr(currentValue), RakString.getCPtr(lastValue));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void WriteCompressedDelta(RakString currentValue)
	{
		RakNetPINVOKE.BitStream_WriteCompressedDelta__SWIG_21(swigCPtr, RakString.getCPtr(currentValue));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void WriteCompressedDelta(RakNetGUID currentValue, RakNetGUID lastValue)
	{
		RakNetPINVOKE.BitStream_WriteCompressedDelta__SWIG_22(swigCPtr, RakNetGUID.getCPtr(currentValue), RakNetGUID.getCPtr(lastValue));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void WriteCompressedDelta(RakNetGUID currentValue)
	{
		RakNetPINVOKE.BitStream_WriteCompressedDelta__SWIG_23(swigCPtr, RakNetGUID.getCPtr(currentValue));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void WriteCompressedDelta(uint24_t currentValue, uint24_t lastValue)
	{
		RakNetPINVOKE.BitStream_WriteCompressedDelta__SWIG_24(swigCPtr, uint24_t.getCPtr(currentValue), uint24_t.getCPtr(lastValue));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void WriteCompressedDelta(uint24_t currentValue)
	{
		RakNetPINVOKE.BitStream_WriteCompressedDelta__SWIG_25(swigCPtr, uint24_t.getCPtr(currentValue));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool Read(out bool outTemplateVar)
	{
		return RakNetPINVOKE.BitStream_Read__SWIG_3(swigCPtr, out outTemplateVar);
	}

	public bool Read(out byte outTemplateVar)
	{
		return RakNetPINVOKE.BitStream_Read__SWIG_4(swigCPtr, out outTemplateVar);
	}

	public bool Read(out short outTemplateVar)
	{
		return RakNetPINVOKE.BitStream_Read__SWIG_5(swigCPtr, out outTemplateVar);
	}

	public bool Read(SWIGTYPE_p_unsigned_short outTemplateVar)
	{
		bool result = RakNetPINVOKE.BitStream_Read__SWIG_6(swigCPtr, SWIGTYPE_p_unsigned_short.getCPtr(outTemplateVar));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool Read(out int outTemplateVar)
	{
		return RakNetPINVOKE.BitStream_Read__SWIG_7(swigCPtr, out outTemplateVar);
	}

	public bool Read(out long outTemplateVar)
	{
		return RakNetPINVOKE.BitStream_Read__SWIG_8(swigCPtr, out outTemplateVar);
	}

	public bool Read(out float outTemplateVar)
	{
		return RakNetPINVOKE.BitStream_Read__SWIG_9(swigCPtr, out outTemplateVar);
	}

	public bool Read(RakString outTemplateVar)
	{
		bool result = RakNetPINVOKE.BitStream_Read__SWIG_10(swigCPtr, RakString.getCPtr(outTemplateVar));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool Read(RakNetGUID outTemplateVar)
	{
		bool result = RakNetPINVOKE.BitStream_Read__SWIG_11(swigCPtr, RakNetGUID.getCPtr(outTemplateVar));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool Read(uint24_t outTemplateVar)
	{
		bool result = RakNetPINVOKE.BitStream_Read__SWIG_12(swigCPtr, uint24_t.getCPtr(outTemplateVar));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool ReadDelta(out bool outTemplateVar)
	{
		return RakNetPINVOKE.BitStream_ReadDelta__SWIG_1(swigCPtr, out outTemplateVar);
	}

	public bool ReadDelta(out byte outTemplateVar)
	{
		return RakNetPINVOKE.BitStream_ReadDelta__SWIG_2(swigCPtr, out outTemplateVar);
	}

	public bool ReadDelta(out short outTemplateVar)
	{
		return RakNetPINVOKE.BitStream_ReadDelta__SWIG_3(swigCPtr, out outTemplateVar);
	}

	public bool ReadDelta(SWIGTYPE_p_unsigned_short outTemplateVar)
	{
		bool result = RakNetPINVOKE.BitStream_ReadDelta__SWIG_4(swigCPtr, SWIGTYPE_p_unsigned_short.getCPtr(outTemplateVar));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool ReadDelta(out int outTemplateVar)
	{
		return RakNetPINVOKE.BitStream_ReadDelta__SWIG_5(swigCPtr, out outTemplateVar);
	}

	public bool ReadDelta(out long outTemplateVar)
	{
		return RakNetPINVOKE.BitStream_ReadDelta__SWIG_6(swigCPtr, out outTemplateVar);
	}

	public bool ReadDelta(out float outTemplateVar)
	{
		return RakNetPINVOKE.BitStream_ReadDelta__SWIG_7(swigCPtr, out outTemplateVar);
	}

	public bool ReadDelta(RakString outTemplateVar)
	{
		bool result = RakNetPINVOKE.BitStream_ReadDelta__SWIG_8(swigCPtr, RakString.getCPtr(outTemplateVar));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool ReadDelta(RakNetGUID outTemplateVar)
	{
		bool result = RakNetPINVOKE.BitStream_ReadDelta__SWIG_9(swigCPtr, RakNetGUID.getCPtr(outTemplateVar));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool ReadDelta(uint24_t outTemplateVar)
	{
		bool result = RakNetPINVOKE.BitStream_ReadDelta__SWIG_10(swigCPtr, uint24_t.getCPtr(outTemplateVar));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool ReadCompressed(out bool outTemplateVar)
	{
		return RakNetPINVOKE.BitStream_ReadCompressed__SWIG_1(swigCPtr, out outTemplateVar);
	}

	public bool ReadCompressed(out byte outTemplateVar)
	{
		return RakNetPINVOKE.BitStream_ReadCompressed__SWIG_2(swigCPtr, out outTemplateVar);
	}

	public bool ReadCompressed(out short outTemplateVar)
	{
		return RakNetPINVOKE.BitStream_ReadCompressed__SWIG_3(swigCPtr, out outTemplateVar);
	}

	public bool ReadCompressed(SWIGTYPE_p_unsigned_short outTemplateVar)
	{
		bool result = RakNetPINVOKE.BitStream_ReadCompressed__SWIG_4(swigCPtr, SWIGTYPE_p_unsigned_short.getCPtr(outTemplateVar));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool ReadCompressed(out int outTemplateVar)
	{
		return RakNetPINVOKE.BitStream_ReadCompressed__SWIG_5(swigCPtr, out outTemplateVar);
	}

	public bool ReadCompressed(out long outTemplateVar)
	{
		return RakNetPINVOKE.BitStream_ReadCompressed__SWIG_6(swigCPtr, out outTemplateVar);
	}

	public bool ReadCompressed(out float outTemplateVar)
	{
		return RakNetPINVOKE.BitStream_ReadCompressed__SWIG_7(swigCPtr, out outTemplateVar);
	}

	public bool ReadCompressed(RakString outTemplateVar)
	{
		bool result = RakNetPINVOKE.BitStream_ReadCompressed__SWIG_8(swigCPtr, RakString.getCPtr(outTemplateVar));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool ReadCompressed(RakNetGUID outTemplateVar)
	{
		bool result = RakNetPINVOKE.BitStream_ReadCompressed__SWIG_9(swigCPtr, RakNetGUID.getCPtr(outTemplateVar));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool ReadCompressed(uint24_t outTemplateVar)
	{
		bool result = RakNetPINVOKE.BitStream_ReadCompressed__SWIG_10(swigCPtr, uint24_t.getCPtr(outTemplateVar));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool ReadCompressedDelta(out bool outTemplateVar)
	{
		return RakNetPINVOKE.BitStream_ReadCompressedDelta__SWIG_1(swigCPtr, out outTemplateVar);
	}

	public bool ReadCompressedDelta(out byte outTemplateVar)
	{
		return RakNetPINVOKE.BitStream_ReadCompressedDelta__SWIG_2(swigCPtr, out outTemplateVar);
	}

	public bool ReadCompressedDelta(out short outTemplateVar)
	{
		return RakNetPINVOKE.BitStream_ReadCompressedDelta__SWIG_3(swigCPtr, out outTemplateVar);
	}

	public bool ReadCompressedDelta(SWIGTYPE_p_unsigned_short outTemplateVar)
	{
		bool result = RakNetPINVOKE.BitStream_ReadCompressedDelta__SWIG_4(swigCPtr, SWIGTYPE_p_unsigned_short.getCPtr(outTemplateVar));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool ReadCompressedDelta(out int outTemplateVar)
	{
		return RakNetPINVOKE.BitStream_ReadCompressedDelta__SWIG_5(swigCPtr, out outTemplateVar);
	}

	public bool ReadCompressedDelta(out long outTemplateVar)
	{
		return RakNetPINVOKE.BitStream_ReadCompressedDelta__SWIG_6(swigCPtr, out outTemplateVar);
	}

	public bool ReadCompressedDelta(out float outTemplateVar)
	{
		return RakNetPINVOKE.BitStream_ReadCompressedDelta__SWIG_7(swigCPtr, out outTemplateVar);
	}

	public bool ReadCompressedDelta(RakString outTemplateVar)
	{
		bool result = RakNetPINVOKE.BitStream_ReadCompressedDelta__SWIG_8(swigCPtr, RakString.getCPtr(outTemplateVar));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool ReadCompressedDelta(RakNetGUID outTemplateVar)
	{
		bool result = RakNetPINVOKE.BitStream_ReadCompressedDelta__SWIG_9(swigCPtr, RakNetGUID.getCPtr(outTemplateVar));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool ReadCompressedDelta(uint24_t outTemplateVar)
	{
		bool result = RakNetPINVOKE.BitStream_ReadCompressedDelta__SWIG_10(swigCPtr, uint24_t.getCPtr(outTemplateVar));
		if (RakNetPINVOKE.SWIGPendingException.Pending)
		{
			throw RakNetPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}
}
