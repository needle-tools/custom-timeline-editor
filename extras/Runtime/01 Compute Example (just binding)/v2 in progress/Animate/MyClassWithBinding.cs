
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Needle.Timeline.Samples
{
    public struct Point
    {
        public Vector2 Position;
        public Color Color;
    }
    
    [ExecuteAlways]
    public class MyClassWithBinding : MonoBehaviour, IAnimated
    {
        public ComputeShader Shader;
        
        [Animate] public List<Point> Points;
        [TextureInfo(512, 512)] public RenderTexture Result;
        
        public Renderer Renderer;

        protected void Update()
        {
            Shader.Run(this, "DrawBackground", 0, 0, 0);
            Shader.Run(this, "DrawPoints", 0, 0, Points);
            
            // display the texture:
            if (Renderer?.sharedMaterial)
                Renderer.sharedMaterial.mainTexture = Result;
        }
    }
}
