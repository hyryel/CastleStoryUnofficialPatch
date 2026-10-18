using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class FileListNodeContext : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	public byte op
	{
		get
		{
			return RakNetPINVOKE.FileListNodeContext_op_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.FileListNodeContext_op_set(swigCPtr, value);
		}
	}

	public uint flnc_extraData1
	{
		get
		{
			return RakNetPINVOKE.FileListNodeContext_flnc_extraData1_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.FileListNodeContext_flnc_extraData1_set(swigCPtr, value);
		}
	}

	public uint flnc_extraData2
	{
		get
		{
			return RakNetPINVOKE.FileListNodeContext_flnc_extraData2_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.FileListNodeContext_flnc_extraData2_set(swigCPtr, value);
		}
	}

	public uint flnc_extraData3
	{
		get
		{
			return RakNetPINVOKE.FileListNodeContext_flnc_extraData3_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.FileListNodeContext_flnc_extraData3_set(swigCPtr, value);
		}
	}

	public SWIGTYPE_p_void dataPtr
	{
		get
		{
			IntPtr intPtr = RakNetPINVOKE.FileListNodeContext_dataPtr_get(swigCPtr);
			return (!(intPtr == IntPtr.Zero)) ? new SWIGTYPE_p_void(intPtr, futureUse: false) : null;
		}
		set
		{
			RakNetPINVOKE.FileListNodeContext_dataPtr_set(swigCPtr, SWIGTYPE_p_void.getCPtr(value));
		}
	}

	public uint dataLength
	{
		get
		{
			return RakNetPINVOKE.FileListNodeContext_dataLength_get(swigCPtr);
		}
		set
		{
			RakNetPINVOKE.FileListNodeContext_dataLength_set(swigCPtr, value);
		}
	}

	internal FileListNodeContext(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	public FileListNodeContext()
		: this(RakNetPINVOKE.new_FileListNodeContext__SWIG_0(), cMemoryOwn: true)
	{
	}

	public FileListNodeContext(byte o, uint f1, uint f2, uint f3)
		: this(RakNetPINVOKE.new_FileListNodeContext__SWIG_1(o, f1, f2, f3), cMemoryOwn: true)
	{
	}

	internal static HandleRef getCPtr(FileListNodeContext obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~FileListNodeContext()
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
					RakNetPINVOKE.delete_FileListNodeContext(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
		}
	}
}
