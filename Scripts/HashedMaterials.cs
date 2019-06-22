using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HashedMaterials
{ 
    HashSet<SpriteSheetMaterial> mats_ = new HashSet<SpriteSheetMaterial>();

    /// <summary>
    /// Compares and tracks all the given shared materials. Returns true
    /// if the materials have NOT changed since the last comparison.
    /// </summary>
    public bool ChangedSinceLastFrame(List<SpriteSheetMaterial> materials)
    {
        bool equal = mats_.SetEquals(materials);
        if (!equal)
        {
            mats_.Clear();
            foreach (var mat in materials)
                mats_.Add(mat);
        }
        return !equal;

    }
}
