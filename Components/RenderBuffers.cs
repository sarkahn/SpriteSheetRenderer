﻿using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DOTSSpriteRenderer.RenderBuffers

{
    [System.Serializable]
    public struct ColorBuffer : IBufferElementData
    {
        public Color value;
        public static implicit operator Color(ColorBuffer c) => c.value;
        public static implicit operator ColorBuffer(Color c) => new ColorBuffer { value = c };
    }

    [System.Serializable]
    public struct PosBuffer : IBufferElementData
    {
        public float2 pos;
        public static implicit operator float2(PosBuffer p) => p.pos;
        public static implicit operator PosBuffer(float2 p) => new PosBuffer { pos = p };
    }

    [System.Serializable]
    public struct RotBuffer : IBufferElementData
    {
        public float value;
        public static implicit operator float(RotBuffer p) => p.value;
        public static implicit operator RotBuffer(float p) => new RotBuffer { value = p };
    }

    [System.Serializable]
    public struct ScaleBuffer : IBufferElementData
    {
        public float value;
        public static implicit operator float(ScaleBuffer p) => p.value;
        public static implicit operator ScaleBuffer(float p) => new ScaleBuffer { value = p };
    }

    [System.Serializable]
    public struct UVCellBuffer : IBufferElementData
    {
        public int value;
        public static implicit operator int(UVCellBuffer v) => v.value;
        public static implicit operator UVCellBuffer(int v) => new UVCellBuffer { value = v };
    }
}