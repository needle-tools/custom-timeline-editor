using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Build.Player;
using UnityEngine;

namespace Needle.Timeline
{
	[CustomEditor(typeof(CodeControlAsset))]
	public class CodeControlAssetInspector : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			var asset = target as CodeControlAsset;
			if (!asset)
			{
				base.OnInspectorGUI();
				return;
			}
			
			if (!asset.data || !EditorUtility.IsPersistent(asset.data))
			{
				// EditorGUILayout.HelpBox("Test", MessageType.Info);
				if(GUILayout.Button("Save as asset", GUILayout.Height(30)))
				{
					var data = CreateInstance<CodeControlAssetData>();
					var path = "Assets/" + asset.id + ".asset";
					data.ClipData = new List<JsonContainer>(asset.clipData);
					asset.data = data;
					AssetDatabase.CreateAsset(data, path);
					AssetDatabase.Refresh();
					Debug.Log("Saved as " + path, data);
				}
			}

			
			base.OnInspectorGUI();


			
			var viewModels = asset?.viewModels;
			if (viewModels == null) return;

			GUILayout.Space(10);
			EditorGUILayout.LabelField("State", EditorStyles.boldLabel);
			EditorGUILayout.LabelField("Name", asset.name, EditorStyles.boldLabel);
			
			var anySolo = viewModels.Any(v => v.Solo);
			using (var change = new EditorGUI.ChangeCheckScope())
			{
				anySolo = EditorGUILayout.Toggle("Solo", anySolo);
				if (change.changed)
				{
					foreach (var vm in viewModels)
					{
						vm.Solo = anySolo;
					}
				}
			}

			GUILayout.Space(10);
			EditorGUILayout.LabelField("ViewModels", EditorStyles.boldLabel);
			foreach (var vm in viewModels)
			{
				EditorGUILayout.LabelField(new GUIContent(vm.Name, vm.Id), new GUIContent(vm.Script.GetType().Name), 
					EditorStyles.boldLabel);
				EditorGUILayout.LabelField(vm.startTime.ToString("0.00"), vm.endTime.ToString("0.00"));
				using (var change = new EditorGUI.ChangeCheckScope())
				{
					vm.Solo = EditorGUILayout.Toggle("Solo", vm.Solo);
					if (change.changed)
					{
						vm.director.Evaluate();
					}
				}
			}
		}
	}
}