using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Needle.Timeline
{
    public class ModulesTest : MonoBehaviour, IAnimated
    {
        [Animate]
        private List<MyType> MyTypeList;
        
        private struct MyType
        {
            public MyEnum Options;
            public enum MyEnum
            {
                EnumVal0,
                OtherOption
            }

            public Vector3 Position;
        }
    }
}
