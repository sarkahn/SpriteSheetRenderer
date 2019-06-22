using ECSSprites.Util;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class BufferStates : System.IDisposable
{
    ComputeBuffer argsBuffer;
    uint[] args;



    /// <summary>
    /// Base list of buffer states. When syncing to the list of shared component data  this
    /// list of states will be added to all keys in the <see cref="materialBufferMap_"/>
    /// </summary>
    List<BufferState> baseBufferStates_ = new List<BufferState>();
    
    /// <summary>
    /// Maps materials (Shared Component Data) to lists of buffers.
    /// </summary>
    MultiDictionary<SpriteSheetMaterial, BufferState> materialBufferMap_ = new MultiDictionary<SpriteSheetMaterial, BufferState>();

    /// <summary>
    /// Cached instance counts to avoid resizing.
    /// </summary>
    Dictionary<Material, int> instanceCounts_ = new Dictionary<Material, int>();

    Dictionary<SpriteSheetMaterial, bool> invalidState_;

    public BufferStates()
    {
        materialBufferMap_.onRemove_ += (l) =>
        {
            foreach (var state in l)
                state.Dispose();
        };
    }

    public void Dispose()
    {
        materialBufferMap_.Clear();
    }

    /// <summary>
    /// Add a buffer to the list of base buffers used during <see cref="SyncBufferStates(List{SpriteSheetMaterial})"/>
    /// </summary>
    public void AddBaseBuffer()
    {
        baseBufferStates_.Add(new BufferState());
    }

    /// <summary>
    /// Syncs the buffer states to the list of shared component data.
    /// Effectively this gives us a list of cached compute buffers (one for each render buffer) for every type
    /// of shared component data.
    /// </summary>
    /// <param name="mats">List of shared materials</param>
    public void SyncBufferStates(List<SpriteSheetMaterial> mats)
    {
        if (mats.Count != materialBufferMap_.KeyCount)
        {
            // Ensure all buffers get repopulated
            invalidState_.Clear();
            foreach (var mat in mats)
                invalidState_[mat] = true;

            HashSet<SpriteSheetMaterial> currMatSet = new HashSet<SpriteSheetMaterial>(mats);
            HashSet<SpriteSheetMaterial> bufferMapSet = new HashSet<SpriteSheetMaterial>(materialBufferMap_.Keys);

            bufferMapSet.ExceptWith(currMatSet);
            // Remove any old buffers
            foreach (var mat in bufferMapSet)
                materialBufferMap_.Remove(mat);

            // Add new ones
            currMatSet.ExceptWith(bufferMapSet);
            foreach(var mat in currMatSet )
                foreach (var state in baseBufferStates_)
                    materialBufferMap_.Add(mat, state);
            

        }
    }

    bool StateIsValid(SpriteSheetMaterial scd)
    {
        bool b;
        invalidState_.TryGetValue(scd, out b);
        return !b;
    }
}
