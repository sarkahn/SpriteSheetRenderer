using Unity.Entities;
using Unity.Mathematics;

namespace DOTSSpriteRenderer.Components
{
    public struct Position2D : IComponentData
    {
        public float2 value;
        public static implicit operator float2(Position2D p) => p.value;
        public static implicit operator Position2D(float2 v) => new Position2D { value = v };
    }
}