using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.LowLevel;

namespace Needle.BetterTransformChanged
{
	public static class PlayerLoopHelper
	{
		public static bool DebugLogs = false;

		public enum Stages
		{
			TimeUpdate,
			Initialization,
			EarlyUpdate,
			FixedUpdate,
			PreUpdate,
			Update,
			PreLateUpdate,
			PostLateUpdate,
		}
		
		public static bool AddUpdateCallback(object obj, PlayerLoopSystem.UpdateFunction callback, Stages stage, int index = int.MaxValue)
		{
			return AddUpdateCallback(obj.GetType(), callback, stage.ToString(), index);
		}

		public static bool AddUpdateCallback(Type type, PlayerLoopSystem.UpdateFunction callback, Stages stage, int index = int.MaxValue)
		{
			return AddUpdateCallback(type, callback, stage.ToString(), index);
		}

		public static bool AddUpdateCallback(Type type, PlayerLoopSystem.UpdateFunction callback, string stage, int index = int.MaxValue)
		{
			var playerLoop = PlayerLoop.GetCurrentPlayerLoop();

			var added = false;
			for (var i = playerLoop.subSystemList.Length - 1; i >= 0; i--)
			{
				var update = playerLoop.subSystemList[i];
				if (update.type.Name != stage) continue;

				var list = new List<PlayerLoopSystem>(update.subSystemList);
				var system = new PlayerLoopSystem
				{
					type = type,
					updateDelegate = callback
				};
				if (index < 0) list.Insert(0, system);
				else if (index < list.Count) list.Insert(index, system);
				else list.Add(system);
				update.subSystemList = list.ToArray();
				playerLoop.subSystemList[i] = update;
				added = true;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				if (DebugLogs)
					Debug.Log("Added update " + type + " to " + stage + " at " + index);
#endif
				break;
			}

			if (!added)
			{
				Debug.LogError("Failed finding update stage " + stage + " to add callback for " + type + ", " + callback);
			}

			PlayerLoop.SetPlayerLoop(playerLoop);
			return added;
		}

		public static void RemoveUpdateDelegate(object obj, PlayerLoopSystem.UpdateFunction callback)
		{
			RemoveUpdateDelegate(obj.GetType(), callback);
		}

		public static void RemoveUpdateDelegate(Type type, PlayerLoopSystem.UpdateFunction callback)
		{
			var playerLoop = PlayerLoop.GetCurrentPlayerLoop();

			PlayerLoopSystem FindAndRemove(PlayerLoopSystem system)
			{
				if (system.subSystemList == null || system.subSystemList.Length <= 0) return system;
				for (var i = system.subSystemList.Length - 1; i >= 0; i--)
				{
					var loop = system.subSystemList[i];
					if (loop.type == type && loop.updateDelegate == callback)
					{
						var list = system.subSystemList.ToList();
						list.RemoveAt(i);
						system.subSystemList = list.ToArray();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
						if (DebugLogs)
							Debug.Log("Removed " + type);
#endif
						continue;
					}

					if (loop.subSystemList != null)
					{
						system.subSystemList[i] = FindAndRemove(loop);
					}
				}

				return system;
			}

			playerLoop = FindAndRemove(playerLoop);
			PlayerLoop.SetPlayerLoop(playerLoop);
		}
	}
}