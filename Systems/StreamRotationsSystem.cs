using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

public class StreamRotation2DsSystem : StreamDataSystem<RotBuffer, Rotation2D>
{
    [BurstCompile]
    struct StreamDataJob : IJobForEachWithEntity<Rotation2D>
    {
        public Entity bufferEntity;
        [NativeDisableParallelForRestriction]
        public BufferFromEntity<RotBuffer> bufferFromEntity;

        public void Execute(Entity e, int index, [ReadOnly, ChangedFilter] ref Rotation2D data)
        {
            var b = bufferFromEntity[bufferEntity];
            b[index] = data.angle;
        }
    }

    public override JobHandle PopulateBuffer(Entity e, BufferFromEntity<RotBuffer> bfe, JobHandle inputDeps)
    {
        return new StreamDataJob { bufferEntity = e, bufferFromEntity = bfe }.Schedule(this, inputDeps);
    }
}