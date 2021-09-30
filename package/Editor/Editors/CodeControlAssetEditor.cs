using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Build.Player;
using UnityEngine;

namespace Needle.Timeline
{
	[CustomEditor(typeof(CodeControlAsset))]
	public class CodeControlAssetEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			var asset = target as CodeControlAsset;
			
			var viewModels = asset?.viewModels;
			if (viewModels == null) return;
			
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