using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

/// <summary>
/// Base class for systems that want to fill render buffers from
/// component data.
/// </summary>
// I didn't really want to do this but it saves a lot of typing.
public abstract class RenderBufferSystem<BufferDataT> : JobComponentSystem
  where BufferDataT : struct, IBufferElementData
{
    static List<SpriteSheetMaterial> sharedMaterials_ = new List<SpriteSheetMaterial>();

    EndSimulationEntityCommandBufferSystem initBufferSystem_;

    EntityQuery uninitializedBuffers_;
    EntityQuery initializedBuffers_;

    /// <summary>
    /// Derived classes should schedule a job to populate the buffer.
    /// The buffer is only guaranteed to be initialized - early outs or resizing can be handled at the call site or
    /// from the job.
    /// </summary>
    protected abstract JobHandle PopulateBuffer(Entity bufferEntity, SpriteSheetMaterial filterMat, JobHandle inputDeps);

    //[BurstCompile]
    struct InitializeBuffersJob : IJobForEachWithEntity<RenderBufferTag>
    {
        public EntityCommandBuffer.Concurrent commandBuffer;

        public void Execute(Entity entity, int index, [ReadOnly] ref RenderBufferTag c0)
        {
            commandBuffer.AddBuffer<BufferDataT>(index, entity);
        }
    }

    protected override void OnCreate()
    {
        uninitializedBuffers_ = GetEntityQuery(
          ComponentType.ReadOnly<RenderBufferTag>(),
          ComponentType.ReadOnly<SpriteSheetMaterial>(),
          ComponentType.Exclude<BufferDataT>());
        uninitializedBuffers_.SetFilterChanged(ComponentType.ReadOnly<SpriteSheetMaterial>());

        initializedBuffers_ = GetEntityQuery(
          ComponentType.ReadOnly<RenderBufferTag>(),
          ComponentType.ReadOnly<SpriteSheetMaterial>(),
          ComponentType.ReadOnly<BufferDataT>());

        initBufferSystem_ = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        if (uninitializedBuffers_.CalculateLength() > 0)
        {
            inputDeps = new InitializeBuffersJob
            {
                commandBuffer = initBufferSystem_.CreateCommandBuffer().ToConcurrent(),
            }.Schedule(this, inputDeps);

            initBufferSystem_.AddJobHandleForProducer(inputDeps);
        }

        if (initializedBuffers_.CalculateLength() == 0)
            return inputDeps;

        sharedMaterials_.Clear();
        EntityManager.GetAllUniqueSharedComponentData(sharedMaterials_);
        // Ignore default (null material)
        sharedMaterials_.RemoveAt(0);

        foreach (var mat in sharedMaterials_)
        {
            initializedBuffers_.SetFilter(mat);
            var bufferEntity = initializedBuffers_.GetSingletonEntity();

            // Derived classes can fill the buffer here
            inputDeps = PopulateBuffer(bufferEntity, mat, inputDeps);
        }

        return inputDeps;
    }
}
