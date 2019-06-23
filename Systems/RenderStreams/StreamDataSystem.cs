using DOTSSpriteRenderer.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;


namespace DOTSSpriteRenderer.Systems.RenderStreams
{
    /// <summary>
    /// Base class for systems that stream a single data type to a RenderBuffer.
    /// Derived classes should define <see cref="PopulateBuffer(Entity, BufferFromEntity{BufferType}, JobHandle)"/> 
    /// and fill the buffer from there.
    /// </summary>
    /// <typeparam name="BufferType">Type of dynamic buffer to hold the data.</typeparam>
    /// <typeparam name="DataType">Data type that will be streamed to the buffer.</typeparam>
    public abstract class StreamDataSystem<BufferType, DataType> : JobComponentSystem
    where BufferType : struct, IBufferElementData where DataType : struct, IComponentData
    {
        EntityQuery buffers_;
        protected EntityQuery source_;
        List<SpriteSheetMaterial> mats_ = new List<SpriteSheetMaterial>();

        public abstract JobHandle PopulateBuffer(Entity e, BufferFromEntity<BufferType> bfe, JobHandle inputDeps);

        [BurstCompile]
        struct InitBufferJob : IJob
        {
            public Entity bufferEntity;
            public BufferFromEntity<BufferType> bufferFromEntity;
            public int size;
            public void Execute()
            {
                bufferFromEntity[bufferEntity].ResizeUninitialized(size);
            }
        }


        protected override void OnCreate()
        {
            buffers_ = GetEntityQuery(
              ComponentType.ReadOnly<Components.RenderBufferTag>(),
              ComponentType.ReadOnly<SpriteSheetMaterial>(),
              ComponentType.ReadOnly<BufferType>());

            source_ = GetEntityQuery(
                ComponentType.ReadOnly<DataType>(),
                ComponentType.ReadOnly<SpriteSheetMaterial>());
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            mats_.Clear();
            EntityManager.GetAllUniqueSharedComponentData(mats_);
            // Ignore default (null material)
            mats_.RemoveAt(0);

            foreach (var mat in mats_)
            {
                buffers_.SetFilter(mat);
                source_.SetFilter(mat);

                if (buffers_.CalculateLength() == 0)
                    continue;

                Entity e;
                try
                {
                    e = buffers_.GetSingletonEntity();
                }
                catch (System.InvalidOperationException except)
                {
                    Debug.LogErrorFormat("Error retrieving singleton buffer {0}" +
                        ". Make sure you've initialized the buffer from GenerateRenderBuffersSystem.",
                        typeof(BufferType).ToString().ToUpper());
                    throw except;
                }

                var bfe = GetBufferFromEntity<BufferType>(false);

                int count = source_.CalculateLength();
                if (EntityManager.GetBuffer<BufferType>(e).Length != count)
                {
                    inputDeps = new InitBufferJob
                    {
                        size = count,
                        bufferEntity = e,
                        bufferFromEntity = bfe
                    }.Schedule(inputDeps);
                }

                inputDeps = PopulateBuffer(e, bfe, inputDeps);
            }

            return inputDeps;
        }
    }

}