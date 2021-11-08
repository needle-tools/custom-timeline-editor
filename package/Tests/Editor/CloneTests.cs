using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Needle.Timeline.Tests
{
    public class CloneTests
    {
        [Test]
        public void ValueClone()
        {
            var val = 42f;

            var cloned = CloneUtil.TryClone(val);

            Assert.AreNotSame(val, cloned);
            Assert.IsTrue(Mathf.Approximately(val, cloned));
        }

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

        private class MyClass
        {
            public int MyField;
        }

        [Test]
        public void ListReferenceClone()
        {
            var list = new List<MyClass>();
            var entry = new MyClass();
            entry.MyField = 42;
            list.Add(entry);

            var obj = CloneUtil.TryClone(list);

            Assert.NotNull(obj);
            Assert.IsInstanceOf<List<MyClass>>(obj);
            Assert.AreNotSame(obj, list);
            Assert.IsTrue(obj[0] != entry, "Content was not cloned");
            Assert.IsTrue(!ReferenceEquals(obj[0], entry));
            Assert.AreEqual(42, obj[0].MyField);
        }

        private class MyClassWithNestedReference
        {
            public MyClass Nested;
        }

        [Test]
        public void ListReferenceCloneNestedReference()
        {
            var list = new List<MyClassWithNestedReference>();
            var entry = new MyClassWithNestedReference();
            var nested = new MyClass();
            entry.Nested = nested;
            list.Add(entry);

            var obj = CloneUtil.TryClone(list);

            Assert.NotNull(obj);
            Assert.IsInstanceOf<List<MyClassWithNestedReference>>(obj);
            Assert.AreNotSame(obj, list);
            Assert.IsTrue(obj[0] != entry, "Content was not cloned");
            Assert.IsTrue(obj[0].Nested != entry.Nested, "Nested content was not cloned");
            Assert.IsTrue(!ReferenceEquals(obj[0], entry));
            Assert.IsTrue(!ReferenceEquals(obj[0].Nested, entry.Nested), "Nested reference type was not cloned");
        }

        private class MyClassWithNestedList
        {
            public List<MyClass> List;
        }

        [Test]
        public void ListReferenceCloneNestedList()
        {
            var list = new List<MyClassWithNestedList>();
            var entry = new MyClassWithNestedList();
            var nestedList = new List<MyClass>();
            nestedList.Add(new MyClass());
            entry.List = nestedList;
            list.Add(entry);

            var obj = CloneUtil.TryClone(list);

            Assert.NotNull(obj);
            Assert.IsInstanceOf<List<MyClassWithNestedList>>(obj);
            Assert.AreNotSame(obj, list);
            Assert.IsTrue(obj[0] != entry, "Content was not cloned");
            Assert.IsTrue(obj[0].List != entry.List, "Nested content was not cloned");
            Assert.IsTrue(obj[0].List[0] != entry.List[0], "Nested list content was not cloned");
            Assert.IsTrue(!ReferenceEquals(obj[0], entry));
            Assert.IsTrue(!ReferenceEquals(obj[0].List, entry.List), "Nested reference type was not cloned");
            Assert.IsTrue(!ReferenceEquals(obj[0].List[0], entry.List[0]), "Nested list content was not cloned");
        }
    }
}
