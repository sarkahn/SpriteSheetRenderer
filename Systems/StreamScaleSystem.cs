using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

public class StreamScaleSystem : StreamDataSystem<ScaleBuffer, Scale>
{
    [BurstCompile]
    struct StreamDataJob : IJobForEachWithEntity<Scale>
    {
        public Entity bufferEntity;
        [NativeDisableParallelForRestriction]
        public BufferFromEntity<ScaleBuffer> bufferFromEntity;

        public void Execute(Entity e, int index, [ReadOnly, ChangedFilter] ref Scale data)
        {
            var b = bufferFromEntity[bufferEntity];
            b[index] = data.Value;
        }
    }

    public override JobHandle PopulateBuffer(Entity e, BufferFromEntity<ScaleBuffer> bfe, JobHandle inputDeps)
    {
        return new StreamDataJob { bufferEntity = e, bufferFromEntity = bfe }.Schedule(this, inputDeps);
    }
}