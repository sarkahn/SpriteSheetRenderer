using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


//public class ColorBufferState : BufferState, IRenderData<ColorBuffer, NativeArray<float4>>
//{
//    public NativeArray<float4> GetRenderDataFromBuffer(DynamicBuffer<ColorBuffer> buffer)
//        => buffer.Reinterpret<float4>().AsNativeArray();
//}

//public class MatrixBufferState : BufferState, IRenderData<MatrixBuffer, NativeArray<float4x2>>
//{
//    public NativeArray<float4x2> GetRenderDataFromBuffer(DynamicBuffer<MatrixBuffer> buffer)
//        => buffer.Reinterpret<float4x2>().AsNativeArray();
//}

//public class UVCellBufferState : BufferState, IRenderData<UVCellBuffer, NativeArray<int>>
//{
//    public NativeArray<int> GetRenderDataFromBuffer(DynamicBuffer<UVCellBuffer> buffer)
//        => buffer.Reinterpret<int>().AsNativeArray();
//}

///// <summary>
///// Interface to convert data from an entity dynamic buffer into data we can send 
///// into a compute buffer.
///// </summary>
///// <typeparam name="BufferT">Type of Dynamic Buffer</typeparam>
///// <typeparam name="DataT">Type of data that will be sent to the compute buffer.</typeparam>
//public interface IRenderData<BufferT,Data> where BufferT : struct, IBufferElementData
//{
//    Data GetRenderDataFromBuffer(DynamicBuffer<BufferT> buffer);
//}

/// <summary>
/// Stores compute buffer state to avoid resizing unless necessary.
/// </summary>
public class BufferState : System.IDisposable
{
    int count_ = 0;
    int stride_ = 0;
    ComputeBuffer buffer_;
    
    public void Initialize(int stride)
    {
        stride_ = stride;
    }

    public void Set<T>(NativeArray<T> data) where T : struct
    {
        if (buffer_ == null || count_ != data.Length)
        {
            count_ = data.Length;
            Resize();
        }
        buffer_.SetData(data);
    }

    void Resize()
    {
        buffer_?.Dispose();
        buffer_ = new ComputeBuffer(count_, stride_);
    }
    
    public void Dispose() => buffer_?.Dispose();
};



