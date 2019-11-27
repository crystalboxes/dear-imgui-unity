using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace ImGuiNET.Unity
{
  class ComputeBufferPinned
  {
    public int Size { get { return _size; } }
    public int Stride { get { return _stride; } }
    public ComputeBuffer ComputeBuffer { get { return _computeBuffer; } }
    public byte[] Array { get { return _data; } }
    public IntPtr ArrayPtr { get { return _nativePtr; } }
    int ArraySize { get { return _size * _stride; } }

    byte[] _data;
    ComputeBuffer _computeBuffer;
    int _size;

    int _stride;

    IntPtr _nativePtr;
    GCHandle _gcHandle;

    public ComputeBufferPinned(int size, int stride)
    {
      _size = size;
      _stride = stride;
      _computeBuffer = new ComputeBuffer(size, stride);
      _data = new byte[size * stride];

      _gcHandle = GCHandle.Alloc(_data, GCHandleType.Pinned);
      _nativePtr = _gcHandle.AddrOfPinnedObject();
    }

    public void UpdateData()
    {
      _computeBuffer.SetData(_data, 0, 0, _size * _stride);
    }

    public void Release()
    {
      if (_computeBuffer != null)
      {
        _computeBuffer.Release();
        _computeBuffer = null;
      }
      if (_gcHandle.IsAllocated)
      {
        _gcHandle.Free();
      }
      _size = 0;
      _stride = 0;
      _nativePtr = IntPtr.Zero;
      _data = null;
    }

    ~ComputeBufferPinned()
    {
      Release();
    }
  }
}
