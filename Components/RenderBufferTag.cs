
using Unity.Entities;

namespace DOTSSpriteRenderer.Components
{ 
    [System.Serializable]
    public struct RenderBufferTag : IComponentData
    {
        public bool changed;
    }
}
