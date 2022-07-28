using System;
using System.Collections;
using System.Collections.Generic;
using Needle.Timeline;
using UnityEngine;

public class SimpleScript : MonoBehaviour, IAnimated
{
    public class Point
    {
        public Vector3 Position;
        public float Radius = .1f;
    }
    
    [Animate]
    public List<Point> MyList;

    private void OnDrawGizmos()
    {
        if (MyList == null) return;
        foreach (var pt in MyList)
        {
            Gizmos.DrawSphere(pt.Position, pt.Radius);
        }
    }
}
