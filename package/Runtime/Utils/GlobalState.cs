using UnityEditor;
using UnityEngine;

namespace Needle.Timeline
{
	public static class GlobalState
	{
		public static bool GetBool(string id, bool defaultValue)
		{
#if UNITY_EDITOR
			return SessionState.GetBool(id, defaultValue);
#else
			return PlayerPrefs.GetInt(id, defaultValue ? 1 : 0) > 0 ? true : false;
#endif
		}
		
		public static void SetBool(string id, bool value)
		{
#if UNITY_EDITOR
			SessionState.SetBool(id, value);
#else
			PlayerPrefs.SetInt(id, value ? 1 : 0);
#endif
		}
	}
}