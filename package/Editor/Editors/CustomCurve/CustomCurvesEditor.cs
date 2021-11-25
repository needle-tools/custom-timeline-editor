using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Needle.Timeline.Commands;
using Needle.Timeline.CurveEasing;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;

namespace Needle.Timeline.Editors.CustomCurve
{
	[CustomTimelineEditor(typeof(CodeControlTrack))]
	public class CustomCurvesEditor : UnityEditor.Timeline.CustomCurvesEditor
	{
		internal readonly float lineHeight = EditorGUIUtility.singleLineHeight * 1.1f;

		protected override void OnDrawHeader(Rect rect)
		{
			customCurvesEditorHeader.OnDrawHeader(rect);
		}

		private static object _dragging, _lastClicked;
		private double _lastKeyframeClickedTime;
		private readonly Color _keySelectedColor = new Color(1, 1, 0, .7f);
		private readonly Color _normalColor = new Color(.8f, .8f, .8f, .4f);
		private readonly Color _assetSelectedColor = new Color(.8f, .8f, .8f, .9f);
		private ICustomKeyframe _copy;
		private ICustomClip _copyClip;

		private bool _isMultiSelectDragging => _dragTrack == Track;
		private static Vector2 _startDragPoint;
		private static Rect _dragRect;
		private static TrackAsset _dragTrack;

		private readonly List<KeyframeModifyTime> modifyTimeActions = new List<KeyframeModifyTime>();
		private static readonly List<DeleteKeyframe> deletionList = new List<DeleteKeyframe>();
		private readonly List<ICommand> dragActions = new List<ICommand>();

		private readonly CustomCurvesEditor_Header customCurvesEditorHeader;

