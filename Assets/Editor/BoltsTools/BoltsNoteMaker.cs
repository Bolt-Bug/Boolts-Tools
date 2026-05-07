using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace editor.BoltsTools
{
    public class BoltsNoteMaker : EditorWindow
    {
        // State
        List<StickyNote> notes = new();
        const string SAVE_PATH = "Assets/Editor/BoltsTools/BoltsNotes.json";
        StickyNote resizingNote;
        Vector2 resizeStart;
        Rect resizeStartRect;
        
        StickyNote lineSourceNote;
        bool isDrawingLine;
        List<NoteLine> lines = new();

        // Pan And Zoom
        Vector2 pan = Vector2.zero;
        float zoom = 1;
        bool isPanning;
        Vector2 lastMousePos;
        
        // Context Menu
        bool showContextMenu;
        Vector2 contextMenuPos;
        Rect contextMenuRect;

        readonly Color[] noteColors =
        {
            new(1, 0.96f, 0.6f), // Yellow
            new(0.6f, 1, 0.7f), //Green
            new(0.6f, 0.85f, 1), // Blue
            new(1, 0.75f, 0.75f), // Pink
            new(0.9f, 0.75f, 1), // Purple
        };

        [MenuItem("Tools/Bolts Tools/Notes &n")]
        public static void Open()
        {
            var win = GetWindow<BoltsNoteMaker>("Notes");
            win.minSize = new(600, 400);
        }

        void OnEnable()
        {
            if (File.Exists(SAVE_PATH))
            {
                var json = File.ReadAllText(SAVE_PATH);
                var wrapper = JsonUtility.FromJson<NoteListWrapper>(json);
                notes = wrapper?.notes ?? new List<StickyNote>();
                lines = wrapper?.lines ?? new List<NoteLine>();
            }
        }

        void OnDisable()
        {
            SaveNotes();
        }

        void SaveNotes()
        {
            var json = JsonUtility.ToJson(new NoteListWrapper { notes = notes, lines = lines}, true);
            File.WriteAllTextAsync(SAVE_PATH, json);
            AssetDatabase.Refresh();
        }

        void OnGUI()
        {
            DrawGrid();
            
            GUI.EndClip();
            var canvasRect = new Rect(0, 0, position.width, position.height);
            GUI.BeginClip(canvasRect);

            var matrix = GUI.matrix;
            GUIUtility.ScaleAroundPivot(Vector2.one * zoom, Vector2.zero);
            GUI.matrix = Matrix4x4.TRS(pan * zoom, Quaternion.identity, Vector3.one) * GUI.matrix;

            foreach (var line in lines)
            {
                if(line.sourceIndex >= notes.Count || line.targetIndex >= notes.Count) continue;
                var a = notes[line.sourceIndex];
                var b = notes[line.targetIndex];
                DrawGradientLine(new Vector2(a.rect.center.x, a.rect.center.y),
                    new Vector2(b.rect.center.x, b.rect.center.y),
                    a.color, b.color);
            }

            if (isDrawingLine && lineSourceNote != null)
            {
                var mouseInCanvas = (Event.current.mousePosition / zoom) - pan;
                DrawGradientLine(
                    new Vector2(lineSourceNote.rect.center.x, lineSourceNote.rect.center.y),
                    mouseInCanvas,
                    lineSourceNote.color, lineSourceNote.color);
                Repaint();
            }
            
            foreach (var note in notes)
                DrawStickyNote(note);

            GUI.matrix = matrix;
            
            HandleEvents();
            
            if(showContextMenu)
                DrawContextMenu();
        }

        void DrawGrid()
        {
            var bgColor = new Color(0.18f, 0.18f, 0.18f);
            EditorGUI.DrawRect(new(0, 0, position.width, position.height), bgColor);

            DrawGridLines(20 * zoom, new Color(1, 1, 1, 0.04f));
            DrawGridLines(100 * zoom, new Color(1, 1, 1, 0.08f));
        }

        void DrawGridLines(float spacing, Color color)
        {
            float offsetX = pan.x * zoom % spacing;
            for(float x = offsetX; x < position.width; x += spacing)
                EditorGUI.DrawRect(new(x, 0,1, position.height), color);
            
            float offsetY = pan.y * zoom % spacing;
            for(float y = offsetY; y < position.width; y += spacing)
                EditorGUI.DrawRect(new(0,y,position.width, 1), color);
        }

        void DrawStickyNote(StickyNote note)
        {
            // Shadow
            var shadow = new Rect(note.rect.x + 4, note.rect.y + 4, note.rect.width, note.rect.height);
            EditorGUI.DrawRect(shadow, new (0, 0, 0, 0.3f));
            
            // Body
            EditorGUI.DrawRect(note.rect, note.color);
            
            // Header Bar
            var header = new Rect(note.rect.x, note.rect.y, note.rect.width, 24);
            EditorGUI.DrawRect(header, new(0, 0, 0, 0.15f));
            
            // Label In Header
            GUI.Label(new Rect(header.x + 6, header.y + 4, header.width - 30, 18),
                "Sticky Note", EditorStyles.boldLabel);
            
            // Delete Button
            var deleteBtn = new Rect(note.rect.xMax - 22, note.rect.y + 3, 18, 18);
            if (GUI.Button(deleteBtn, "X", EditorStyles.label))
            {
                int idx = notes.IndexOf(note);
                lines.RemoveAll(l => l.sourceIndex == idx || l.targetIndex == idx);
                lines = lines.Select(l => new NoteLine
                {
                    sourceIndex = l.sourceIndex > idx ? l.sourceIndex - 1 : l.sourceIndex,
                    targetIndex = l.targetIndex > idx ? l.targetIndex - 1 : l.targetIndex
                }).ToList();
                
                notes.Remove(note);
                
                SaveNotes();
                Repaint();
                GUIUtility.ExitGUI();
                return;
            }
            
            // Text Area
            var textArea = new Rect(note.rect.x + 4, note.rect.y + 28, note.rect.width - 8, note.rect.height - 36);
            var style = new GUIStyle(EditorStyles.textArea)
            {
                wordWrap = true,
                fontSize = 12,
            };
            var newText = GUI.TextArea(textArea, note.text, style);
            if (newText != note.text)
            {
                note.text = newText;
                SaveNotes();
            }
            
            // Resize Handle
            var handle = new Rect(note.rect.xMax - 12, note.rect.yMax - 12, 12, 12);
            EditorGUI.DrawRect(handle, new(0, 0, 0, 0.25f));
            EditorGUIUtility.AddCursorRect(handle, MouseCursor.ResizeUpLeft);
            
            // Color Picker Dots At Bottom
            float dotSize = 10;
            float dotY = note.rect.yMax - 14;
            float dotStartX = note.rect.x + 6;
            for(int i = 0; i < noteColors.Length; i++)
            {
                var dotRect = new Rect(dotStartX + i * 14, dotY, dotSize, dotSize);
                EditorGUI.DrawRect(dotRect, noteColors[i]);
                if (GUI.Button(dotRect, GUIContent.none, GUIStyle.none))
                    note.color = noteColors[i];
            }

            if (Event.current.type == EventType.ContextClick && note.rect.Contains(Event.current.mousePosition))
            {
                // var noteIndex = notes.IndexOf(note);
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Draw Line"), false, () =>
                {
                    lineSourceNote = note;
                    isDrawingLine = true;
                    showContextMenu = false;
                });
                menu.ShowAsContext();
                Event.current.Use();
            }
            
            // Handle Drag And resize
            HandleNoteDrag(note, header, handle);
        }

        void HandleNoteDrag(StickyNote note, Rect header, Rect handle)
        {
            var e = Event.current;
            var mousePos = e.mousePosition;

            if (isDrawingLine)
            {
                if (e.type == EventType.MouseDown && e.button == 0 && note.rect.Contains(e.mousePosition) &&
                    note != lineSourceNote)
                {
                    int srcIdx = notes.IndexOf(lineSourceNote);
                    int dstIdx = notes.IndexOf(note);
                    bool exists = lines.Exists(l =>
                        (l.sourceIndex == srcIdx && l.targetIndex == dstIdx) ||
                        (l.sourceIndex == dstIdx && l.targetIndex == srcIdx));

                    if (!exists)
                    {
                        lines.Add(new NoteLine { sourceIndex = srcIdx, targetIndex = dstIdx });
                        SaveNotes();
                    }

                    isDrawingLine = false;
                    lineSourceNote = null;
                    e.Use();
                }
                
                return;
            }
            
            if (e.type == EventType.MouseDown && handle.Contains(mousePos) && e.button == 0)
            {
                resizingNote = note;
                resizeStart = mousePos;
                resizeStartRect = note.rect;
                e.Use();
            }

            if (resizingNote == note)
            {
                if (e.type == EventType.MouseDrag)
                {
                    var delta = mousePos - resizeStart;
                    note.rect = new Rect(
                        resizeStartRect.x,
                        resizeStartRect.y,
                        Mathf.Max(140, resizeStartRect.width + delta.x),
                        Mathf.Max(100, resizeStartRect.height + delta.y));
                    Repaint();
                    e.Use();
                    SaveNotes();
                }

                if (e.type == EventType.MouseUp)
                {
                    resizingNote = null;
                    e.Use();
                }
            }
            
            // Drag Via Header
            if (e.type == EventType.MouseDown && header.Contains(mousePos) && e.button == 0
                && !handle.Contains(mousePos))
            {
                GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive);
                e.Use();
            }

            if (e.type == EventType.MouseDrag && GUIUtility.hotControl != 0
                                              && header.Contains(new Rect(note.rect.x, note.rect.y, note.rect.width, 24)
                                                  .Contains(mousePos)
                                                  ? mousePos
                                                  : Vector2.one * -999))
            {
                note.rect.position += e.delta / zoom;
                Repaint();
                e.Use();
                SaveNotes();
            }
        }

        void HandleEvents()
        {
            var e = Event.current;

            switch (e.type)
            {
                case EventType.ContextClick:
                case EventType.MouseDown when e.button == 1:
                    contextMenuPos = e.mousePosition;
                    contextMenuRect = new Rect(contextMenuPos.x, contextMenuPos.y, 180, 36);
                    showContextMenu = true;
                    e.Use();
                    break;
                
                case EventType.MouseDown when e.button == 2 || (e.button == 0 && e.alt):
                    isPanning = true;
                    lastMousePos = e.mousePosition;
                    e.Use();
                    break;
                
                case EventType.MouseDrag when isPanning:
                    pan += (e.mousePosition - lastMousePos) / zoom;
                    lastMousePos = e.mousePosition;
                    Repaint();
                    e.Use();
                    break;
                
                case EventType.MouseUp when isPanning:
                    isPanning = false;
                    e.Use();
                    break;
                
                case EventType.ScrollWheel:
                    var zoomDelta = e.delta.y * 0.05f;
                    zoom = Mathf.Clamp(zoom + zoomDelta, 0.3f, 3f);
                    Repaint();
                    e.Use();
                    break;
                
                case EventType.MouseDown when showContextMenu && !contextMenuRect.Contains(e.mousePosition):
                    showContextMenu = false;
                    Repaint();
                    break;
                
                case EventType.KeyDown when e.keyCode == KeyCode.Escape && isDrawingLine:
                    isDrawingLine = false;
                    lineSourceNote = null;
                    Repaint();
                    e.Use();
                    break;
            }
        }

        void DrawContextMenu()
        {
            // Background
            EditorGUI.DrawRect(contextMenuRect, new Color(0.22f, 0.22f, 0.22f));
            // Border
            var border = new Color(0.45f, 0.45f, 0.45f);
            EditorGUI.DrawRect(new Rect(contextMenuRect.x, contextMenuRect.y, contextMenuRect.width, 1), border);
            EditorGUI.DrawRect(new Rect(contextMenuRect.x, contextMenuRect.yMax - 1, contextMenuRect.width, 1), border);
            EditorGUI.DrawRect(new Rect(contextMenuRect.x, contextMenuRect.y, 1, contextMenuRect.height), border);
            EditorGUI.DrawRect(new Rect(contextMenuRect.xMax - 1, contextMenuRect.y, 1, contextMenuRect.height), border);

            var btnRect = new Rect(contextMenuRect.x + 1, contextMenuRect.y + 1, contextMenuRect.width - 2, 34);
            var style = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = Color.white },
                hover = { textColor = Color.white, background = MakeTex(1, 1, new Color(0.3f, 0.5f, 0.9f, 0.5f)) },
                padding = new RectOffset(10, 0, 0, 0),
                fontSize = 12
            };

            if (GUI.Button(btnRect, "📝  Create Sticky Note", style))
            {
                var canvasPos = (contextMenuPos / zoom) - pan;
                notes.Add(new StickyNote(canvasPos));
                
                showContextMenu = false;
                Repaint();
            }
        }

        Texture2D MakeTex(int w, int h, Color col)
        {
            var tex = new Texture2D(w, h);
            tex.SetPixel(0,0, col);
            tex.Apply();
            return tex;
        }

        void DrawGradientLine(Vector2 from, Vector2 to, Color colorA, Color colorB, int segments = 40)
        {
            if(Event.current.type != EventType.Repaint) return;

            var handelColor = Handles.color;
            for (int i = 0; i < segments; i++)
            {
                float t0 = (float)i / segments;
                float t1 = (float)(i + 1) / segments;
                var p0 = Vector2.Lerp(from, to, t0);
                var p1 = Vector2.Lerp(from, to, t1);
                Handles.color = Color.Lerp(colorA, colorB, (t0 + t1) / 2);
                Handles.DrawAAPolyLine(3, new Vector3(p0.x, p0.y), new Vector3(p1.x, p1.y));
            }

            Handles.color = handelColor;
        }
        
        [Serializable]
        public class StickyNote
        {
            public Rect rect;
            public string text;
            public Color color;
            
            public StickyNote(Vector2 position)
            {
                rect = new Rect(position.x, position.y, 200, 150);
                text = "";
                color = new Color(1f, 0.96f, 0.6f);
            }
        }
        
        [Serializable]
        public class NoteListWrapper
        {
            public List<StickyNote> notes;
            public List<NoteLine> lines;
        }
        
        [Serializable]
        public class NoteLine
        {
            public int sourceIndex;
            public int targetIndex;
        }
    }
}
