using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using System.Linq;
using DOTSSpriteRenderer.Components;
using DOTSSpriteRenderer.RenderBuffers;

namespace DOTSSpriteRenderer.Systems
{
    /// <summary>
    /// Generates our render buffer entities. We generate one entity
    /// for each material (Shared Component Data). Ensures the render
    /// buffer entities stays in sync with our materials.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [AlwaysUpdateSystem]
    public class GenerateRenderBuffersSystem : ComponentSystem
    {
        EntityArchetype bufferArchetype;
        EntityQuery bufferQuery;

        static List<SpriteSheetMaterial> sharedMaterials_ = new List<SpriteSheetMaterial>();

        List<ComponentType> buffersToGenerate_ = new List<ComponentType>();

        /// <summary>
        /// Convenience method for adding buffers to the generation list.
        /// </summary>
        void GenBuffer<T>() where T : struct, IBufferElementData
        {
            buffersToGenerate_.Add(ComponentType.ReadOnly<T>());
        }

        protected override void OnCreate()
        {
            GenBuffer<ColorBuffer>();
            GenBuffer<PosBuffer>();
            GenBuffer<ScaleBuffer>();
            GenBuffer<RotBuffer>();
            GenBuffer<UVCellBuffer>();

            var t = new ComponentType[]
            {
                ComponentType.ReadOnly<RenderBufferTag>(),
                ComponentType.ReadOnly<SpriteSheetMaterial>(),
            };
            t = t.Concat(buffersToGenerate_).ToArray();
            bufferQuery = GetEntityQuery(t);
            bufferArchetype = EntityManager.CreateArchetype(t);
        }

        protected override void OnUpdate()
        {
            sharedMaterials_.Clear();
            EntityManager.GetAllUniqueSharedComponentData(sharedMaterials_);
            // Ignore default ( null material )
            sharedMaterials_.RemoveAt(0);

            int bufferCount = bufferQuery.CalculateLength();

            if (bufferCount != sharedMaterials_.Count)
            {
                EntityManager.DestroyEntity(bufferQuery);
                //Debug.Log("Generating buffers");
                for (int i = 0; i < sharedMaterials_.Count; ++i)
                {
                    var e = EntityManager.CreateEntity(bufferArchetype);
                    EntityManager.SetSharedComponentData<SpriteSheetMaterial>(e, sharedMaterials_[i]);
                }
            }
        }
    }
}