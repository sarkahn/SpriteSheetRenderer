using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

/// <summary>
/// Generates our render buffer entities. We generate one entity
/// for each material (Shared Component Data). Ensures the render
/// buffer entities stays in sync with our materials.
/// </summary>
[UpdateInGroup(typeof(InitializationSystemGroup))]
[AlwaysUpdateSystem]
public class GenerateRenderBuffersSystem : ComponentSystem
{
    EntityArchetype bufferArchetype;
    EntityQuery bufferQuery;

    HashedMaterials hashedMaterials_ = new HashedMaterials();
    static List<SpriteSheetMaterial> sharedMaterials_ = new List<SpriteSheetMaterial>();

    protected override void OnCreate()
    {
        bufferQuery = GetEntityQuery(typeof(RenderBufferTag), typeof(SpriteSheetMaterial));
        bufferArchetype = EntityManager.CreateArchetype(typeof(RenderBufferTag), typeof(SpriteSheetMaterial));
    }

    protected override void OnUpdate()
    {
        sharedMaterials_.Clear();
        EntityManager.GetAllUniqueSharedComponentData(sharedMaterials_);
        // Ignore default ( null material )
        sharedMaterials_.RemoveAt(0);

        int bufferCount = bufferQuery.CalculateLength();

        bool changed = hashedMaterials_.ChangedSinceLastFrame(sharedMaterials_);

        if (!changed)
            return;
        
        if ( bufferCount != sharedMaterials_.Count)
        {
            EntityManager.DestroyEntity(bufferQuery);
            for( int i = 0; i < sharedMaterials_.Count; ++i )
            {
                EntityManager.CreateEntity(bufferArchetype);
            }
        }
        {
            int i = 0;
            Entities.WithAll<RenderBufferTag>().WithAll<SpriteSheetMaterial>().ForEach((Entity e) =>
            {
                PostUpdateCommands.SetSharedComponent(e, sharedMaterials_[i++]);
            });
        }      
    }
}
