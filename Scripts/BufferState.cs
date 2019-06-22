﻿using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Stores compute buffer state to avoid resizing unless necessary.
/// </summary>
public class BufferState : System.IDisposable
{
    int count_ = 0;
    int stride_ = 0;
    ComputeBuffer buffer_;
    string name_;
    
    public BufferState(string name, int stride)
    {
        name_ = name;
        stride_ = stride;
    }

    public int Set<T>(NativeArray<T> data, Material mat) where T : struct
    {
        if (buffer_ == null || count_ != data.Length)
        {
            count_ = data.Length;
            Resize();
        }
        buffer_.SetData(data);
        mat.SetBuffer(name_, buffer_);
        return count_;
    }

    void Resize()
    {
        buffer_?.Dispose();
        buffer_ = new ComputeBuffer(count_, stride_);
    }
    
    public void Dispose() => buffer_?.Dispose();
};



