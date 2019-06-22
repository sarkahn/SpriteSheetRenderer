using ECSSprites.Util;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;


public class SpriteRenderSystem : ComponentSystem
{
    List<SpriteSheetMaterial> sharedMaterials = new List<SpriteSheetMaterial>();
    
    /// <summary>
    /// List of buffer queries. When new buffers are needed they should be added via <see cref="AddBuffer{T}"/>
    /// </summary>
    List<(EntityQuery, System.Func<Entity,BufferState,Material,int>)> bufferQueries_ = 
        new List<(EntityQuery, System.Func<Entity,BufferState,Material,int>)>();

    BufferStates bufferStates;

    List<(ComponentType,string,int)> bufferTypes_ = new List<(ComponentType,string,int)>();


    /// <summary>
    /// Add a buffer type that will be processed for rendering.
    /// </summary>
    /// <param name="dataTypeStride">Stride of the data to be passed to the gpu.</param>
    /// <typeparam name="BufferType">Type of the Dynamic Buffer to be read from.</typeparam>
    /// <typeparam name="DataType">Type that the buffer data will be reinterpreted as when sent to the GPU.</typeparam>
    void AddBufferType<BufferType,DataType>(string shaderName, int dataTypeStride) 
        where BufferType : struct, IBufferElementData where DataType : struct
    {
        System.Func<Entity, BufferState,Material,int> cb = (e, s, m) => WriteToBuffer<BufferType, DataType>(e, s, m);
        var t = ComponentType.ReadOnly<BufferType>();
        var q = GetEntityQuery(t, typeof(SpriteSheetMaterial));

        bufferQueries_.Add((q,cb));
        q.SetFilterChanged(t);
        bufferTypes_.Add((t,shaderName,dataTypeStride));
    }

    void CreateBufferStates()
    {
        bufferStates = new BufferStates(bufferTypes_);
    }

    protected override void OnCreate()
    {
        // Add buffers for processing here
        //AddBufferType<ColorBuffer,float4>( sizeof(float) * 4 );
        AddBufferType<MatrixBuffer,float4x2>( "transformBuffer", sizeof(float) * 8 );
        //AddBufferType<UVCellBuffer,int>( sizeof(int) );
        
        CreateBufferStates();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        bufferStates.Dispose();
    }
    
    /// Returns the number of instances rendered
    int WriteToBuffer<BufferType,DataType>(Entity e, BufferState state, Material mat) 
        where BufferType : struct, IBufferElementData where DataType : struct
    {
        var buffer = EntityManager.GetBuffer<BufferType>(e);
        return state.Set(buffer.Reinterpret<DataType>().AsNativeArray(), mat);
    }

    protected override void OnUpdate()
    {
        sharedMaterials.Clear();
        EntityManager.GetAllUniqueSharedComponentData(sharedMaterials);
        sharedMaterials.RemoveAt(0);

        bufferStates.SyncBufferStates(sharedMaterials);

        foreach (var sharedMat in sharedMaterials)
        {
            int count = 0;
            for( int i = 0; i < bufferQueries_.Count; ++i )
            {
                var (query, fillBufferCallback) = bufferQueries_[i];

                query.SetFilter(sharedMat);
                
                if (query.CalculateLength() == 0)
                    continue;
                
                
                var e = query.GetSingletonEntity();
                
                var state = bufferStates.GetState(sharedMat, i);
                
                count = fillBufferCallback(e, state, sharedMat.material);

                //List<BufferState> bufferStates;
                //if (!bufferStates_.TryGetValues(sharedMat, out bufferStates))
                //    continue;

                //WriteToBuffers(bufferStates);


                //matricesBufferQ.SetFilter(sharedMat);
                //colorsBufferQ.SetFilter(sharedMat);
                //positionsBufferQ.SetFilter(sharedMat);

                //var matrixDataBuffer = EntityManager.GetBuffer<MatrixBuffer>(matricesBufferQ.GetSingletonEntity());
                //var colorDataBuffer = EntityManager.GetBuffer<ColorBuffer>(colorsBufferQ.GetSingletonEntity());
                //var uvCellsDataBuffer = EntityManager.GetBuffer<UVCellBuffer>(uvCellsBufferQ.GetSingletonEntity());
                ////var positionsDatabuffer = EntityManager.GetBuffer<PosBuffer>(positionsBufferQ.GetSingletonEntity());

                ////int instanceCount = positionsDatabuffer.Length;
                //int instanceCount = matrixDataBuffer.Length;

                //var matrixData = matrixDataBuffer.Reinterpret<float4x2>().AsNativeArray();
                //var matrixBuffer = GetBuffer(instanceCount, sizeof(float) * 8);
                //matrixBuffer.SetData(matrixData);
                //sharedMat.material.SetBuffer("transformBuffer", matrixBuffer);

                //var colorData = colorDataBuffer.Reinterpret<float4>().AsNativeArray();
                //var colorsBuffer = GetBuffer(instanceCount, sizeof(float) * 4);
                //colorsBuffer.SetData(colorData);
                //sharedMat.material.SetBuffer("colorsBuffer", colorsBuffer);

                //var uvCellsData = uvCellsDataBuffer.Reinterpret<int>().AsNativeArray();
                //var uvCellsBuffer = GetBuffer(instanceCount, sizeof(int));
                //uvCellsBuffer.SetData(uvCellsData);
                //sharedMat.material.SetBuffer("uvCellsBuffer", uvCellsBuffer);

                ////var positionsData = positionsDatabuffer.Reinterpret<float2>().AsNativeArray();
                ////var posBuffer = GetBuffer(instanceCount, sizeof(float) * 2);
                ////posBuffer.SetData(positionsData);
                ////sharedMat.material.SetBuffer("positionsBuffer", posBuffer);

                //var uvBuffer = CachedUVData.GetUVBuffer(sharedMat.material);
                //oldBuffers.Enqueue(uvBuffer);
                //sharedMat.material.SetBuffer("uvBuffer", uvBuffer);

                //args[1] = (uint)instanceCount;
                //argsBuffer.SetData(args);
                //// Note - bounds essentially defines world space according to the drawmesh call
                //// Meaning the center of the bounds is where it considers (0,0,0)
                //var bounds = new Bounds(Vector3.zero, Vector3.one * 5000);
                //Graphics.DrawMeshInstancedIndirect(mesh, 0, sharedMat.material, bounds, argsBuffer);
            }


            bufferStates.Render(sharedMat, count);
        }
    }

    //void ResizeBuffer<BufferT>(EntityQuery q, int instanceCount, int stride) where BufferT : struct, IBufferElementData
    //{
    //    var e = q.GetSingletonEntity();
    //    var entityBuffer = EntityManager.GetBuffer<BufferT>(e);
    //    var matrixData = entityBuffer.Reinterpret<float4x2>().AsNativeArray();
    //    var matrixBuffer = GetBuffer(instanceCount, sizeof(float) * 8);
    //    matrixBuffer.SetData(matrixData);
    //    sharedMat.material.SetBuffer("transformBuffer", matrixBuffer);
    //}
}
