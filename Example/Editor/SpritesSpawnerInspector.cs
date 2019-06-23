using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DOTSSpriteRenderer.Examples;

namespace DOTSSpriteRendererEditor.Example.Inspector
{
    [CustomEditor(typeof(SpriteSpawnerTest))]
    public class SpritesSpawnerInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (!Application.isPlaying)
                return;

            if (GUILayout.Button("Spawn"))
                (target as SpriteSpawnerTest).MakeEntities();
        }
    }
}
