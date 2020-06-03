using System;
using UnityEditor;
using UnityEngine;
using StateKraft;
using Object = UnityEngine.Object;
using System.Collections.Generic;
using System.Collections;

[CustomPropertyDrawer(typeof(StateMachine))]
public class StateMachineEditor : PropertyDrawer
{
    //Appearance
    private const float TitleHeight = 1.6f;
    private const float BetweenStateEditorLineHeight = 0.5f;
    private const float InspectorTitleHeight = 1.5f;
    private const float StateEditorHorizontalBorder = -0.1f;
    private const float StateEditorTopBorder = -0.05f;
    private const float StateEditorBottomBorder = 0.4f;
    private const float DropAreaHeight = 2.5f;
    // Appearance
    // Title box
    private static readonly GUIStyle TitleBoxStyle = new GUIStyle { fontSize = 14, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, border = new RectOffset(5, 5, 5, 5) };
    // State box
    private static readonly GUIStyle StateNoneBoxStyle = new GUIStyle { fontSize = 13, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, normal = new GUIStyleState { textColor = Color.red }, border = new RectOffset(5, 5, 5, 5) };
    private static readonly GUIStyle StateBoxStyle = new GUIStyle { fontSize = 13, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, border = new RectOffset(5, 5, 5, 5) };
    private static readonly GUIStyle StateBoxBorderStyle = new GUIStyle { fontSize = 13, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, border = new RectOffset(5, 5, 5, 5) };
    // DropArea
    private static readonly GUIStyle DropAreaStyle = new GUIStyle { fontSize = 14, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, border = new RectOffset(15, 15, 15, 15) };

    private readonly Queue<Action<SerializedProperty>> _actionsToPerform = new Queue<Action<SerializedProperty>>();
    private readonly Texture2D[] _texts = Resources.LoadAll<Texture2D>("StateKraft");
    private Editor[] _cachedEditors;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //Gui styles
        StateNoneBoxStyle.normal.background = _texts[2];
        StateBoxStyle.normal.background = _texts[2];
        StateBoxBorderStyle.normal.background = _texts[3];
        TitleBoxStyle.normal.background = _texts[2];

