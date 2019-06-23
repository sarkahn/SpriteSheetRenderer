using DOTSSpriteRenderer.Components;
using DOTSSpriteRenderer.RenderBuffers;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace DOTSSpriteRenderer.Systems.RenderStreams
{
    public class StreamPositionsSystem : StreamDataSystem<PosBuffer, Position2D>
    {
        [BurstCompile]
        struct StreamDataJob : IJobForEachWithEntity<Position2D>
        {
            public Entity bufferEntity;
            [NativeDisableParallelForRestriction]
            public BufferFromEntity<PosBuffer> bufferFromEntity;

            public void Execute(Entity e, int index, [ReadOnly, ChangedFilter] ref Position2D data)
            {
                var b = bufferFromEntity[bufferEntity];
                b[index] = data.value;
            }
        }

        public override JobHandle PopulateBuffer(Entity e, BufferFromEntity<PosBuffer> bfe, JobHandle inputDeps)
        {
            return new StreamDataJob { bufferEntity = e, bufferFromEntity = bfe }.Schedule(this, inputDeps);
        }
    }
}