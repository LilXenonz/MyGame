using UnityEngine;
using UnityEditor;

namespace razz
{
    [CustomEditor(typeof(HitController))]
    public class HitControllerEditor : Editor
    {
        private HitController hitController;
        private Tool lastTool = Tool.None;
        private bool isEditing = false;
        private SerializedProperty hitList;
        private int selectedHitIndex = -1;
        private static int lastSelectedHitIndex = -1;
        private Vector2 scrollPosition;

        private SerializedProperty interactorProp;
        private SerializedProperty targetInteractorProp;
        private SerializedProperty checkInteractorRulesProp;
        private SerializedProperty hitPivotProp;
        private SerializedProperty hitPivotYOnlyProp;
        private SerializedProperty triggerOnInterruptedHitsProp;
        private SerializedProperty smoothInterruptProp;

        private static bool showPositions = false;
        private static bool showEvents = false;
        private static bool showEasings = false;

        private int _activeRotationAxis = -1;

        private void OnEnable()
        {
            hitController = (HitController)target;
            hitList = serializedObject.FindProperty("hits");
            interactorProp = serializedObject.FindProperty("interactor");
            targetInteractorProp = serializedObject.FindProperty("targetInteractor");
            checkInteractorRulesProp = serializedObject.FindProperty("checkInteractorRules");
            hitPivotProp = serializedObject.FindProperty("hitPivot");
            hitPivotYOnlyProp = serializedObject.FindProperty("hitPivotYOnly");
            triggerOnInterruptedHitsProp = serializedObject.FindProperty("triggerOnInterruptedHits");
            smoothInterruptProp = serializedObject.FindProperty("smoothInterrupt");

            if (lastSelectedHitIndex >= 0 && lastSelectedHitIndex < hitList.arraySize)
            {
                selectedHitIndex = lastSelectedHitIndex;
            }
        }

        private void OnDisable()
        {
            if (isEditing)
            {
                Tools.current = lastTool;
                isEditing = false;
            }
            lastSelectedHitIndex = selectedHitIndex;
        }

        #region Inspector
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            string hitsEnabledTooltip = "Enable or disable all hit animations. Useful for toggling when activated through an event.";
            GUIContent hitsEnabledContent = new GUIContent("Hits Enabled", hitsEnabledTooltip);
            hitController.hitsEnabled = EditorGUILayout.Toggle(hitsEnabledContent, hitController.hitsEnabled);

            string returnInitialPositionTooltip = "Return transforms to their initial hold position after hit sequence to reset. When disabled, the target transform will stay where the hit returns.";
            GUIContent returnInitialPositionContent = new GUIContent("Return Initial Position", returnInitialPositionTooltip);
            hitController.returnInitialPosition = EditorGUILayout.Toggle(returnInitialPositionContent, hitController.returnInitialPosition);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            string triggerTooltip = "Trigger hit events even when hits are interrupted by new ones. An interruption means starting a new hit before the previous one finishes. This setting determines whether to call the previous hit’s events if they didn’t reach their event time.";
            GUIContent triggerContent = new GUIContent("Trigger On Interrupted Hits", triggerTooltip);
            EditorGUILayout.PropertyField(triggerOnInterruptedHitsProp, triggerContent);

            string smoothInterruptTooltip = "If an active hit is interrupted by new one, smoothly transition from the current position to next hit's end position, instead of returning to its start.";
            GUIContent smoothInterruptContent = new GUIContent("Smooth Interrupt", smoothInterruptTooltip);
            EditorGUILayout.PropertyField(smoothInterruptProp, smoothInterruptContent);
            EditorGUILayout.EndHorizontal();

            DrawToolbar();
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            if (hitList.arraySize > 0)
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                DrawHitList();
                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.Space();

