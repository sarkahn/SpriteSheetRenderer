using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct BufferSignature
{
    public ComponentType dynamicBufferType;
    public string shaderName;
    public int stride;
};
