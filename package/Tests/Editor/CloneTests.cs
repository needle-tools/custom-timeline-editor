using System.Collections;
using System.Collections.Generic;
using Needle.Timeline;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class CloneTests
{
    [Test]
    public void CloneComputeBuffer()
    {
        var buffer = new ComputeBuffer(1, sizeof(float));
        buffer.SetData(new float[]{42});
        
        var obj = CloneUtil.TryClone(buffer);
        
        Assert.NotNull(obj);
        Assert.IsInstanceOf<ComputeBuffer>(obj);
        Assert.AreNotSame(obj, buffer);
        var arr = new float[1];
        var bufferClone = (ComputeBuffer)obj;
        bufferClone.GetData(arr);
        var res = arr[0];
        Assert.IsTrue(Mathf.Approximately(res, 42));
    }

    [Test]
    public void ListClone()
    {
        var list = new List<int>();
        list.Add(1);
        list.Add(42);

        var obj = CloneUtil.TryClone(list);

        Assert.NotNull(obj);
        Assert.IsInstanceOf<List<int>>(obj);
        Assert.AreNotSame(obj, list);
        var clonedList = obj as List<int>;
        Assert.IsTrue(clonedList![0] == 1);
        Assert.IsTrue(clonedList![1] == 42);
    }
}
