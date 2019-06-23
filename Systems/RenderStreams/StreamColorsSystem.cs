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
    public class StreamColorData : StreamDataSystem<ColorBuffer, SpriteSheetColor>
    {
        [BurstCompile]
        struct StreamColorsJob : IJobForEachWithEntity<SpriteSheetColor>
        {
            public Entity bufferEntity;
            [NativeDisableParallelForRestriction]
            public BufferFromEntity<ColorBuffer> bufferFromEntity;

            public void Execute(Entity e, int index, [ReadOnly, ChangedFilter] ref SpriteSheetColor data)
            {
                var b = bufferFromEntity[bufferEntity];
                b[index] = data.value;
            }
        }

        public override JobHandle PopulateBuffer(Entity e, BufferFromEntity<ColorBuffer> bfe, JobHandle inputDeps)
        {
            return new StreamColorsJob
            {
                bufferEntity = e,
                bufferFromEntity = bfe,
            }.Schedule(this, inputDeps);
        }
    }
}