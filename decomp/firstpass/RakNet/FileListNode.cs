using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class FileListNode : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	private bool dataIsCached;

	private byte[] dataCache;

	public RakString filename
	{
		get
		{
			IntPtr intPtr = RakNetPINVOKE.FileListNode_filename_get(swigCPtr);
			return (!(intPtr == IntPtr.Zero)) ? new RakString(intPtr, cMemoryOwn: false) : null;
		}
		set
		{
			RakNetPINVOKE.FileListNode_filename_set(swigCPtr, RakString.getCPtr(value));
		}
	}

	public RakString fullPathToFile
	{
		get
		{
			IntPtr intPtr = RakNetPINVOKE.FileListNode_fullPathToFile_get(swigCPtr);
			return (!(intPtr == IntPtr.Zero)) ? new RakString(intPtr, cMemoryOwn: false) : null;
		}
		set
		{
			RakNetPINVOKE.FileListNode_fullPathToFile_set(swigCPtr, RakString.getCPtr(value));
		}
	}

	public byte[] data
	{
		get
		{
			byte[] array;
			if (!dataIsCached)
			{
				IntPtr source = RakNetPINVOKE.FileListNode_data_get(swigCPtr);
				int num = (int)dataLengthBytes;
				if (num <= 0)
				{
					return null;
				}
				array = new byte[num];
				byte[] array2 = new byte[num];
				Marshal.Copy(source, array2, 0, num);
				array2.CopyTo(array, 0);
				dataCache = array;
				dataIsCached = true;
			}
			else
			{
				array = dataCache;
			}
			return array;
		}
		set
		{
			dataCache = value;
			dataIsCached = true;
			SetData(value, value.Length);
		}
	}

	public uint dataLengthBytes
	{
		get
		{
			return RakNetPINVOKE.FileListNode_dataLengthBytes_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.FileListNode_dataLengthBytes_set(swigCPtr, value);
		}
	}

	public uint fileLengthBytes
	{
		get
		{
			return RakNetPINVOKE.FileListNode_fileLengthBytes_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.FileListNode_fileLengthBytes_set(swigCPtr, value);
		}
	}

	public FileListNodeContext context
	{
		get
		{
			IntPtr intPtr = RakNetPINVOKE.FileListNode_context_get(swigCPtr);
			return (!(intPtr == IntPtr.Zero)) ? new FileListNodeContext(intPtr, cMemoryOwn: false) : null;
		}
		set
		{
			RakNetPINVOKE.FileListNode_context_set(swigCPtr, FileListNodeContext.getCPtr(value));
		}
	}

	public bool isAReference
	{
		get
		{
			return RakNetPINVOKE.FileListNode_isAReference_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.FileListNode_isAReference_set(swigCPtr, value);
		}
	}

	internal FileListNode(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	public FileListNode()
		: this(RakNetPINVOKE.new_FileListNode(), cMemoryOwn: true)
	{
	}

	internal static HandleRef getCPtr(FileListNode obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~FileListNode()
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
					RakNetPINVOKE.delete_FileListNode(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
		}
	}

	public void SetData(byte[] inByteArray, int numBytes)
	{
		RakNetPINVOKE.FileListNode_SetData(swigCPtr, inByteArray, numBytes);
	}
}
