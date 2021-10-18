using System;
using System.Collections.Generic;
using System.ComponentModel;
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
		private GUIStyle recordButtonStyle;

		protected override void OnDrawHeader(Rect rect)
		{
			if (typeStyle == null)
			{
				typeStyle = new GUIStyle(EditorStyles.label);
				typeStyle.alignment = TextAnchor.MiddleRight;
			}

			void ForAllClips(int index, Action<ICustomClip> callback)
			{
				foreach (var clip in EnumerateClips())
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
			
			foreach (var clip in EnumerateClips())
			{
				if (!(clip.asset is CodeControlAsset code)) continue;
				var row = 0;
				foreach (var viewModel in code.viewModels)
				{
					if (viewModel?.clips != null)
					{
						for (var index = 0; index < viewModel.clips.Count; index++)
						{
							var curves = viewModel.clips[index];
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


							float endingSpace = 0;
							if (curves is IRecordable rec)
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
									TimelineEditor.GetWindow().Repaint();
									var remainder = Time.realtimeSinceStartup % 1;
									if (remainder < 0.22f)
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
							GUI.Label(tr, new GUIContent(ObjectNames.NicifyVariableName(curves.Name), viewModel.Script.GetType().Name));

							tr.width = rect.width - 30;
							tr.x += rect.x;
							tr.x -= endingSpace;
							builder.Clear();
							StringHelper.GetGenericsString(curves.GetType(), builder);
							GUI.Label(tr, builder.ToString(), typeStyle);
							++row;
						}
					}
				}
				break;
			}
		}

		private ICustomKeyframe _dragging, _lastClicked;
		private double _lastKeyframeClickedTime;
		private readonly Color _keySelectedColor = new Color(1, 1, 0, .7f);
		private readonly Color _normalColor = new Color(.8f, .8f, .8f, .4f);
		private readonly Color _assetSelectedColor = new Color(.8f, .8f, .8f, .9f);
		private ICustomKeyframe _copy;
		private ICustomClip _copyClip;

		private bool _isMultiSelectDragging => _dragTrack == Track;
		private Vector2 _startDragPoint;
		private Rect _dragRect;
		private TrackAsset _dragTrack;

		private readonly List<KeyframeModifyTime> modifyTimeActions = new List<KeyframeModifyTime>();

		private static readonly List<DeleteKeyframe> deletionList = new List<DeleteKeyframe>();
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
			var mousePos = Event.current.mousePosition;

			switch (evtType)
			{
				case EventType.MouseDown:
					GUIUtility.hotControl = controlId;
					// first deselect
					if (rect.Contains(mousePos))
					{
						if (_isMultiSelectDragging)
						{
							_dragTrack = Track;
							KeyframeSelector.Deselect();
						}
					}
					break;
				case EventType.MouseUp:
					for (var index = modifyTimeActions.Count - 1; index >= 0; index--)
					{
						var mod = modifyTimeActions[index];
						mod.IsDone = true;
						if (Mathf.Approximately(mod.keyframe.time, mod.previousTime))
							modifyTimeActions.RemoveAt(index);
						else mod.newTime = mod.keyframe.time;
					}
					if (modifyTimeActions.Count > 0)
						CustomUndo.Register(modifyTimeActions.ToCompound("Modify Keyframe(s) time", true));
					modifyTimeActions.Clear();
					break;

				case EventType.MouseDrag:
					if (_isMultiSelectDragging)
					{
						_dragRect.xMin = Mathf.Min(mousePos.x, _startDragPoint.x);
						_dragRect.yMin = Mathf.Min(mousePos.y, _startDragPoint.y);
						_dragRect.xMax = Mathf.Max(mousePos.x, _startDragPoint.x);
						_dragRect.yMax = Mathf.Max(mousePos.y, _startDragPoint.y);
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
					var row = 0;
					foreach (var viewModel in code.viewModels)
					{
						if (viewModel?.clips != null)
						{
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
										if (!rect.Contains(r.max) && !rect.Contains(r.min)) continue;
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

														if (!kf.IsSelected())
															KeyframeSelector.Deselect();
														kf.Select(clip);

														// double click keyframe?
														var time = DateTime.Now.TimeOfDay.TotalSeconds;
														if (time - _lastKeyframeClickedTime < 1 && _lastClicked == kf)
														{
															var newTime = kf.time / timelineClip.timeScale + timelineClip.start;
															CustomUndo.Register(new TimelineModifyTime(viewModel.director, newTime));
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
														if (KeyframeSelector.SelectionCount > 0 &&
														    KeyframeSelector.selectedKeyframes.Any(s => s.Keyframe == kf))
														{
															if (!didApplyDeltaToSelectedKeyframes)
															{
																didApplyDeltaToSelectedKeyframes = true;
																var canPerform = true;
																foreach (var entry in KeyframeSelector.EnumerateSelected())
																{
																	if (!canPerform) break;
																	if (entry.time + timeDelta < 0)
																	{
																		canPerform = false;
																	}
																}
																foreach (var entry in KeyframeSelector.EnumerateSelected())
																{
																	if (!canPerform) break;
																	if (!modifyTimeActions.Any(e => e.keyframe == entry))
																		modifyTimeActions.Add(new KeyframeModifyTime(entry));
																	entry.time += timeDelta;
																	Repaint();
																	UpdatePreview();
																	useEvent = true;
																}
															}
														}
														else
														{
															if (!modifyTimeActions.Any(e => e.keyframe == kf))
																modifyTimeActions.Add(new KeyframeModifyTime(kf));
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
													switch (evt.keyCode)
													{
														case KeyCode.Delete:
															if (kf.IsSelected())
																deletionList.Add(new DeleteKeyframe(kf, clip));
															break;

														case KeyCode.C:
															if (kf.IsSelected() && (evt.modifiers & EventModifiers.Control) != 0)
															{
																_copy = kf;
																_copyClip = clip;
															}
															break;
														case KeyCode.V:
															if ((evt.modifiers & EventModifiers.Control) != 0)
															{
																if (_copy != null && _copy is ICloneable cloneable && clip == _copyClip)
																{
																	if (viewModel.currentlyInClipTime)
																	{
																		if (cloneable.Clone() is ICustomKeyframe copy)
																		{
																			copy.time = (float)viewModel.clipTime;
																			CustomUndo.Register(new CreateKeyframe(copy, clip));
																			Repaint();
																			UpdatePreview();
																			return;
																		}
																	}
																}
															}
															break;
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
			}

			if (deletionList.Count > 0)
			{
				if (deletionList.Any(c => c.IsValid))
					CustomUndo.Register(deletionList.ToCompound("Delete Keyframes"));
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