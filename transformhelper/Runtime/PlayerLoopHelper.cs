using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.LowLevel;

namespace Needle.TransformExtensions
{
	public enum PlayerLoopEvent
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
	
	public static class PlayerLoopHelper
	{
		public static bool DebugLogs = false;

		
		public static bool AddUpdateCallback(object obj, PlayerLoopSystem.UpdateFunction callback, PlayerLoopEvent playerLoopEvent, int index = int.MaxValue)
		{
			return AddUpdateCallback(obj.GetType(), callback, playerLoopEvent.ToString(), index);
		}

		public static bool AddUpdateCallback(Type type, PlayerLoopSystem.UpdateFunction callback, PlayerLoopEvent playerLoopEvent, int index = int.MaxValue)
		{
			return AddUpdateCallback(type, callback, playerLoopEvent.ToString(), index);
		}

		private static PlayerLoopSystem? _defaultPlayerLoopSystem;

		public static bool AddUpdateCallback(Type type, PlayerLoopSystem.UpdateFunction callback, string stage, int index = int.MaxValue)
		{
			_defaultPlayerLoopSystem ??= PlayerLoop.GetDefaultPlayerLoop();
			if (_defaultPlayerLoopSystem == null) return false;
			var added = false;  
			for (var i = _defaultPlayerLoopSystem.Value.subSystemList.Length - 1; i >= 0; i--)
			{
				var update = _defaultPlayerLoopSystem.Value.subSystemList[i];
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
				_defaultPlayerLoopSystem.Value.subSystemList[i] = update;
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

			PlayerLoop.SetPlayerLoop(_defaultPlayerLoopSystem.Value);
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