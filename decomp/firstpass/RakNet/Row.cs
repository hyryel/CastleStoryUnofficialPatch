using System;
using System.Runtime.InteropServices;

namespace RakNet;

public class Row : IDisposable
{
	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	public RakNetListCellPointer cells
	{
		get
		{
			IntPtr intPtr = RakNetPINVOKE.Row_cells_get(swigCPtr);
			return (!(intPtr == IntPtr.Zero)) ? new RakNetListCellPointer(intPtr, cMemoryOwn: false) : null;
		}
		set
		{
			RakNetPINVOKE.Row_cells_set(swigCPtr, RakNetListCellPointer.getCPtr(value));
		}
	}

	internal Row(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	public Row()
		: this(RakNetPINVOKE.new_Row(), cMemoryOwn: true)
	{
	}

	internal static HandleRef getCPtr(Row obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	~Row()
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
					RakNetPINVOKE.delete_Row(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
			GC.SuppressFinalize(this);
		}
	}

	public void UpdateCell(uint columnIndex, double value)
	{
		RakNetPINVOKE.Row_UpdateCell__SWIG_0(swigCPtr, columnIndex, value);
	}

	public void UpdateCell(uint columnIndex, string str)
	{
		RakNetPINVOKE.Row_UpdateCell__SWIG_1(swigCPtr, columnIndex, str);
	}

	public void UpdateCell(uint columnIndex, int byteLength, byte[] inByteArray)
	{
		RakNetPINVOKE.Row_UpdateCell__SWIG_2(swigCPtr, columnIndex, byteLength, inByteArray);
	}
}
