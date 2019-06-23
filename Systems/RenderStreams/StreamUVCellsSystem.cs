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
    public class StreamUVCellsSystem : StreamDataSystem<UVCellBuffer, UVCell>
    {
        [BurstCompile]
        struct StreamDataJob : IJobForEachWithEntity<UVCell>
        {
            public Entity bufferEntity;
            [NativeDisableParallelForRestriction]
            public BufferFromEntity<UVCellBuffer> bufferFromEntity;

            public void Execute(Entity e, int index, [ReadOnly, ChangedFilter] ref UVCell data)
            {
                var b = bufferFromEntity[bufferEntity];
                b[index] = data.value;
            }
        }

        public override JobHandle PopulateBuffer(Entity e, BufferFromEntity<UVCellBuffer> bfe, JobHandle inputDeps)
        {
            return new StreamDataJob { bufferEntity = e, bufferFromEntity = bfe }.Schedule(this, inputDeps);
        }
    }
}