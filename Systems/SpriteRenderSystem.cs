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

    Mesh mesh;

    /// <summary>
    /// List of buffer queries. When new buffers are needed they should be added via <see cref="AddBuffer{T}"/>
    /// </summary>
    List<(EntityQuery, ComponentType)> bufferQueries_ = new List<(EntityQuery, ComponentType)>();

    void AddBufferQuery<T>() where T : struct, IBufferElementData
    {
        var t = ComponentType.ReadOnly<T>();
        var q = GetEntityQuery(t, typeof(SpriteSheetMaterial));
        bufferQueries_.Add((q, t));
        q.SetFilterChanged(t);
    }

    protected override void OnCreate()
    {
        GetEntityQuery
        // Add buffers for processing here
        AddBuffer<ColorBuffer>();
        AddBuffer<MatrixBuffer>();
        AddBuffer<UVCellBuffer>();
        
        args = new uint[5] { 6, 0, 0, 0, 0 };
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);



        mesh = MeshExtension.Quad();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        argsBuffer.Dispose();
    }



    void UpdateBuffers()
    {
        //foreach( var (mat,buffers) in bufferStates_ )
        //{
        //    foreach (var buffer in buffers)
        //    {
        //        buffer.Dispose();
        //    }
        //}
    }

    void WriteToBuffers(Entity e, List<BufferState> states)
    {
        foreach(var state in states)
        {
            WriteToColorBuffer(e, state);
        }
    }

    void WriteToColorBuffer(Entity e, BufferState state)
    {
        var buffer = EntityManager.GetBuffer<ColorBuffer>(e);
        state.Set(buffer.Reinterpret<float4>().AsNativeArray());
    }

    protected override void OnUpdate()
    {
        sharedMaterials.Clear();
        EntityManager.GetAllUniqueSharedComponentData(sharedMaterials);
        sharedMaterials.RemoveAt(0);

        foreach (var sharedMat in sharedMaterials)
        {
            foreach (var (query, bufferType) in bufferQueries_)
            {
                query.SetFilter(sharedMat);
                int count = query.CalculateLength();
                if (count == 0)
                    continue;

                var e = query.GetSingletonEntity();

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
