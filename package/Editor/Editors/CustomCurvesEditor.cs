using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;

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

		private bool _isMultiSelectDragging => _dragTrack == Track;
		private Vector2 _startDragPoint;
		private Rect _dragRect;
		private TrackAsset _dragTrack;

		private readonly List<ModifyTime> modifyTimeActions = new List<ModifyTime>();

		private static readonly List<(ICustomClip clip, ICustomKeyframe keyframe)> deletionList = new List<(ICustomClip, ICustomKeyframe)>();
		// private static int? controlId;

		protected override void OnDrawTrack(Rect rect)
		{
			// GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true);
			var useEvent = false;
			var mouseDownOnKeyframe = false;
			var didApplyDeltaToSelectedKeyframes = false;
			var controlId = GUIUtility.GetControlID(FocusType.Passive);
			var evt = Event.current;
			var evtType = evt.GetTypeForControl(controlId);

			switch (evtType)
			{
				case EventType.MouseDown:
					GUIUtility.hotControl = controlId;
					if (_isMultiSelectDragging)
					{
						// first deselect
						var pos = Event.current.mousePosition;
						if (rect.Contains(pos))
						{
							_dragTrack = Track;
							KeyframeSelector.Deselect();
						}
					}
					break;

				case EventType.MouseUp:
					foreach (var mod in modifyTimeActions)
					{
						mod.IsDone = true;
						if (!Mathf.Approximately(mod.keyframe.time, mod.previousTime))
							mod.newTime = mod.keyframe.time;
					}
					CustomUndo.Register(new CompoundCommand(modifyTimeActions) { Name = "Modify Keyframe(s) time", IsDone = true });
					modifyTimeActions.Clear();
					break;

				case EventType.MouseDrag:
					if (_isMultiSelectDragging)
					{
						var pos = Event.current.mousePosition;
						_dragRect.xMin = Mathf.Min(pos.x, _startDragPoint.x);
						_dragRect.yMin = Mathf.Min(pos.y, _startDragPoint.y);
						_dragRect.xMax = Mathf.Max(pos.x, _startDragPoint.x);
						_dragRect.yMax = Mathf.Max(pos.y, _startDragPoint.y);
						useEvent = true;
					}
					break;

				case EventType.Repaint:
					if (_isMultiSelectDragging)
					{
						var col = new Color(.6f, .6f, 1, .2f);
						GUI.DrawTexture(_dragRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 1, col, 0, 0);
					}
					break;
			}

			foreach (var timelineClip in EnumerateClips())
			{
				if (timelineClip.asset is CodeControlAsset code)
				{
					var viewModel = code.viewModel;
					if (viewModel?.clips != null)
					{
						var row = 0;
						foreach (var clip in viewModel.clips)
						{
							if (clip is IKeyframesProvider prov)
							{
								foreach (var kf in prov.Keyframes)
								{
									#region Draw
									var r = new Rect();
									r.x += TimeToPixel(timelineClip.start + kf.time / timelineClip.timeScale);
									r.width = 8;
									r.x -= r.width * .5f;
									r.height = r.width;
									r.y = rect.y + r.height;
									r.y += row * lineHeight;
									if (Event.current.type == EventType.Repaint)
									{
										var col = SelectedClip == timelineClip ? _assetSelectedColor : _normalColor;
										if (kf.IsSelected()) col = _keySelectedColor;
										GUI.DrawTexture(r, Texture2D.whiteTexture, ScaleMode.StretchToFill, true,
											1, col, 0, 4);
									}
									#endregion

									if (evt.button == 0 || evt.isKey)
									{
										switch (evtType)
										{
											case EventType.MouseDown:
												if (r.Contains(evt.mousePosition))
												{
													mouseDownOnKeyframe = true;
													useEvent = true;
													_dragging = kf;

													// double click keyframe?
													var time = DateTime.Now.TimeOfDay.TotalSeconds;
													if (time - _lastKeyframeClickedTime < 1 && _lastClicked == kf)
													{
														viewModel.director.time = kf.time / timelineClip.timeScale + timelineClip.start;
														UpdatePreview();
													}
													_lastKeyframeClickedTime = time;
													_lastClicked = kf;
												}

												break;
											case EventType.MouseDrag:
												if (_dragging == kf)
												{
													var timeDelta = PixelDeltaToDeltaTime(evt.delta.x * (float)timelineClip.timeScale);
													if (KeyframeSelector.SelectionCount > 0 && KeyframeSelector.selectedKeyframes.Any(s => s.Keyframe == kf))
													{
														if (!didApplyDeltaToSelectedKeyframes)
														{
															didApplyDeltaToSelectedKeyframes = true;
															foreach (var entry in KeyframeSelector.EnumerateSelected())
															{
																if(!modifyTimeActions.Any(e => e.keyframe == entry))
																	modifyTimeActions.Add(new ModifyTime(entry));
																entry.time += timeDelta;
																if (kf.time < 0) kf.time = 0;
																Repaint();
																UpdatePreview();
																useEvent = true;
															}
														}
													}
													else
													{
														if(!modifyTimeActions.Any(e => e.keyframe == kf))
															modifyTimeActions.Add(new ModifyTime(kf));
														kf.Select(clip);
														kf.time += timeDelta;
														if (kf.time < 0) kf.time = 0;
														Repaint();
														UpdatePreview();
														useEvent = true;
													}
												}

												break;
											case EventType.MouseUp:
												_dragging = null;

												if (_dragRect.Contains(r.position))
												{
													if (_isMultiSelectDragging)
														kf.Select(clip);
												}
												else if (r.Contains(evt.mousePosition))
												{
													// Debug.Log("Up on keyframe");
													// deselect previously selected
													KeyframeSelector.Deselect();
													kf.Select(clip);
													useEvent = true;
												}

												break;

											case EventType.KeyDown:
												if (kf.IsSelected())
												{
													switch (evt.keyCode)
													{
														case KeyCode.Delete:
															deletionList.Add((clip, kf));
															break;

														case KeyCode.C:
															if ((evt.modifiers & EventModifiers.Control) != 0)
															{
																_copy = kf;
															}
															break;
														case KeyCode.V:
															if ((evt.modifiers & EventModifiers.Control) != 0)
															{
																if (_copy != null && _copy is ICloneable cloneable)
																{
																	if (cloneable.Clone() is ICustomKeyframe copy)
																	{
																		copy.time = (float)viewModel.director.time - (float)timelineClip.start;
																		clip.Add(copy);
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

			if (deletionList.Count > 0)
			{
				foreach (var (clip, keyframe) in deletionList)
				{
					clip.Remove(keyframe);
				}
				deletionList.Clear();
				Repaint();
				UpdatePreview();
			}

			switch (evtType)
			{
				case EventType.MouseDown:
					if (!mouseDownOnKeyframe && !_isMultiSelectDragging)
					{
						_startDragPoint = Event.current.mousePosition;
						_dragRect = Rect.zero;
						_dragTrack = Track;
						KeyframeSelector.Deselect();
					}
					break;

				case EventType.MouseUp:
					var wasDragging = _isMultiSelectDragging && (_dragRect.width > 5 || _dragRect.height > 5) && Track == _dragTrack;
					if (wasDragging && _isMultiSelectDragging) useEvent = true;
					if (_dragTrack == Track) _dragTrack = null;
					break;
			}

			// if (isClick && !mouseUpOnKeyframe)
			// 	KeyframeInspectorHelper.Deselect();

			if (useEvent)
			{
				// Debug.Log("Use " + Event.current.type);
				Event.current.Use();
			}
		}
	}
}