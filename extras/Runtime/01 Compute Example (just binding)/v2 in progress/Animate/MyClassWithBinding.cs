
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Needle.Timeline.Samples
{
    public class MyClassWithBinding : BindingBaseClass, IAnimated
    {
        [Animate]
        public List<Vector2> Points;

        protected override void OnDispatch(ComputeShaderRunner runner)
        {
            runner.Run("DrawBackground", 0, 0, 0);
            runner.Run("DrawPoints", 0, 0, Points);
        }

        private void OnDrawGizmos()
        {
            if (Points == null) return;
            foreach (var pt in Points)
            {
                Gizmos.DrawSphere(pt, .03f);
            }
        }
    }
}
