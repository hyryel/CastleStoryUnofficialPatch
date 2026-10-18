using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class FileProgressStruct : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	private bool firstDataChunkIsCached;

	private byte[] firstDataChunkCache;

	private bool iriDataChunkIsCached;

	private byte[] iriDataChunkCache;

	public OnFileStruct onFileStruct
	{
		get
		{
			IntPtr intPtr = RakNetPINVOKE.FileProgressStruct_onFileStruct_get(swigCPtr);
			return (!(intPtr == IntPtr.Zero)) ? new OnFileStruct(intPtr, cMemoryOwn: false) : null;
		}
		set
		{
			RakNetPINVOKE.FileProgressStruct_onFileStruct_set(swigCPtr, OnFileStruct.getCPtr(value));
		}
	}

	public uint partCount
	{
		get
		{
			return RakNetPINVOKE.FileProgressStruct_partCount_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.FileProgressStruct_partCount_set(swigCPtr, value);
		}
	}

	public uint partTotal
	{
		get
		{
			return RakNetPINVOKE.FileProgressStruct_partTotal_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.FileProgressStruct_partTotal_set(swigCPtr, value);
		}
	}

	public uint dataChunkLength
	{
		get
		{
			return RakNetPINVOKE.FileProgressStruct_dataChunkLength_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.FileProgressStruct_dataChunkLength_set(swigCPtr, value);
		}
	}

	public byte[] firstDataChunk
	{
		get
		{
			byte[] array;
			if (!firstDataChunkIsCached)
			{
				IntPtr source = RakNetPINVOKE.FileProgressStruct_firstDataChunk_get(swigCPtr);
				int num = (int)dataChunkLength;
				if (num <= 0)
				{
					return null;
				}
				array = new byte[num];
				byte[] array2 = new byte[num];
				Marshal.Copy(source, array2, 0, num);
				array2.CopyTo(array, 0);
				firstDataChunkCache = array;
				firstDataChunkIsCached = true;
			}
			else
			{
				array = firstDataChunkCache;
			}
			return array;
		}
		set
		{
			firstDataChunkCache = value;
			firstDataChunkIsCached = true;
			SetFirstDataChunk(value, value.Length);
		}
	}

	public byte[] iriDataChunk
	{
		get
		{
			byte[] array;
			if (!iriDataChunkIsCached)
			{
				IntPtr source = RakNetPINVOKE.FileProgressStruct_iriDataChunk_get(swigCPtr);
				int num = (int)dataChunkLength;
				if (num <= 0)
				{
					return null;
				}
				array = new byte[num];
				byte[] array2 = new byte[num];
				Marshal.Copy(source, array2, 0, num);
				array2.CopyTo(array, 0);
				iriDataChunkCache = array;
				iriDataChunkIsCached = true;
			}
			else
			{
				array = iriDataChunkCache;
			}
			return array;
		}
		set
		{
			iriDataChunkCache = value;
			iriDataChunkIsCached = true;
			SetIriDataChunk(value, value.Length);
		}
	}

	public uint iriWriteOffset
	{
		get
		{
			return RakNetPINVOKE.FileProgressStruct_iriWriteOffset_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.FileProgressStruct_iriWriteOffset_set(swigCPtr, value);
		}
	}

	public SystemAddress senderSystemAddress
	{
		get
		{
			IntPtr intPtr = RakNetPINVOKE.FileProgressStruct_senderSystemAddress_get(swigCPtr);
			return (!(intPtr == IntPtr.Zero)) ? new SystemAddress(intPtr, cMemoryOwn: false) : null;
		}
		set
		{
			RakNetPINVOKE.FileProgressStruct_senderSystemAddress_set(swigCPtr, SystemAddress.getCPtr(value));
		}
	}

	public RakNetGUID senderGuid
	{
		get
		{
			IntPtr intPtr = RakNetPINVOKE.FileProgressStruct_senderGuid_get(swigCPtr);
			return (!(intPtr == IntPtr.Zero)) ? new RakNetGUID(intPtr, cMemoryOwn: false) : null;
		}
		set
		{
			RakNetPINVOKE.FileProgressStruct_senderGuid_set(swigCPtr, RakNetGUID.getCPtr(value));
		}
	}

	public bool allocateIrIDataChunkAutomatically
	{
		get
		{
			return RakNetPINVOKE.FileProgressStruct_allocateIrIDataChunkAutomatically_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.FileProgressStruct_allocateIrIDataChunkAutomatically_set(swigCPtr, value);
		}
	}

	internal FileProgressStruct(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	public FileProgressStruct()
		: this(RakNetPINVOKE.new_FileProgressStruct(), cMemoryOwn: true)
	{
	}

	internal static HandleRef getCPtr(FileProgressStruct obj)
	{
		if (obj != null)
		{
			if (obj.firstDataChunkIsCached)
			{
				obj.SetFirstDataChunk(obj.firstDataChunk, obj.firstDataChunk.Length);
			}
			if (obj.iriDataChunkIsCached)
			{
				obj.SetIriDataChunk(obj.iriDataChunk, obj.iriDataChunk.Length);
			}
			obj.firstDataChunkIsCached = false;
			obj.iriDataChunkIsCached = false;
		}
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~FileProgressStruct()
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
					RakNetPINVOKE.delete_FileProgressStruct(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
		}
	}

	private void SetFirstDataChunk(byte[] inByteArray, int numBytes)
	{
		RakNetPINVOKE.FileProgressStruct_SetFirstDataChunk(swigCPtr, inByteArray, numBytes);
	}

	private void SetIriDataChunk(byte[] inByteArray, int numBytes)
	{
		RakNetPINVOKE.FileProgressStruct_SetIriDataChunk(swigCPtr, inByteArray, numBytes);
	}
}
