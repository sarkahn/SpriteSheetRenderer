using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[System.Serializable]
public struct RenderBuffer : IComponentData
{
    public bool changed;
}
