using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;

namespace Needle.Timeline
{
	[CustomTimelineEditor(typeof(CodeControlTrack))]
	public class CustomCurvesEditor : UnityEditor.Timeline.CustomCurvesEditor
	{
		private readonly float lineHeight = EditorGUIUtility.singleLineHeight * 1.1f;

		protected override void OnDrawHeader(Rect rect)
		{
			foreach (var clip in EnumerateClips())
			{
				if (clip.asset is CodeControlAsset code)
				{
					var viewModel = code.viewModel;
					if (viewModel?.clips != null)
					{
						var row = 0;
						foreach (var curves in viewModel.clips)
						{
							if (curves is AnimationCurveWrapper) continue;
							var r = new Rect();
							r.x = rect.x;
							r.width = rect.width;
							r.height = lineHeight;
							r.y = rect.y + r.height * row;

							var backgroundRect = new Rect(r);
							backgroundRect.height -= 2;
							var col = new Color(0, 0, 0, .1f);
							GUI.DrawTexture(backgroundRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 1, col, 0, 0);

							var textRect = new Rect(r);
							textRect.y -= 1.5f;
							textRect.x += 5;
							textRect.width -= 5;
							GUI.Label(textRect, ObjectNames.NicifyVariableName(curves.Name));
							++row;
						}
					}
				}
			}
		}

		private ICustomKeyframe _dragging;

		protected override void OnDrawTrack(Rect rect)
		{
			var useEvent = false;
			var clickedKeyframe = false;
			var isClick = Event.current.type == EventType.KeyUp;
			foreach (var clip in EnumerateClips())
			{
				if (clip.asset is CodeControlAsset code)
				{
					var viewModel = code.viewModel;
					if (viewModel?.clips != null)
					{
						var row = 0;
						foreach (var curves in viewModel.clips)
						{
							if (curves is IKeyframesProvider prov)
							{
								foreach (var kf in prov.Keyframes)
								{
									#region Draw
									var r = new Rect();
									r.x += TimeToPixel(clip.start + kf.time / clip.timeScale);
									r.width = 8;
									r.x -= r.width * .5f;
									r.height = r.width;
									r.y = rect.y + r.height;
									r.y += row * lineHeight;
									var highlight = kf.IsSelected();
									var col = highlight ? Color.yellow : Color.gray;
									col.a = .7f;
									GUI.DrawTexture(r, Texture2D.whiteTexture, ScaleMode.StretchToFill, true,
										1, col, 0, 4);
									#endregion

									if (Event.current.button == 0)
									{
										switch (Event.current.type)
										{
											case EventType.MouseDown:
												if (r.Contains(Event.current.mousePosition))
												{
													useEvent = true;
													_dragging = kf;
												}

												break;
											case EventType.MouseDrag:
												if (_dragging == kf)
												{
													kf.Select(curves);
													var timeDelta = PixelDeltaToDeltaTime(Event.current.delta.x * (float)clip.timeScale);
													kf.time += timeDelta;
													if (kf.time < 0) kf.time = 0;
													Repaint();
													UpdatePreview();
													useEvent = true;
												}

												break;
											case EventType.MouseUp:
												_dragging = null;
												if (r.Contains(Event.current.mousePosition))
												{
													clickedKeyframe = true;
													useEvent = true;
													kf.Select(curves);
												}

												break;
										}
									}
								}

								++row;
							}
						}
					}
				}
			}

			if (isClick && !clickedKeyframe)
				KeyframeInspectorHelper.Deselect();

			if (useEvent)
				Event.current.Use();
		}
	}
}