		public CustomCurvesEditor()
		{
			customCurvesEditorHeader = new CustomCurvesEditor_Header(this); 
		}

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
					if (rect.Contains(mousePos))
					{
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

					if (dragActions.Count > 0)
					{
						var compound = dragActions.ToCompound("Modified Keyframes", true);
						CustomUndo.Register(compound);
						dragActions.Clear();
					}
					
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
						if (!viewModel.IsValid) continue;
						if (TimelineEditor.inspectedDirector != viewModel.director) continue;
						if (viewModel?.clips == null) continue;
						foreach (var clip in viewModel.clips)
						{
							if (OnRenderClip(rect, clip, timelineClip, row, viewModel, 
								    ref didApplyDeltaToSelectedKeyframes,
								    ref mouseDownOnKeyframe
								    )) 
								return;
							++row;
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
					if (!mouseDownOnKeyframe && !_isMultiSelectDragging && rect.Contains(mousePos))
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

		private int _markerDraggingIndex;
		
		
		private bool OnRenderClip(Rect rect,
			ICustomClip clip,
			TimelineClip timelineClip,
			int row,
			ClipInfoViewModel viewModel,
			ref bool didApplyDeltaToSelectedKeyframes,
			ref bool mouseDownOnKeyframe)
		{
			var evt = Event.current;
			var evtType = evt.type;
			Rect lastKeyframeRect = Rect.zero;
			for (var index = 0; index < clip.Keyframes.Count; index++)
			{
				var kf = clip.Keyframes[index];
				if (!GetIsInKeyframeRect(rect, timelineClip, kf, row, out var keyframeRect))
					continue;

				
				var keyframe = kf as ICustomKeyframe;
				if (keyframe == null) continue;

				if (index >= 1)
				{
					var prev = clip.Keyframes[index-1] as ICustomKeyframe;
					var weight = prev.GetWeight(keyframe);
					weight = Mathf.Clamp(weight, .03f, .97f);
					var dist = keyframeRect.x - lastKeyframeRect.x;
					var middle = lastKeyframeRect.x + dist * weight;
					var r = new Rect(keyframeRect);
					r.x = middle;
					// r.y += r.height * .25f;
					// r.height *= .5f;
					// r.width *= .5f;
					GUI.DrawTexture(r, Texture2D.whiteTexture, ScaleMode.StretchToFill, true,
						1, new Color(.5f,.5f,.5f, .3f), 0, 4);
					if (evt.button == 0 || evt.isKey)
					{
						switch (evtType)
						{
							case EventType.MouseDown:
								if (r.Contains(evt.mousePosition))
								{
									dragActions.Add(new KeyframeModifyEasing(prev));
									dragActions.Add(new KeyframeModifyEasing(keyframe));
									// TODO: figure out how we unify these actions across various things that can receive input on a clip ui view
									_dragging = clip;
									_markerDraggingIndex = index;
									mouseDownOnKeyframe = true;
								}
								break;
							case EventType.MouseUp:
								if(_dragging == clip)
									_dragging = null;
								break;
							case EventType.MouseDrag:
								if (_dragging == clip && _markerDraggingIndex == index)
								{
									var weightChange = evt.delta.x / dist;
									prev.AddWeightChange(keyframe, weightChange);
									clip.RaiseChangedEvent();
								}
								break;
						}
					}
				}
				lastKeyframeRect = keyframeRect;

				if (evt.button == 0 || evt.isKey)
				{
					switch (evtType)
					{
						case EventType.MouseDown:
							if (keyframeRect.Contains(evt.mousePosition))
							{
								_dragging = keyframe;
								mouseDownOnKeyframe = true;

								if (!kf.IsSelected())
									KeyframeSelector.Deselect();
								keyframe.Select(clip);

								// double click keyframe?
								var time = DateTime.Now.TimeOfDay.TotalSeconds;
								if (time - _lastKeyframeClickedTime < 1 && _lastClicked == kf)
								{
									var newTime = kf.time / timelineClip.timeScale + timelineClip.start;
									CustomUndo.Register(new TimelineModifyTime(viewModel.director, newTime));
									UpdatePreview();
								}
								_lastKeyframeClickedTime = time;
								_lastClicked = keyframe;
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
										}
									}
								}
								else
								{
									if (!modifyTimeActions.Any(e => e.keyframe == kf))
										modifyTimeActions.Add(new KeyframeModifyTime(keyframe));
									keyframe.Select(clip);
									keyframe.time += timeDelta;
									if (kf.time < 0) keyframe.time = 0;
									Repaint();
									UpdatePreview();
								}
							}

							break;
						case EventType.MouseUp:
							_dragging = null;

							if (_dragRect.Contains(keyframeRect.position))
							{
								if (_isMultiSelectDragging)
									keyframe.Select(clip);
							}
							else if (keyframeRect.Contains(evt.mousePosition))
							{
								// Debug.Log("Up on keyframe");
								// deselect previously selected
								KeyframeSelector.Deselect();
								keyframe.Select(clip);
							}

							break;

						case EventType.KeyDown:
							switch (evt.keyCode)
							{
								case KeyCode.Delete:
									if (kf.IsSelected())
										deletionList.Add(new DeleteKeyframe(keyframe, clip));
									break;

								case KeyCode.C:
									if (kf.IsSelected() && (evt.modifiers & EventModifiers.Control) != 0)
									{
										_copy = keyframe;
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
													return true;
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
			return false;
		}

		private bool GetIsInKeyframeRect(Rect rect, TimelineClip timelineClip, IReadonlyCustomKeyframe kf, int row, out Rect keyframeRect)
		{
			keyframeRect = new Rect();
			keyframeRect.x += TimeToPixel(timelineClip.start + kf.time / timelineClip.timeScale);
			keyframeRect.width = 8;
			keyframeRect.x -= keyframeRect.width * .5f;
			keyframeRect.height = keyframeRect.width;
			keyframeRect.y = rect.y + keyframeRect.height;
			keyframeRect.y += row * lineHeight;
			if (!rect.Contains(keyframeRect.max) && !rect.Contains(keyframeRect.min)) 
				return false;
			DoDrawKeyframe(timelineClip, kf, keyframeRect);
			return true;
		}

		private void DoDrawKeyframe(TimelineClip timelineClip, IReadonlyCustomKeyframe kf, Rect keyframeRect)
		{
			if (Event.current.type != EventType.Repaint) return;
			var col = SelectedClip == timelineClip ? _assetSelectedColor : _normalColor;
			if (kf.IsSelected()) col = _keySelectedColor;
			GUI.DrawTexture(keyframeRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true,
				1, col, 0, 4);
		}
		
		
	}
}