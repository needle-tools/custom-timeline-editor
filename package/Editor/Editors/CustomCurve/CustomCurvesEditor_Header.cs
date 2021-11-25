using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Needle.Timeline.CurveEasing;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.UIElements;

namespace Needle.Timeline.Editors.CustomCurve
{
	public class CustomCurvesEditor_Header
	{
		private readonly CustomCurvesEditor customCurvesEditor;
		private GUIStyle typeStyle;
		private GUIStyle recordButtonStyle;
		private readonly StringBuilder builder = new StringBuilder();
		
		private readonly string[] easingTypeOptions;
		private readonly Type[] easingTypes;

		public CustomCurvesEditor_Header(CustomCurvesEditor customCurvesEditor)
		{
			this.customCurvesEditor = customCurvesEditor;

			var types = EasingRegistry.Instance.GetAll();
			easingTypes = types.ToArray();
			easingTypeOptions = easingTypes.Select(e => e.Name).ToArray();
		}

		public void OnDrawHeader(Rect rect)
		{
			if (typeStyle == null)
			{
				typeStyle = new GUIStyle(EditorStyles.label);
				typeStyle.alignment = TextAnchor.MiddleRight;
				var col = typeStyle.normal.textColor;
				col.a = .5f;
				typeStyle.normal.textColor = col;
			}

			var evt = Event.current;
			foreach (var timelineClip in customCurvesEditor.EnumerateClips())
			{
				if (!(timelineClip.asset is CodeControlAsset code)) continue;
				var row = 0;
				foreach (var viewModel in code.viewModels) 
				{
					if (!viewModel.IsValid) continue;
					if (TimelineEditor.inspectedDirector != viewModel.director) continue;
					if (viewModel?.clips != null)
					{
						for (var index = 0; index < viewModel.clips.Count; index++)
						{
							var clip = viewModel.clips[index];
							if (clip is AnimationCurveWrapper) continue;
							var r = new Rect();
							r.x = rect.x;
							r.width = rect.width;
							r.height = customCurvesEditor.lineHeight;
							r.y = rect.y + r.height * row;

							if (index % 2 == 0)
							{
								var backgroundRect = new Rect(r);
								backgroundRect.height -= 2;
								var col = new Color(0, 0, 0, .1f);
								GUI.DrawTexture(backgroundRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 1, col, 0, 0);
							}

							if (evt.IsContextClick(r))
							{
								var menu = new GenericMenu();
								for (var i = 0; i < easingTypeOptions.Length; i++)
								{
									var option = easingTypeOptions[i];
									var type = easingTypes[i];
									var clipIndex = index;
									menu.AddItem(new GUIContent(option), clip.IsCurrentDefaultEasingType(type), () =>
									{
										// apply easing to all clips
										ForAllClips(clipIndex, c =>
										{
											if(c.GetType() == clip.GetType() && c.Name == clip.Name)
												c.SetEasing(type);
										});
									});
								}
								menu.ShowAsContext();
							}

							float endingSpace = 0;
							if (clip is IRecordable rec)
							{
								if (recordButtonStyle == null)
								{
									recordButtonStyle = EditorStyles.FromUSS("trackRecordButton");
									recordButtonStyle.fixedWidth = 0;
									recordButtonStyle.fixedHeight = 0;
								}
								var style = recordButtonStyle;
								if (rec.IsRecording)
								{
									async void RepaintDelayed()
									{
										await Task.Delay(300);
										var window = TimelineEditor.GetWindow();
										if(window)
											window.Repaint();
									}
									RepaintDelayed();
									var remainder = Time.realtimeSinceStartup % 1;
									if (remainder > 0.78f)
										style = GUIStyle.none;
								}
								var recButtonWidth = new Rect(r);
								const float factor = 0.75f;
								recButtonWidth.height = r.height * factor;
								recButtonWidth.width = recButtonWidth.height;
								recButtonWidth.y += r.height * (1 - factor) * .5f;
								recButtonWidth.x = rect.width;
								endingSpace = recButtonWidth.width + 5;
								if (GUI.Button(recButtonWidth, new GUIContent(string.Empty), style))
								{
									var newState = !rec.IsRecording;
									ForAllClips(index, c =>
									{
										if(c is IRecordable r)
											r.IsRecording = newState;
									});
								}
							}

							var tr = new Rect(r);
							tr.y -= 1.5f;
							tr.x += 5;
							tr.width = r.width * .5f;
							GUI.Label(tr, new GUIContent(ObjectNames.NicifyVariableName(clip.Name), viewModel.Script.GetType().Name));

							tr.width = rect.width - 30;
							tr.x += rect.x;
							tr.x -= endingSpace;
							builder.Clear();
							StringHelper.GetGenericsString(clip.GetType(), builder);
							GUI.Label(tr, builder.ToString(), typeStyle);
							++row;
						}
					}
				}
				break;
			}
		}

		private void ForAllClips(int index, Action<ICustomClip> callback)
		{
			foreach (var clip in customCurvesEditor.EnumerateClips())
			{
				if (!(clip.asset is CodeControlAsset code)) continue;
				foreach (var viewModel in code.viewModels)
				{
					if (!viewModel.IsValid) continue;
					if (viewModel.clips.Count <= index) continue;
					var customClip = viewModel.clips[index];
					callback(customClip);
				}
			}
		}
	}
}