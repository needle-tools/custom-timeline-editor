using System;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;

namespace Needle.Timeline
{
	[CustomTimelineEditor(typeof(CodeControlTrack))]
	public class CustomCurvesEditor : UnityEditor.Timeline.CustomCurvesEditor
	{
		private readonly float lineHeight = EditorGUIUtility.singleLineHeight * 1.1f;
		private readonly StringBuilder builder = new StringBuilder();
		private GUIStyle typeStyle;

		protected override void OnDrawHeader(Rect rect)
		{
			if (typeStyle == null)
			{
				typeStyle = new GUIStyle(EditorStyles.label);
				typeStyle.alignment = TextAnchor.MiddleRight;
			}
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

							var tr = new Rect(r);
							tr.y -= 1.5f;
							tr.x += 5;
							tr.width = r.width * .5f;
							GUI.Label(tr, ObjectNames.NicifyVariableName(curves.Name));
							
							tr.width = rect.width - 25;
							tr.x += rect.x;
							builder.Clear();
							StringHelper.GetGenericsString(curves.GetType(), builder);
							GUI.Label(tr, builder.ToString(), typeStyle);
							++row;
						}
					}
				}
			}
		}

		private ICustomKeyframe _dragging, _lastClicked;
		private double _lastKeyframeClickedTime;
		private readonly Color _keySelectedColor = new Color(1, 1, 0, .7f);
		private readonly Color _normalColor = new Color(.8f, .8f, .8f, .4f);
		private readonly Color _assetSelectedColor = new Color(.8f, .8f, .8f, .9f);
		private ICustomKeyframe _copy;

		protected override void OnDrawTrack(Rect rect)
		{
			var useEvent = false;
			var clickedKeyframe = false;
			var isClick = Event.current.type == EventType.MouseUp;
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
									var col = SelectedClip == clip ? _assetSelectedColor : _normalColor;
									if (kf.IsSelected()) col = _keySelectedColor;
									GUI.DrawTexture(r, Texture2D.whiteTexture, ScaleMode.StretchToFill, true,
										1, col, 0, 4);
									#endregion
									
									if (Event.current.button == 0 || Event.current.isKey)
									{
										switch (Event.current.type)
										{
											case EventType.MouseDown:
												if (r.Contains(Event.current.mousePosition))
												{
													useEvent = true;
													_dragging = kf;
													
													var time = DateTime.Now.TimeOfDay.TotalSeconds;
													if (time - _lastKeyframeClickedTime < 1 && _lastClicked == kf)
													{
														viewModel.director.time = kf.time / clip.timeScale + clip.start;
														UpdatePreview();
													}
													_lastKeyframeClickedTime = time;
													_lastClicked = kf;
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
											
											case EventType.KeyDown:
												if (kf.IsSelected())
												{
													switch (Event.current.keyCode)
													{
														case KeyCode.Delete:
															curves.Remove(kf);
															Repaint();
															UpdatePreview();
															return;
														
														case KeyCode.C:
															if ((Event.current.modifiers & EventModifiers.Control) != 0)
															{
																_copy = kf;
															}
															break;
														case KeyCode.V:
															if ((Event.current.modifiers & EventModifiers.Control) != 0)
															{
																if (_copy != null && _copy is ICloneable cloneable)
																{
																	var copy = cloneable.Clone() as ICustomKeyframe;
																	if (copy != null)
																	{
																		copy.time = (float)viewModel.director.time - (float)clip.start;
																		curves.Add(copy);
																		Repaint();
																		UpdatePreview();
																		return;
																	}
																}
															}
															break;
															
													}
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