            DrawInteractorSettings();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawInteractorSettings()
        {
            string headerTooltip = "Additional settings, mostly for HitReaction setup";
            GUIContent headerContent = new GUIContent("Hitting Another Interactor", headerTooltip);
            EditorGUILayout.LabelField(headerContent, EditorStyles.boldLabel);

            string interactorTooltip = "The Interactor that performs hits";
            GUIContent interactorContent = new GUIContent("Hitting Interactor", interactorTooltip);
            EditorGUILayout.PropertyField(interactorProp, interactorContent);

            EditorGUI.BeginDisabledGroup(interactorProp.objectReferenceValue == null);
            string targetInteractorTooltip = "The target Interactor that will receive hit reactions";
            GUIContent targetInteractorContent = new GUIContent("Target Interactor", targetInteractorTooltip);
            EditorGUILayout.PropertyField(targetInteractorProp, targetInteractorContent);

            string checkRulesTooltip = "Check Interactor orbital rules before allowing hits (checks effector rules to allow hits or not). Also uses the effector type in hit settings.";
            GUIContent checkRulesContent = new GUIContent("Check Interactor Rules", checkRulesTooltip);
            EditorGUILayout.PropertyField(checkInteractorRulesProp, checkRulesContent);

            string hitPivotTooltip = "Transform used as pivot point for hit direction calculations. Will rotate the pivot object towards Interactor for each hit.";
            GUIContent hitPivotContent = new GUIContent("Hit Pivot", hitPivotTooltip);
            EditorGUILayout.PropertyField(hitPivotProp, hitPivotContent);

            string hitPivotYOnlyTooltip = "Rotate the pivot object on only Y axis";
            GUIContent hitPivotYOnlyContent = new GUIContent("Hit Pivot Y Only", hitPivotYOnlyTooltip);
            EditorGUILayout.PropertyField(hitPivotYOnlyProp, hitPivotYOnlyContent);
            EditorGUI.EndDisabledGroup();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Hit", GUILayout.Height(30)))
            {
                int newIndex = hitList.arraySize;
                hitList.arraySize++;

                SerializedProperty newHit = hitList.GetArrayElementAtIndex(newIndex);

                newHit.FindPropertyRelative("hitName").stringValue = $"New Hit {newIndex + 1}";
                newHit.FindPropertyRelative("hitKey").enumValueIndex = (int)KeyCode.Space;
                newHit.FindPropertyRelative("hitColor").colorValue = new Color(0.2f + (newIndex * 0.31f) % 0.8f, 0.3f + (newIndex * 0.47f) % 0.7f, 0.5f + (newIndex * 0.29f) % 0.5f);
                newHit.FindPropertyRelative("hitDuration").floatValue = 0.7f;
                newHit.FindPropertyRelative("returnDuration").floatValue = 0.5f;
                newHit.FindPropertyRelative("returnSpeed").floatValue = 5f;
                newHit.FindPropertyRelative("hitForce").floatValue = 1.0f;
                newHit.FindPropertyRelative("startPosition").vector3Value = new Vector3(0, 0.1f, 0.1f);
                newHit.FindPropertyRelative("startRotation").vector3Value = Vector3.zero;
                newHit.FindPropertyRelative("endPosition").vector3Value = new Vector3(0, 0.1f, -0.1f);
                newHit.FindPropertyRelative("endRotation").vector3Value = Vector3.zero;
                newHit.FindPropertyRelative("startControlPoint").vector3Value = new Vector3(0, 0.1f, 0);
                newHit.FindPropertyRelative("endControlPoint").vector3Value = new Vector3(0, 0.1f, 0);
                newHit.FindPropertyRelative("alternateHitPosition").vector3Value = Vector3.zero;
                newHit.FindPropertyRelative("positionEaseType").enumValueIndex = (int)EaseType.QuadOut;
                newHit.FindPropertyRelative("animationCurve").animationCurveValue = AnimationCurve.EaseInOut(0, 0, 1, 1);
                newHit.FindPropertyRelative("rotationEaseType").enumValueIndex = (int)EaseType.QuadOut;
                newHit.FindPropertyRelative("returnEaseType").enumValueIndex = (int)EaseType.QuadOut;
            }

            GUI.enabled = hitList.arraySize > 0 && selectedHitIndex >= 0;
            if (GUILayout.Button("Remove Selected", GUILayout.Height(30)))
            {
                hitList.DeleteArrayElementAtIndex(selectedHitIndex);
                selectedHitIndex = -1;
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            GUI.enabled = hitList.arraySize > 1 && selectedHitIndex >= 0;
            int newSelectedIndex = EditorGUILayout.IntSlider("Selected Hit", selectedHitIndex >= 0 ? selectedHitIndex : 0, 0, Mathf.Max(0, hitList.arraySize - 1));
            if (GUI.enabled && newSelectedIndex != selectedHitIndex)
            {
                selectedHitIndex = newSelectedIndex;
                ClearEditorFocus();
            }
            GUI.enabled = true;

            EditorGUILayout.Space();
            EditorGUI.BeginChangeCheck();
            float newHandlesSize = EditorGUILayout.Slider("Handles Size", hitController.handlesSize, 0.1f, 8.0f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(hitController, "Change Handles Size");
                hitController.handlesSize = newHandlesSize;
            }

            hitController.showGizmos = selectedHitIndex >= 0;
        }

        private void DrawHitList()
        {
            for (int i = 0; i < hitList.arraySize; i++)
            {
                SerializedProperty hit = hitList.GetArrayElementAtIndex(i);
                bool isSelected = i == selectedHitIndex;

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                DrawHitHeader(i, hit, isSelected);

                if (isSelected)
                {
                    EditorGUI.indentLevel++;
                    DrawHitProperties(hit, i);
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawHitHeader(int index, SerializedProperty hit, bool isSelected)
        {
            var hitName = hit.FindPropertyRelative("hitName");
            var hitKey = hit.FindPropertyRelative("hitKey");
            var hitColor = hit.FindPropertyRelative("hitColor");

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.alignment = TextAnchor.MiddleLeft;
            buttonStyle.fontStyle = isSelected ? FontStyle.Bold : FontStyle.Normal;
            buttonStyle.fontSize = 12;
            buttonStyle.padding = new RectOffset(20, 8, 6, 6);

            Color bgColor = isSelected ? Color.white : new Color(0.9f, 0.9f, 0.9f);
            GUI.backgroundColor = bgColor;

            EditorGUILayout.BeginHorizontal();

            string buttonText = $"{hitName.stringValue} ({hitKey.enumDisplayNames[hitKey.enumValueIndex]})";
            Rect buttonRect = GUILayoutUtility.GetRect(GUIContent.none, buttonStyle, GUILayout.ExpandWidth(true), GUILayout.Height(30));

            bool clicked = GUI.Button(buttonRect, buttonText, buttonStyle);

            Rect colorBarRect = new Rect(buttonRect.x + 4, buttonRect.y + 4, 2, buttonRect.height - 8);
            EditorGUI.DrawRect(colorBarRect, hitColor.colorValue);

            if (clicked)
            {
                int newSelection = (selectedHitIndex == index) ? -1 : index;
                if (newSelection != selectedHitIndex)
                {
                    selectedHitIndex = newSelection;
                    ClearEditorFocus();
                }
            }

            EditorGUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;
        }

        private void ClearEditorFocus()
        {
            GUI.FocusControl(null);
            EditorGUI.FocusTextInControl(null);
            GUIUtility.keyboardControl = 0;
        }

        private void DrawHitProperties(SerializedProperty hit, int index)
        {
            string hitNameTooltip = "Name identifier for this hit animation, just for Editor";
            GUIContent hitNameContent = new GUIContent("Hit Name", hitNameTooltip);
            EditorGUILayout.PropertyField(hit.FindPropertyRelative("hitName"), hitNameContent);

            string hitKeyTooltip = "Key to press for starting this hit animation";
            GUIContent hitKeyContent = new GUIContent("Hit Key", hitKeyTooltip);
            EditorGUILayout.PropertyField(hit.FindPropertyRelative("hitKey"), hitKeyContent);

            string targetTransformTooltip = "Transform to animate during the hit sequence. Can be a hold transform to move a picked-up object, or any other transform for creative solutions.";
            GUIContent targetTransformContent = new GUIContent("Target Transform", targetTransformTooltip);
            EditorGUILayout.PropertyField(hit.FindPropertyRelative("targetTransform"), targetTransformContent);

            EditorGUI.BeginDisabledGroup(interactorProp.objectReferenceValue == null);
            string effectorTooltip = "Effector type used for interaction rule checking to allow hits by effector rules";
            GUIContent effectorContent = new GUIContent("Effector", effectorTooltip);
            EditorGUILayout.PropertyField(hit.FindPropertyRelative("effector"), effectorContent);
            EditorGUI.EndDisabledGroup();

            string hitColorTooltip = "Editor colors used for visual handles and Bézier curves in the Scene view to distinguish hits";
            GUIContent hitColorContent = new GUIContent("Hit Color", hitColorTooltip);
            EditorGUILayout.PropertyField(hit.FindPropertyRelative("hitColor"), hitColorContent);

            EditorGUILayout.Space();
            DrawAnimationSettings(hit);

            DrawCollapsibleSection("Points", ref showPositions, () => DrawPositionSettings(hit, index), "All points responsible for creating the hit animation path");

            DrawCollapsibleSection("Easings", ref showEasings, () => DrawEaseSettings(hit), "Adjust the speed along the path of the hit animation");

            DrawCollapsibleSection("Events", ref showEvents, () => DrawEventSettings(hit), "Event triggered when the hit animation finishes, after reaching the end position and before starting to return");
        }

        private void DrawCollapsibleSection(string title, ref bool isOpen, System.Action drawContent, string tooltip = "")
        {
            EditorGUILayout.BeginHorizontal();
            GUIContent sectionContent = string.IsNullOrEmpty(tooltip) ? new GUIContent(title) : new GUIContent(title, tooltip);
            bool newIsOpen = EditorGUILayout.Foldout(isOpen, sectionContent, true, EditorStyles.foldoutHeader);
            if (newIsOpen != isOpen)
            {
                isOpen = newIsOpen;
            }
            EditorGUILayout.EndHorizontal();

            if (isOpen)
            {
                EditorGUI.indentLevel++;
                drawContent();
                EditorGUI.indentLevel--;
            }
        }

        private void DrawAnimationSettings(SerializedProperty hit)
        {
            EditorGUILayout.LabelField("Animation", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            string hitDurationTooltip = "Duration in seconds for the hit animation phase";
            GUIContent hitDurationContent = new GUIContent("Hit Duration", hitDurationTooltip);
            EditorGUILayout.PropertyField(hit.FindPropertyRelative("hitDuration"), hitDurationContent);

            string returnDurationTooltip = "Duration in seconds for the return animation phase";
            GUIContent returnDurationContent = new GUIContent("Return Duration", returnDurationTooltip);
            EditorGUILayout.PropertyField(hit.FindPropertyRelative("returnDuration"), returnDurationContent);

            string returnSpeedTooltip = "Speed multiplier for the return animation when a new hit interrupts the previous one, to quickly return and start that new one";
            GUIContent returnSpeedContent = new GUIContent("Return Speed", returnSpeedTooltip);
            EditorGUILayout.PropertyField(hit.FindPropertyRelative("returnSpeed"), returnSpeedContent);

            EditorGUI.BeginDisabledGroup(interactorProp.objectReferenceValue == null);
            string hitForceTooltip = "Force value sent to hit reaction component when used";
            GUIContent hitForceContent = new GUIContent("Hit Force", hitForceTooltip);
            EditorGUILayout.PropertyField(hit.FindPropertyRelative("hitForce"), hitForceContent);
            EditorGUI.EndDisabledGroup();

            EditorGUI.indentLevel--;
        }

        private void DrawPositionSettings(SerializedProperty hit, int index)
        {
            EditorGUILayout.LabelField("Positions & Rotations", EditorStyles.miniBoldLabel);
            EditorGUI.indentLevel++;

            string startPositionTooltip = "Local starting position for the hit animation";
            GUIContent startPositionContent = new GUIContent("Start Position", startPositionTooltip);
            EditorGUILayout.PropertyField(hit.FindPropertyRelative("startPosition"), startPositionContent);

            string startRotationTooltip = "Local rotation for the hit animation in euler angles";
            GUIContent startRotationContent = new GUIContent("Start Rotation", startRotationTooltip);
            EditorGUILayout.PropertyField(hit.FindPropertyRelative("startRotation"), startRotationContent);

            EditorGUILayout.Space();

            string endPositionTooltip = "Local end position for the hit animation";
            GUIContent endPositionContent = new GUIContent("End Position", endPositionTooltip);
            EditorGUILayout.PropertyField(hit.FindPropertyRelative("endPosition"), endPositionContent);

            string endRotationTooltip = "Local end rotation for the hit animation in euler angles";
            GUIContent endRotationContent = new GUIContent("End Rotation", endRotationTooltip);
            EditorGUILayout.PropertyField(hit.FindPropertyRelative("endRotation"), endRotationContent);

            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(interactorProp.objectReferenceValue == null);
            string alternateHitPositionTooltip = "Alternative hit point position used for hit reaction calculations. If left at all zeros, this feature is disabled, and the hit end point will be used as the hit reaction point. If values are provided, the hit will use this position for the character’s reaction.";
            GUIContent alternateHitPositionContent = new GUIContent("Alternate Hit Point", alternateHitPositionTooltip);
            EditorGUILayout.PropertyField(hit.FindPropertyRelative("alternateHitPosition"), alternateHitPositionContent);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Assign to Start"))
                AssignTargetToStart(hit);
            if (GUILayout.Button("Assign to End"))
                AssignTargetToEnd(hit);
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel--;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Bezier Control Points", EditorStyles.miniBoldLabel);
            EditorGUI.indentLevel++;

            string startControlPointTooltip = "Start bezier control point offset for curved animation path";
            GUIContent startControlPointContent = new GUIContent("Start Bezier", startControlPointTooltip);
            EditorGUILayout.PropertyField(hit.FindPropertyRelative("startControlPoint"), startControlPointContent);

            string endControlPointTooltip = "End bezier control point offset for curved animation path";
            GUIContent endControlPointContent = new GUIContent("End Bezier", endControlPointTooltip);
            EditorGUILayout.PropertyField(hit.FindPropertyRelative("endControlPoint"), endControlPointContent);

            if (GUILayout.Button("Reset Control Points"))
            {
                Undo.RecordObject(hitController, "Reset Control Points");
                hitController.ResetControlPoints(index);
                serializedObject.ApplyModifiedProperties();
            }
            EditorGUI.indentLevel--;
        }

        private void DrawEventSettings(SerializedProperty hit)
        {
            string onHitEndTooltip = "Event triggered when the hit animation finishes, after reaching the end position and before starting to return";
            GUIContent onHitEndContent = new GUIContent("On Hit End", onHitEndTooltip);
            EditorGUILayout.PropertyField(hit.FindPropertyRelative("onHitEnd"), onHitEndContent);
        }

        private void DrawEaseSettings(SerializedProperty hit)
        {
            string positionEaseTypeTooltip = "Easing type for position animation";
            GUIContent positionEaseTypeContent = new GUIContent("Position Ease Type", positionEaseTypeTooltip);
            EditorGUILayout.PropertyField(hit.FindPropertyRelative("positionEaseType"), positionEaseTypeContent);

            SerializedProperty positionEaseType = hit.FindPropertyRelative("positionEaseType");
            EditorGUI.BeginChangeCheck();
            bool wasCustomCurve = positionEaseType.enumValueIndex == (int)EaseType.CustomCurve;
            if (EditorGUI.EndChangeCheck() && positionEaseType.enumValueIndex == (int)EaseType.CustomCurve && !wasCustomCurve)
            {
                SerializedProperty animationCurveProp = hit.FindPropertyRelative("animationCurve");
                RescaleAnimationCurve(animationCurveProp);
            }

            if (positionEaseType.enumValueIndex == (int)EaseType.CustomCurve)
            {
                SerializedProperty animationCurveProp = hit.FindPropertyRelative("animationCurve");
                string animationCurveTooltip = "Custom animation curve for position easing for more control";
                GUIContent animationCurveContent = new GUIContent("Animation Curve", animationCurveTooltip);
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(animationCurveProp, animationCurveContent);
                if (EditorGUI.EndChangeCheck())
                {
                    RescaleAnimationCurve(animationCurveProp);
                }
            }

            string rotationEaseTypeTooltip = "Easing type for rotation animation";
            GUIContent rotationEaseTypeContent = new GUIContent("Rotation Ease Type", rotationEaseTypeTooltip);
            EditorGUILayout.PropertyField(hit.FindPropertyRelative("rotationEaseType"), rotationEaseTypeContent);

            SerializedProperty returnEaseType = hit.FindPropertyRelative("returnEaseType");
            returnEaseType.enumValueIndex = (int)EaseType.QuadInOut;
        }
        private void RescaleAnimationCurve(SerializedProperty animationCurveProp)
        {
            AnimationCurve curve = animationCurveProp.animationCurveValue;
            Keyframe[] keyframes;
            if (curve == null) keyframes = new Keyframe[0];
            else keyframes = curve.keys;
            if (keyframes.Length < 3)
            {
                keyframes = new Keyframe[3];
                keyframes[0].value = 0;
                keyframes[0].time = 0;
                keyframes[1].value = 1f;
                keyframes[1].time = 1f;
                keyframes[2].value = 0;
                keyframes[2].time = 2f;
            }
            else
            {
                if (keyframes[0].value != 0 || keyframes[0].time != 0)
                {
                    keyframes[0].value = 0;
                    keyframes[0].time = 0;
                }
                bool correctMidPointExist = false;
                for (int i = 1; i < keyframes.Length - 1; i++)
                {
                    if (keyframes[i].value > 1f)
                        keyframes[i].value = 1f;
                    if (keyframes[i].value < 0)
                        keyframes[i].value = 0;
                    if (keyframes[i].time > 1.98f)
                        keyframes[i].time = 1.98f;
                    if (keyframes[i].time < 0.02f)
                        keyframes[i].time = 0.02f;
                    if (keyframes[i].time == 1f && keyframes[i].value == 1f)
                        correctMidPointExist = true;
                }
                if (keyframes[curve.keys.Length - 1].value != 0 || keyframes[curve.keys.Length - 1].time != 2f)
                {
                    keyframes[curve.keys.Length - 1].value = 0;
                    keyframes[curve.keys.Length - 1].time = 2f;
                }
                if (!correctMidPointExist)
                {
                    keyframes[1].time = 1f;
                    keyframes[1].value = 1f;
                }
            }
            curve.keys = keyframes;
            animationCurveProp.animationCurveValue = curve;
        }
        private void AssignTargetToStart(SerializedProperty hit)
        {
            SerializedProperty targetTransform = hit.FindPropertyRelative("targetTransform");
            Transform target = (Transform)targetTransform.objectReferenceValue;

            if (target != null)
            {
                Vector3 localPos = hitController.transform.InverseTransformPoint(target.position);
                Vector3 localRot = (Quaternion.Inverse(hitController.transform.rotation) * target.rotation).eulerAngles;

                hit.FindPropertyRelative("startPosition").vector3Value = localPos;
                hit.FindPropertyRelative("startRotation").vector3Value = localRot;

                serializedObject.ApplyModifiedProperties();
            }
        }

        private void AssignTargetToEnd(SerializedProperty hit)
        {
            SerializedProperty targetTransform = hit.FindPropertyRelative("targetTransform");
            Transform target = (Transform)targetTransform.objectReferenceValue;

            if (target != null)
            {
                Vector3 localPos = hitController.transform.InverseTransformPoint(target.position);
                Vector3 localRot = (Quaternion.Inverse(hitController.transform.rotation) * target.rotation).eulerAngles;

                hit.FindPropertyRelative("endPosition").vector3Value = localPos;
                hit.FindPropertyRelative("endRotation").vector3Value = localRot;

                serializedObject.ApplyModifiedProperties();
            }
        }
        #endregion

        #region Sceneview
        private void OnSceneGUI()
        {
            if (hitList == null || hitList.arraySize == 0 || selectedHitIndex < 0 || !hitController.enabled)
                return;

            SerializedProperty selectedHit = hitList.GetArrayElementAtIndex(selectedHitIndex);
            if (selectedHit == null || !hitController.showGizmos)
                return;

            SerializedProperty targetTransform = selectedHit.FindPropertyRelative("targetTransform");
            Transform transform = (Transform)targetTransform.objectReferenceValue;

            DrawHitHandles(selectedHit, transform);
            DrawBezierCurve(selectedHit, transform);

            if (Event.current.type == EventType.MouseDrag)
            {
                SceneView.RepaintAll();
            }
        }

        private void HandleSceneViewInput()
        {
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                lastTool = Tools.current;
                isEditing = true;
                Tools.current = Tool.None;
            }
            else if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
            {
                isEditing = false;
                Tools.current = lastTool;
            }
        }

        private void DrawHitHandles(SerializedProperty hit, Transform transform)
        {
            EditorGUI.BeginChangeCheck();

            SerializedProperty startPos = hit.FindPropertyRelative("startPosition");
            SerializedProperty endPos = hit.FindPropertyRelative("endPosition");
            SerializedProperty startEuler = hit.FindPropertyRelative("startRotation");
            SerializedProperty endEuler = hit.FindPropertyRelative("endRotation");
            SerializedProperty startControl = hit.FindPropertyRelative("startControlPoint");
            SerializedProperty endControl = hit.FindPropertyRelative("endControlPoint");

            Transform controllerTransform = hitController.transform;
            Vector3 worldStartPos = controllerTransform.TransformPoint(startPos.vector3Value);
            Vector3 worldEndPos = controllerTransform.TransformPoint(endPos.vector3Value);
            Quaternion worldStartRot = controllerTransform.rotation * Quaternion.Euler(startEuler.vector3Value);
            Quaternion worldEndRot = controllerTransform.rotation * Quaternion.Euler(endEuler.vector3Value);

            float handleSize = 0.1f * hitController.handlesSize;
            bool useLocalSpace = Tools.pivotRotation == PivotRotation.Local;

            // Position handles for start position
            Vector3 newWorldStartPos = worldStartPos;
            Quaternion rotationSpace = useLocalSpace ? worldStartRot : Quaternion.identity;

            Handles.color = Color.red;
            Vector3 tempPos = Handles.Slider(newWorldStartPos, rotationSpace * Vector3.right, handleSize, Handles.ArrowHandleCap, 0.01f);
            newWorldStartPos = tempPos;

            Handles.color = Color.green;
            tempPos = Handles.Slider(newWorldStartPos, rotationSpace * Vector3.up, handleSize, Handles.ArrowHandleCap, 0.01f);
            newWorldStartPos = tempPos;

            Handles.color = Color.blue;
            tempPos = Handles.Slider(newWorldStartPos, rotationSpace * Vector3.forward, handleSize, Handles.ArrowHandleCap, 0.01f);
            newWorldStartPos = tempPos;

            // Position handles for end position
            Vector3 newWorldEndPos = worldEndPos;
            rotationSpace = useLocalSpace ? worldEndRot : Quaternion.identity;

            Handles.color = Color.red;
            tempPos = Handles.Slider(newWorldEndPos, rotationSpace * Vector3.right, handleSize, Handles.ArrowHandleCap, 0.01f);
            newWorldEndPos = tempPos;

            Handles.color = Color.green;
            tempPos = Handles.Slider(newWorldEndPos, rotationSpace * Vector3.up, handleSize, Handles.ArrowHandleCap, 0.01f);
            newWorldEndPos = tempPos;

            Handles.color = Color.blue;
            tempPos = Handles.Slider(newWorldEndPos, rotationSpace * Vector3.forward, handleSize, Handles.ArrowHandleCap, 0.01f);
            newWorldEndPos = tempPos;

            // Fixed rotation handles with proper global/local support
            Handles.color = Color.yellow;
            Quaternion rotationHandleSpace = useLocalSpace ? worldStartRot : Quaternion.identity;
            Quaternion newWorldStartRot = DrawFixedSizeRotationHandle(worldStartPos, worldStartRot, handleSize * 1.2f, rotationHandleSpace);

            rotationHandleSpace = useLocalSpace ? worldEndRot : Quaternion.identity;
            Quaternion newWorldEndRot = DrawFixedSizeRotationHandle(worldEndPos, worldEndRot, handleSize * 1.2f, rotationHandleSpace);

            // Control point handles (rest of the code stays the same)
            Vector3 worldStartControl = controllerTransform.TransformPoint(startPos.vector3Value + startControl.vector3Value);
            Vector3 worldEndControl = controllerTransform.TransformPoint(endPos.vector3Value + endControl.vector3Value);

            Vector3 newStartControl = worldStartControl;
            Vector3 newEndControl = worldEndControl;

            rotationSpace = useLocalSpace ? worldStartRot : Quaternion.identity;

            Handles.color = Color.red;
            tempPos = Handles.Slider(newStartControl, rotationSpace * Vector3.right, handleSize * 0.5f, Handles.ArrowHandleCap, 0.01f);
            newStartControl = tempPos;

            Handles.color = Color.green;
            tempPos = Handles.Slider(newStartControl, rotationSpace * Vector3.up, handleSize * 0.5f, Handles.ArrowHandleCap, 0.01f);
            newStartControl = tempPos;

            Handles.color = Color.blue;
            tempPos = Handles.Slider(newStartControl, rotationSpace * Vector3.forward, handleSize * 0.5f, Handles.ArrowHandleCap, 0.01f);
            newStartControl = tempPos;

            rotationSpace = useLocalSpace ? worldEndRot : Quaternion.identity;

            Handles.color = Color.red;
            tempPos = Handles.Slider(newEndControl, rotationSpace * Vector3.right, handleSize * 0.5f, Handles.ArrowHandleCap, 0.01f);
            newEndControl = tempPos;

            Handles.color = Color.green;
            tempPos = Handles.Slider(newEndControl, rotationSpace * Vector3.up, handleSize * 0.5f, Handles.ArrowHandleCap, 0.01f);
            newEndControl = tempPos;

            Handles.color = Color.blue;
            tempPos = Handles.Slider(newEndControl, rotationSpace * Vector3.forward, handleSize * 0.5f, Handles.ArrowHandleCap, 0.01f);
            newEndControl = tempPos;

            Vector3 newAlternatePos = Vector3.zero;
            SerializedProperty alternatePos = hit.FindPropertyRelative("alternateHitPosition");
            if (interactorProp.objectReferenceValue != null && alternatePos.vector3Value != Vector3.zero)
            {
                Vector3 worldAlternatePos = controllerTransform.TransformPoint(alternatePos.vector3Value);
                newAlternatePos = worldAlternatePos;

                Handles.color = Color.magenta;
                tempPos = Handles.Slider(newAlternatePos, Vector3.right, handleSize, Handles.ArrowHandleCap, 0.01f);
                newAlternatePos = tempPos;

                Handles.color = Color.magenta;
                tempPos = Handles.Slider(newAlternatePos, Vector3.up, handleSize, Handles.ArrowHandleCap, 0.01f);
                newAlternatePos = tempPos;

                Handles.color = Color.magenta;
                tempPos = Handles.Slider(newAlternatePos, Vector3.forward, handleSize, Handles.ArrowHandleCap, 0.01f);
                newAlternatePos = tempPos;
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Modified Hit Properties");
                UpdateHandlePositions(controllerTransform, startPos, endPos, startEuler, endEuler,
                    startControl, endControl, alternatePos, worldStartPos, worldEndPos, worldStartControl,
                    worldEndControl, newWorldStartPos, newWorldEndPos, newWorldStartRot,
                    newWorldEndRot, newStartControl, newEndControl, newAlternatePos);
                serializedObject.ApplyModifiedProperties();
            }
        }

        private Quaternion DrawFixedSizeRotationHandle(Vector3 position, Quaternion rotation, float size, Quaternion handleSpace)
        {
            EditorGUI.BeginChangeCheck();

            // X axis (red) - use handleSpace for axis direction
            Handles.color = new Color(1, 0.2f, 0.2f, 0.8f);
            Quaternion xRotation = Handles.Disc(rotation, position, handleSpace * Vector3.right, size, false, 1f);

            // Y axis (green)
            Handles.color = new Color(0.2f, 1, 0.2f, 0.8f);
            Quaternion yRotation = Handles.Disc(xRotation, position, handleSpace * Vector3.up, size, false, 1f);

            // Z axis (blue)
            Handles.color = new Color(0.2f, 0.2f, 1, 0.8f);
            Quaternion finalRotation = Handles.Disc(yRotation, position, handleSpace * Vector3.forward, size, false, 1f);

            if (EditorGUI.EndChangeCheck())
            {
                return finalRotation;
            }

            return rotation;
        }
        // Custom method to draw fixed-size rotation handles
        private Quaternion DrawRotationHandles(Vector3 position, Quaternion rotation, float size)
        {
            // Draw three rotation discs (one for each axis)
            Color xColor = new Color(1, 0.3f, 0.3f, 0.8f); // Red
            Color yColor = new Color(0.3f, 1, 0.3f, 0.8f); // Green
            Color zColor = new Color(0.3f, 0.3f, 1, 0.8f); // Blue

            // Store original rotation
            Quaternion result = rotation;

            // Handle X axis rotation
            Handles.color = xColor;
            EditorGUI.BeginChangeCheck();
            Handles.DrawWireDisc(position, rotation * Vector3.right, size);
            Vector3 xHandlePos = position + (rotation * Vector3.up) * size;
            if (Handles.Button(xHandlePos, rotation, size * 0.1f, size * 0.1f, Handles.SphereHandleCap))
            {
                // Toggle rotation mode for this axis
                _activeRotationAxis = _activeRotationAxis == 0 ? -1 : 0;
            }

            // Handle Y axis rotation
            Handles.color = yColor;
            Handles.DrawWireDisc(position, rotation * Vector3.up, size);
            Vector3 yHandlePos = position + (rotation * Vector3.right) * size;
            if (Handles.Button(yHandlePos, rotation, size * 0.1f, size * 0.1f, Handles.SphereHandleCap))
            {
                _activeRotationAxis = _activeRotationAxis == 1 ? -1 : 1;
            }

            // Handle Z axis rotation
            Handles.color = zColor;
            Handles.DrawWireDisc(position, rotation * Vector3.forward, size);
            Vector3 zHandlePos = position + (rotation * Vector3.up) * size;
            if (Handles.Button(zHandlePos, rotation, size * 0.1f, size * 0.1f, Handles.SphereHandleCap))
            {
                _activeRotationAxis = _activeRotationAxis == 2 ? -1 : 2;
            }

            // If we're actively rotating on an axis
            if (_activeRotationAxis >= 0 && Event.current.type == EventType.MouseDrag)
            {
                Vector3 rotationAxis = Vector3.right;
                if (_activeRotationAxis == 1) rotationAxis = Vector3.up;
                else if (_activeRotationAxis == 2) rotationAxis = Vector3.forward;

                // Rotate based on mouse delta
                float rotationDelta = Event.current.delta.x * 0.5f;
                if (rotationDelta != 0)
                {
                    result = Quaternion.AngleAxis(rotationDelta, rotation * rotationAxis) * rotation;
                    Event.current.Use();
                }
            }

            return result;
        }

        private void DrawBezierCurve(SerializedProperty hit, Transform transform)
        {
            SerializedProperty startPos = hit.FindPropertyRelative("startPosition");
            SerializedProperty endPos = hit.FindPropertyRelative("endPosition");
            SerializedProperty startControl = hit.FindPropertyRelative("startControlPoint");
            SerializedProperty endControl = hit.FindPropertyRelative("endControlPoint");
            SerializedProperty hitColor = hit.FindPropertyRelative("hitColor");

            // Use the controller's transform to get world positions
            Transform controllerTransform = hitController.transform;

            Vector3 worldStartPos = controllerTransform.TransformPoint(startPos.vector3Value);
            Vector3 worldEndPos = controllerTransform.TransformPoint(endPos.vector3Value);
            Vector3 worldStartControl = controllerTransform.TransformPoint(startPos.vector3Value + startControl.vector3Value);
            Vector3 worldEndControl = controllerTransform.TransformPoint(endPos.vector3Value + endControl.vector3Value);

            // Use the custom color instead of fixed yellow
            Handles.color = hitColor.colorValue;
            Handles.SphereHandleCap(0, worldStartPos, Quaternion.identity, 0.04f * hitController.handlesSize, EventType.Repaint);
            Handles.SphereHandleCap(0, worldEndPos, Quaternion.identity, 0.04f * hitController.handlesSize, EventType.Repaint);
            Handles.SphereHandleCap(0, worldStartControl, Quaternion.identity, 0.04f * hitController.handlesSize, EventType.Repaint);
            Handles.SphereHandleCap(0, worldEndControl, Quaternion.identity, 0.04f * hitController.handlesSize, EventType.Repaint);

            Handles.DrawLine(worldStartPos, worldStartControl);
            Handles.DrawLine(worldEndPos, worldEndControl);

            // Draw bezier curve using line segments
            int segments = 20;
            Vector3 previousPoint = worldStartPos;
            for (int i = 1; i <= segments; i++)
            {
                float t = i / (float)segments;
                Vector3 point = EvaluateBezier(worldStartPos, worldStartControl, worldEndControl, worldEndPos, t);
                Handles.DrawLine(previousPoint, point);
                previousPoint = point;
            }

            if (interactorProp.objectReferenceValue != null)
            {
                SerializedProperty alternatePos = hit.FindPropertyRelative("alternateHitPosition");
                if (alternatePos.vector3Value != Vector3.zero)
                {
                    Vector3 worldAlternatePos = controllerTransform.TransformPoint(alternatePos.vector3Value);
                    Handles.color = Color.magenta;
                    Handles.SphereHandleCap(0, worldAlternatePos, Quaternion.identity, 0.06f * hitController.handlesSize, EventType.Repaint);
                }
            }
        }

        private Vector3 EvaluateBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float u = 1f - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;
            return uuu * p0 + 3f * uu * t * p1 + 3f * u * tt * p2 + ttt * p3;
        }

        private void UpdateHandlePositions(Transform transform,
            SerializedProperty startPos, SerializedProperty endPos,
            SerializedProperty startEuler, SerializedProperty endEuler,
            SerializedProperty startControl, SerializedProperty endControl,
            SerializedProperty alternatePos,
            Vector3 worldStartPos, Vector3 worldEndPos,
            Vector3 worldStartControl, Vector3 worldEndControl,
            Vector3 newWorldStartPos, Vector3 newWorldEndPos,
            Quaternion newWorldStartRot, Quaternion newWorldEndRot,
            Vector3 newStartControl, Vector3 newEndControl,
            Vector3 newAlternatePos)
        {
            if (Tools.pivotRotation == PivotRotation.Local)
            {
                Vector3 localStartDelta = transform.InverseTransformVector(newWorldStartPos - worldStartPos);
                Vector3 localEndDelta = transform.InverseTransformVector(newWorldEndPos - worldEndPos);
                startPos.vector3Value += localStartDelta;
                endPos.vector3Value += localEndDelta;

                Vector3 localStartControlDelta = transform.InverseTransformVector(newStartControl - worldStartControl);
                Vector3 localEndControlDelta = transform.InverseTransformVector(newEndControl - worldEndControl);
                startControl.vector3Value += localStartControlDelta;
                endControl.vector3Value += localEndControlDelta;

                if (interactorProp.objectReferenceValue != null && alternatePos.vector3Value != Vector3.zero)
                {
                    Vector3 worldAlternatePos = transform.TransformPoint(alternatePos.vector3Value);
                    Vector3 localAlternateDelta = transform.InverseTransformVector(newAlternatePos - worldAlternatePos);
                    alternatePos.vector3Value += localAlternateDelta;
                }
            }
            else
            {
                startPos.vector3Value = transform.InverseTransformPoint(newWorldStartPos);
                endPos.vector3Value = transform.InverseTransformPoint(newWorldEndPos);
                startControl.vector3Value = transform.InverseTransformPoint(newStartControl) - startPos.vector3Value;
                endControl.vector3Value = transform.InverseTransformPoint(newEndControl) - endPos.vector3Value;

                if (interactorProp.objectReferenceValue != null && alternatePos.vector3Value != Vector3.zero)
                {
                    alternatePos.vector3Value = transform.InverseTransformPoint(newAlternatePos);
                }
            }

            startEuler.vector3Value = (Quaternion.Inverse(transform.rotation) * newWorldStartRot).eulerAngles;
            endEuler.vector3Value = (Quaternion.Inverse(transform.rotation) * newWorldEndRot).eulerAngles;
        }
        #endregion
    }
}
