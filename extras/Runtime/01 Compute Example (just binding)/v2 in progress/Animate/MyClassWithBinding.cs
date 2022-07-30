
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Needle.Timeline.Samples
{
    [Serializable]
    public struct Point
    {
        public Vector2 Position;
        public Color Color;
    }
    
    public class MyClassWithBinding : ComputeShaderRunnerUnityComponent, IAnimated
    {
        [Animate]
        public List<Point> Points;
        
        [TextureInfo(512, 512)] 
        public RenderTexture Result;
        
        [Header("Visualization")] 
        public Renderer Renderer;

        protected override void Update()
        {
            if (Runner != null)
            {
                Runner.Run("DrawBackground", 0, 0, 0);
                Runner.Run("DrawPoints", 0, 0, Points);
                ShowTexture();
            }
        }

        private void ShowTexture()
        {
            if (Renderer?.sharedMaterial)
                Renderer.sharedMaterial.mainTexture = Result;
        }

        private void OnDrawGizmos()
        {
            if (Points == null) return;
            foreach (var pt in Points)
            {
                Gizmos.color = pt.Color;
                Gizmos.DrawWireSphere(pt.Position, .03f);
            }
        }

    }
}
