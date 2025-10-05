using UnityEngine;
using UnityEditor;
namespace razz
{
    [CustomEditor(typeof(InteractorNote))]
    public class InteractorNoteEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            InteractorNote noteScript = (InteractorNote)target;
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("📝 Note", EditorStyles.boldLabel);

            string currentNote = (string)noteScript.GetType()
                .GetField("note", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(noteScript);

            GUIStyle textAreaStyle = new GUIStyle(EditorStyles.textArea)
            {
                wordWrap = true
            };

            float availableWidth = EditorGUIUtility.currentViewWidth - 40;
            GUIContent content = new GUIContent(currentNote);
            float calculatedHeight = textAreaStyle.CalcHeight(content, availableWidth);
            calculatedHeight = Mathf.Max(calculatedHeight, EditorGUIUtility.singleLineHeight * 2);

            EditorGUI.BeginChangeCheck();
            string newNote = EditorGUILayout.TextArea(currentNote, textAreaStyle,
                GUILayout.Height(calculatedHeight));

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(noteScript, "Change Note");
                noteScript.GetType().GetField("note", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .SetValue(noteScript, newNote);
                EditorUtility.SetDirty(noteScript);
            }

            EditorGUILayout.EndVertical();
        }
    }
}
