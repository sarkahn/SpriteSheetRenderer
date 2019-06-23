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
        var t = new ComponentType[] 
        {
            ComponentType.ReadOnly<RenderBufferTag>(),
            ComponentType.ReadOnly<SpriteSheetMaterial>() };
        bufferQuery = GetEntityQuery(t);
        bufferArchetype = EntityManager.CreateArchetype(t);
    }

    protected override void OnUpdate()
    {
        sharedMaterials_.Clear();
        EntityManager.GetAllUniqueSharedComponentData(sharedMaterials_);
        // Ignore default ( null material )
        sharedMaterials_.RemoveAt(0);

        int bufferCount = bufferQuery.CalculateLength();
        
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
            Entities.WithAllReadOnly<RenderBufferTag>().WithAllReadOnly<SpriteSheetMaterial>().ForEach((Entity e) =>
            {
                PostUpdateCommands.SetSharedComponent(e, sharedMaterials_[i++]);
            });
        }      
    }
}
