//////////////////////////////////////////////////////
// MK Toon Editor Common             			    //
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de                            //
// Copyright © 2020 All rights reserved.            //
//////////////////////////////////////////////////////

#if UNITY_EDITOR
namespace MK.Toon.Editor
{
    internal enum RenderPipeline
    {
        Built_in,
        //Lightweight,
        Universal
    }
    internal enum RenderPipelineUpgrade
    {
        //Lightweight,
        Universal,
        Noise
    }
    internal enum ShaderTemplate
    {
        Unlit,
        Simple,
        PhysicallyBased
    }
    internal enum BlendOpaque
    {
        Default = 0,
        Custom = 4
    };

    [System.Serializable]
    public class ExampleContainer
    {
        public string name = "";
        public UnityEngine.Object scene = null;
        public UnityEngine.Texture2D icon = null;

        public void DrawEditorButton()
        {
            if(UnityEngine.GUILayout.Button(icon, UnityEngine.GUILayout.Width(64), UnityEngine.GUILayout.Height(64)))
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(UnityEditor.AssetDatabase.GetAssetOrScenePath(scene));
        }
    }
}
#endif
