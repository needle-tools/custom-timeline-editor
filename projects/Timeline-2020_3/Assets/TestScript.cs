using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Needle.Timeline;
using UnityEngine;

public class TestScript : MonoBehaviour, IAnimated
{
    [Animate, NonSerialized]
    public List<Vector3> Vecs;

    private struct ColorPoint
    {
        public Vector3 Point;
        public Color Color;
        public float Size;
    }

    [Animate]
    private List<ColorPoint> colorPoints;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if(Vecs != null)
            foreach (var vec in Vecs)
                Gizmos.DrawSphere(vec, .1f);

        if (colorPoints != null)
        {
            foreach (var cp in colorPoints)
            {
                Gizmos.color = cp.Color;
                Gizmos.DrawSphere(cp.Point, cp.Size+.01f);
            }
        }
    }
}
