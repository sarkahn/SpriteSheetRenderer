using ECSSprites.Util;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

/// <summary>
/// A class to keep Compute Buffer state around to avoid unnecessary work.
/// </summary>
public class BufferStates : System.IDisposable
{
    ComputeBuffer argsBuffer;
    uint[] args;

    Dictionary<SpriteSheetMaterial,ComputeBuffer> uvBuffers_ = new Dictionary<SpriteSheetMaterial, ComputeBuffer>();

    /// <summary>
    /// Maps materials (Shared Component Data) to lists of buffers.
    /// </summary>
    MultiListDictionary<SpriteSheetMaterial,BufferState> materialBufferMap_ = new MultiListDictionary<SpriteSheetMaterial, BufferState>();

    /// <summary>
    /// Cached instance counts to avoid resizing.
    /// </summary>
    Dictionary<SpriteSheetMaterial, int> instanceCounts_ = new Dictionary<SpriteSheetMaterial, int>();

    /// <summary>
    /// Types of buffers as well as their stride. These are added from
    /// <see cref="SpriteRenderSystem.AddBufferType{BufferType, DataType}"/>.
    /// </summary>
    List<(ComponentType, string, int)> bufferTypes_;

    Mesh mesh_;

    void WriteUVs(SpriteSheetMaterial mat)
    {
        ComputeBuffer b;
        if(!uvBuffers_.TryGetValue(mat, out b))
        {
            b = CachedUVData.GetUVBuffer(mat.material);
            mat.material.SetBuffer("uvBuffer", b);
            uvBuffers_[mat] = b;
        }
    }
    
    public BufferStates(List<(ComponentType,string,int)> bufferTypes)
    {
        bufferTypes_ = new List<(ComponentType, string, int)>(bufferTypes);
        materialBufferMap_.onRemove_ += (l) =>
        {
            foreach (var state in l)
                state.Dispose();
        };
        mesh_ = MeshExtension.Quad();

        args = new uint[5] { 6, 0, 0, 0, 0 };
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);

    }

    public void Dispose()
    {
        materialBufferMap_.Clear();
        argsBuffer.Dispose();
        foreach (var buffer in uvBuffers_)
            buffer.Value.Dispose();
    }

    public BufferState GetState(SpriteSheetMaterial key, int index)
    {
        return materialBufferMap_[key][index];
    }
    
    public void Render(SpriteSheetMaterial key, int count) 
    {
        if (count == 0)
            return;

        int oldCount;
        instanceCounts_.TryGetValue(key, out oldCount);
        if( count != oldCount )
        {
            instanceCounts_[key] = count;
            args[1] = (uint)count;
            argsBuffer.SetData(args);
        }
        WriteUVs(key);
        
        var bounds = new Bounds(Vector3.zero, Vector3.one * 5000);
        Graphics.DrawMeshInstancedIndirect(mesh_, 0, key.material, bounds, argsBuffer);
    }

    void BuildBuffersForKey(SpriteSheetMaterial key)
    {
        List<BufferState> states = materialBufferMap_.GetOrCreateValueList(key);
        states.Clear();
        for (int i = 0; i < bufferTypes_.Count; ++i)
            states.Add(new BufferState(bufferTypes_[i].Item2, bufferTypes_[i].Item3));
    }


    /// <summary>
    /// Syncs the buffer states to the list of shared component data.
    /// Effectively this gives us a list of cached compute buffers (one for each render buffer) for every type
    /// of shared component data. This should work in the case that an active material is added or removed.
    /// (I haven't tested that though)
    /// </summary>
    /// <param name="keys">List of shared materials</param>
    public void SyncBufferStates(List<SpriteSheetMaterial> keys)
    {
        if (keys.Count != materialBufferMap_.KeyCount)
        {
            HashSet<SpriteSheetMaterial> inKeys = new HashSet<SpriteSheetMaterial>(keys);
            HashSet<SpriteSheetMaterial> bufferMapSet = new HashSet<SpriteSheetMaterial>(materialBufferMap_.Keys);

            bufferMapSet.ExceptWith(inKeys);
            // Remove any old buffers for any removed material(this will dispose them automatically)
            foreach (var mat in bufferMapSet)
                materialBufferMap_.Remove(mat);

            // Add new buffers
            inKeys.ExceptWith(bufferMapSet);
            foreach (var key in inKeys)
                BuildBuffersForKey(key); 
        }
    }
}
