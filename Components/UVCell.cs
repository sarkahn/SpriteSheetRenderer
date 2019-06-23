using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;


namespace DOTSSpriteRenderer.Components
{
    [System.Serializable]
    public struct UVCell : IComponentData
    {
        public int value;
    }
}