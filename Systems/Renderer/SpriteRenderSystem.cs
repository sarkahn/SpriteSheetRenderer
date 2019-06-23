using DOTSSpriteRenderer.Components;
using DOTSSpriteRenderer.RenderBuffers;
using DOTSSpriteRenderer.Systems.Renderer;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


namespace DOTSSpriteRenderer.Systems.Renderer
{
    public class SpriteRenderSystem : ComponentSystem
    {

        struct BufferQuery
        {
            public EntityQuery bufferQuery;
            // Probably doesn't belong here
            public FillBufferFunc fillBufferCallback;
        };

        List<SpriteSheetMaterial> sharedMats_ = new List<SpriteSheetMaterial>();

        /// <summary>
        /// List of buffer queries. When new buffers are needed they should be added via 
        /// <see cref="AddBufferType{BufferType, DataType}(string, int)"/>.
        /// </summary>
        List<BufferQuery> bufferQueries_ = new List<BufferQuery>();
        List<BufferSignature> bufferSignatures_ = new List<BufferSignature>();

        BufferStates bufferStates;

        delegate int FillBufferFunc(Entity e, BufferState s, Material m);

        /// <summary>
        /// Add a buffer type that will be processed for rendering.
        /// </summary>
        /// <param name="dataTypeStride">Stride of the data to be passed to the gpu.</param>
        /// <param name="shaderName">Name of the buffer inside the shader - must match exactly.</param>
        /// <typeparam name="BufferType">Type of the Dynamic Buffer to be read from.</typeparam>
        /// <typeparam name="DataType">Type that the buffer data will be reinterpreted as when sent to the GPU.</typeparam>
        // TODO: Something a little less awkward.
        void AddBufferType<BufferType, DataType>(string shaderName, int dataTypeStride)
            where BufferType : struct, IBufferElementData where DataType : struct
        {
            FillBufferFunc cb = (e, s, m) => WriteToBuffer<BufferType, DataType>(e, s, m);
            var t = ComponentType.ReadOnly<BufferType>();

            var bufferQuery = new BufferQuery
            {
                bufferQuery = GetEntityQuery(t, typeof(SpriteSheetMaterial)),
                fillBufferCallback = cb
            };

            var bufferSignature = new BufferSignature
            {
                dynamicBufferType = t,
                shaderName = shaderName,
                stride = dataTypeStride
            };

            bufferQueries_.Add(bufferQuery);
            bufferSignatures_.Add(bufferSignature);
        }

        void CreateBufferStates()
        {
            bufferStates = new BufferStates(bufferSignatures_);
        }

        protected override void OnCreate()
        {
            // Add buffers for processing here
            AddBufferType<PosBuffer, float2>("posBuffer", sizeof(float) * 2);
            AddBufferType<RotBuffer, float>("rotBuffer", sizeof(float));
            AddBufferType<ScaleBuffer, float>("scaleBuffer", sizeof(float));

            AddBufferType<ColorBuffer, float4>("colorBuffer", sizeof(float) * 4);
            AddBufferType<UVCellBuffer, int>("uvCellsBuffer", sizeof(int));

            CreateBufferStates();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            bufferStates.Dispose();
        }

        /// Returns the number of instances rendered
        // TODO: Something a little less awkward
        int WriteToBuffer<BufferType, DataType>(Entity e, BufferState state, Material mat)
            where BufferType : struct, IBufferElementData where DataType : struct
        {
            var buffer = EntityManager.GetBuffer<BufferType>(e);
            if (buffer.Length == 0)
                return 0;
            return state.Set(buffer.Reinterpret<DataType>().AsNativeArray(), mat);
        }

        protected override void OnUpdate()
        {
            sharedMats_.Clear();
            EntityManager.GetAllUniqueSharedComponentData(sharedMats_);
            sharedMats_.RemoveAt(0);

            bufferStates.SyncBufferStates(sharedMats_);

            Entities.ForEach((ref Rotation2D r) => { });

            foreach (var sharedMat in sharedMats_)
            {
                int count = 0;
                for (int i = 0; i < bufferQueries_.Count; ++i)
                {
                    var bufferQuery = bufferQueries_[i];

                    bufferQuery.bufferQuery.SetFilter(sharedMat);

                    var e = bufferQuery.bufferQuery.GetSingletonEntity();

                    var state = bufferStates.GetState(sharedMat, i);

                    count = bufferQuery.fillBufferCallback(e, state, sharedMat.material);
                }

                if (count == 0)
                    continue;

                bufferStates.Render(sharedMat, count);

            }
        }
    }
}