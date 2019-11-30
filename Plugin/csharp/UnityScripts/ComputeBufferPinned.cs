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


    PinnedArray<byte> _pinnedArray;

    public byte[] Array { get { return _pinnedArray.Array; } }
    public IntPtr ArrayPtr { get { return _pinnedArray.ArrayPtr; } }

    int ArraySize { get { return _size * _stride; } }

    ComputeBuffer _computeBuffer;
    int _size;

    int _stride;

    public ComputeBufferPinned(int size, int stride)
    {
      _size = size;
      _stride = stride;
      _computeBuffer = new ComputeBuffer(size, stride);

      _pinnedArray = new PinnedArray<byte>(size * stride);
    }

    public void UpdateData()
    {
      _computeBuffer.SetData(_pinnedArray.Array, 0, 0, _size * _stride);
    }

    public void Release()
    {
      if (_computeBuffer != null)
      {
        _computeBuffer.Release();
        _computeBuffer = null;
      }
      _size = 0;
      _stride = 0;
      _pinnedArray.Release();
    }

    ~ComputeBufferPinned()
    {
      Release();
    }
  }
}