        position.height = EditorGUIUtility.singleLineHeight * TitleHeight;
        //Foldout
        property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, "", true);
        //Title
        GUI.backgroundColor = StateKraftSettings.Colors.Title;
        TitleBoxStyle.normal.textColor = StateKraftSettings.Colors.TitleFont;
        EditorGUI.LabelField(position, new GUIContent(property.displayName), TitleBoxStyle);
        GUI.backgroundColor = StateKraftSettings.Colors.TitleBorder;
        EditorGUI.LabelField(position, new GUIContent(""), StateBoxBorderStyle);
        GUI.backgroundColor = Color.white;
        position.y += EditorGUIUtility.singleLineHeight * TitleHeight;

        SerializedProperty states = property.FindPropertyRelative("_states");
        if(_cachedEditors == null || _cachedEditors.Length != states.arraySize)
            _cachedEditors = new Editor[states.arraySize]; 

        //Run queue
        while (_actionsToPerform.Count > 0)
            _actionsToPerform.Dequeue()(states);

        if (!property.isExpanded)
            return;

        //Background Box 
        Rect box = new Rect(position);
        box.height = GetPropertyHeight(property, label) - EditorGUIUtility.singleLineHeight * TitleHeight;
        //EditorGUI.DrawRect(box, _backgroundColor[_usedTheme]);
        GUI.backgroundColor = StateKraftSettings.Colors.Background;
        EditorGUI.LabelField(box, new GUIContent(""), TitleBoxStyle);
        GUI.backgroundColor = Color.white;

        //Draw all states
        EditorGUI.indentLevel++;
        for (int i = 0; i < states.arraySize; i++)
        {
            position.y += EditorGUIUtility.singleLineHeight * BetweenStateEditorLineHeight;
            position.height += EditorGUIUtility.singleLineHeight * BetweenStateEditorLineHeight;
            SerializedProperty state = states.GetArrayElementAtIndex(i);

            Editor.CreateCachedEditor(state.objectReferenceValue, typeof(StateEditor), ref _cachedEditors[i]);
            StateEditorLogic(ref position, state, i);
            Action callback = null;
            if (EditorApplication.isPlaying)
            {
                object obj = fieldInfo.GetValue(property.serializedObject.targetObject);
                if(obj is StateMachine stateMachine)
                    callback = () => stateMachine.ReinitializeState(state.objectReferenceValue as State);
            }
            DrawStateEditor(ref position, state, _cachedEditors[i], callback);
        }
        //Drop To add state area
        position.y += EditorGUIUtility.singleLineHeight * BetweenStateEditorLineHeight;
        box = new Rect(position);
        box.height = EditorGUIUtility.singleLineHeight * DropAreaHeight;
        GUI.backgroundColor = StateKraftSettings.Colors.DropArea;
        DropAreaStyle.normal.textColor = StateKraftSettings.Colors.DropAreaFont;
        //Drop area
        DropAreaStyle.normal.background = _texts[1];
        EditorGUI.LabelField(box, new GUIContent("Drop State Here"), DropAreaStyle);
        //Dotted area
        DropAreaStyle.normal.background = _texts[0];
        GUI.backgroundColor = Color.white;
        box.x = (position.width / 2f) - 100 + EditorGUI.indentLevel * 15f;
        box.width = 200;
        box.y += box.height * 0.7f * 0.25f;
        box.height = box.height * 0.7f;
        EditorGUI.LabelField(box, new GUIContent(""), DropAreaStyle);
        position.y += EditorGUIUtility.singleLineHeight * BetweenStateEditorLineHeight;
        if (box.Contains(Event.current.mousePosition))
        {
            if (Event.current.type == EventType.DragUpdated)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                Event.current.Use();
            }
            else if (Event.current.type == EventType.DragPerform)
            {
                foreach (Object obj in DragAndDrop.objectReferences)
                {
                    if (obj is State state)
                    {
                        states.InsertArrayElementAtIndex(Mathf.Max(states.arraySize - 1, 0));
                        states.GetArrayElementAtIndex(states.arraySize - 1).objectReferenceValue = obj;
                    }
                    else if (obj is MonoScript script)
                    {
                        Type classType = script.GetClass();
                        if (classType.IsSubclassOf(typeof(State)) && !classType.IsAbstract)
                        {
                            State stateInstance = ScriptableObject.CreateInstance(classType) as State;
                            AssetDatabase.CreateAsset(stateInstance, $"{StateKraftSettings.StatePath}/New State.asset");
                            AssetDatabase.SaveAssets();
                            states.InsertArrayElementAtIndex(Mathf.Max(states.arraySize - 1, 0));
                            states.GetArrayElementAtIndex(states.arraySize - 1).objectReferenceValue = stateInstance;
                        }
                    }
                }
                Event.current.Use();
            }
        }
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = 0;
        SerializedProperty states = property.FindPropertyRelative("_states");
        height += EditorGUIUtility.singleLineHeight * TitleHeight;

        //States height
        if (!property.isExpanded)
            return height;

        //States height
        for (int i = 0; i < states.arraySize; i++)
        {
            Editor editor = Editor.CreateEditor(states.GetArrayElementAtIndex(i).objectReferenceValue);
            height += GetHeightOfStateEditor(states.GetArrayElementAtIndex(i), editor) + EditorGUIUtility.singleLineHeight * BetweenStateEditorLineHeight;
        }

        //Drop area
        height += EditorGUIUtility.singleLineHeight * BetweenStateEditorLineHeight;
        height += EditorGUIUtility.singleLineHeight * DropAreaHeight;
        height += EditorGUIUtility.singleLineHeight * BetweenStateEditorLineHeight;
        return height;
    }

    private void DrawStateEditor(ref Rect position, SerializedProperty property, Editor editor, Action changeCallback, GUIContent context = default)
    {
        if (property.objectReferenceValue == null)
        {
            DrawStateBox(ref position, EditorGUIUtility.singleLineHeight * TitleHeight, Color.red, "Missing Reference");
            position.y += EditorGUIUtility.singleLineHeight * TitleHeight;
            return;
        }
        // Draw background box
        DrawStateBox(ref position, GetHeightOfStateEditor(property, editor, context), StateKraftSettings.Colors.StateBorder);
        // boarder
        position.y += EditorGUIUtility.singleLineHeight * StateEditorTopBorder;
        // Draw TitleBar
        position.height = EditorGUIUtility.singleLineHeight * InspectorTitleHeight;
        property.isExpanded = EditorGUI.InspectorTitlebar(position, property.isExpanded, editor);
        position.y += EditorGUIUtility.singleLineHeight * InspectorTitleHeight;
        
        // Draw the editor
        if (property.isExpanded)
        {
            using (EditorGUI.ChangeCheckScope check = new EditorGUI.ChangeCheckScope())
            {
                DrawEditor(ref position, editor, context);
                if (check.changed)
                    changeCallback?.Invoke();
            }
        }

        // boarder
        position.y += EditorGUIUtility.singleLineHeight * StateEditorBottomBorder; 
    }
    private void StateEditorLogic(ref Rect position, SerializedProperty state, int arrayIndex)
    {
        Event e = Event.current;
        position.height = EditorGUIUtility.singleLineHeight * InspectorTitleHeight;
        if (e.type != EventType.MouseDown || e.button != 1 || !position.Contains(e.mousePosition))
            return;
        //Create context menu
        GenericMenu context = new GenericMenu();
        context.AddItem(new GUIContent("Remove"), false, () => _actionsToPerform.Enqueue((s) => { RemoveStateFromArray(s, arrayIndex); }));
        context.AddItem(new GUIContent("MoveUp"), false, () => _actionsToPerform.Enqueue((s) => { MoveStateInArray(s, arrayIndex, -1); }));
        context.AddItem(new GUIContent("MoveDown"), false, () => _actionsToPerform.Enqueue((s) => { MoveStateInArray(s, arrayIndex, 1); }));
        context.AddSeparator("");
        context.AddItem(new GUIContent("Unfold"), state.isExpanded, () => { state.isExpanded = true; });
        context.AddItem(new GUIContent("Fold"), !state.isExpanded, () => { state.isExpanded = false; });
        context.ShowAsContext();
        //Consume event
        e.Use();
        
        //Local methods
        void RemoveStateFromArray(SerializedProperty stateArray, int index)
        {
            stateArray.GetArrayElementAtIndex(index).objectReferenceValue = null;
            stateArray.DeleteArrayElementAtIndex(index);
        }
        void MoveStateInArray(SerializedProperty stateArray, int index, int direction)
        {
            int destination = (index + stateArray.arraySize + direction) % stateArray.arraySize;
            stateArray.MoveArrayElement(index, destination);
        }
    }
    private float GetHeightOfStateEditor(SerializedProperty property, Editor editor, GUIContent context = default)
    {
        float border = EditorGUIUtility.singleLineHeight * StateEditorBottomBorder  + EditorGUIUtility.singleLineHeight * StateEditorTopBorder;
        if (property.objectReferenceValue == null)
            return EditorGUIUtility.singleLineHeight * TitleHeight + border;

        if (!property.isExpanded)
            return EditorGUIUtility.singleLineHeight * InspectorTitleHeight + border;
        return GetHeightOfEditor(editor, context) + EditorGUIUtility.singleLineHeight * InspectorTitleHeight + border;
    }

    //Editor shit
    private float GetHeightOfEditor(Editor editorToDraw, GUIContent context = default)
    {
        float height = 0f;
        if (editorToDraw == null)
            return height;

        SerializedProperty it = editorToDraw.serializedObject.GetIterator();
        it.NextVisible(true);
        while (it.NextVisible(false))
            height += EditorGUI.GetPropertyHeight(it, context);
        return height;
    }
    private void DrawEditor(ref Rect position, Editor editorToDraw, GUIContent context = default)
    {
        SerializedProperty it = editorToDraw.serializedObject.GetIterator();
        it.NextVisible(true);
        editorToDraw.serializedObject.Update();
        int indent = EditorGUI.indentLevel;
        while (it.NextVisible(false))
        {
            EditorGUI.indentLevel = indent + it.depth;
            position.height = EditorGUI.GetPropertyHeight(it, context);
            EditorGUI.PropertyField(position, it, context, true);
            position.y += EditorGUI.GetPropertyHeight(it);
        }
        if (GUI.changed)
            editorToDraw.serializedObject.ApplyModifiedProperties();
    }
    private void DrawStateBox(ref Rect position, float height, Color borderColor, string text = "")
    {
        Rect box = new Rect(position) { height = height };
        box.width -= EditorGUIUtility.singleLineHeight * StateEditorHorizontalBorder;
        box.x += EditorGUIUtility.singleLineHeight * StateEditorHorizontalBorder;
        GUI.backgroundColor = StateKraftSettings.Colors.State;
        EditorGUI.LabelField(box, new GUIContent(text), StateBoxStyle);
        GUI.backgroundColor = borderColor;
        EditorGUI.LabelField(box, new GUIContent(""), StateBoxBorderStyle);
        GUI.backgroundColor = Color.white;
    }
}
