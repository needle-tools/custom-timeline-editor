using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Needle.TransformExtensions
{
    public static class TransformExtensions
    {
        public static void OnHasChanged(this Transform t, Action callback, PlayerLoopEvent evt = PlayerLoopEvent.Update)
        {
            if (!registered.ContainsKey(evt))
            {
                registered.Add(evt, new List<(Transform t, Action cb)>());
                PlayerLoopHelper.AddUpdateCallback(typeof(Transform), () => OnEvt(evt), evt, evt == lastEvent ? 0 : int.MaxValue);
            }
            var list = registered[evt];
            var val = (t, callback);
            if (!list.Contains(val))
                list.Add(val);
        }

        private const PlayerLoopEvent lastEvent = PlayerLoopEvent.PostLateUpdate;

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#endif
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            PlayerLoopHelper.AddUpdateCallback(typeof(TransformExtensions), OnEndOfFrame, lastEvent);
        }

        private static void OnEndOfFrame()
        {
            foreach (var e in registered)
            {
                foreach (var l in e.Value)
                {
                    l.t.hasChanged = false;
                }
            }
        }

        private static void OnEvt(PlayerLoopEvent evt)
        {
            var list = registered[evt];
            for (var index = 0; index < list.Count; index++)
            {
                var e = list[index];
                if (!e.t)
                {
                    removeList.Add(index);
                }
                else if (e.t.hasChanged)
                {
                    e.cb();
                }
            }
            for (var index = removeList.Count - 1; index >= 0; index--)
            {
                var r = removeList[index];
                list.RemoveAt(r);
            }
            removeList.Clear();
        }

        private static readonly List<int> removeList = new List<int>();

        private static readonly Dictionary<PlayerLoopEvent, List<(Transform t, Action cb)>> registered =
            new Dictionary<PlayerLoopEvent, List<(Transform t, Action)>>();
    }
}