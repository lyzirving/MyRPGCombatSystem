using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

[CustomEditor(typeof(AnimationEventTrigger))]
public class AnimationEventTriggerEditor : Editor
{
    private AnimationClip m_PreviewClip;
    private bool m_PreviewAnimation = false;
    private float m_PreviewTime = 0f;
    private int m_PreviewSelection = 0;
    private int m_RemoveSelection = 0;

    public override void OnInspectorGUI()
    {        
        AnimationEventTrigger behaviour = (AnimationEventTrigger)target;
        DrawBehaviorAttrs(behaviour);

        if (!Validate(behaviour, out string errorMessage))
        {
            EditorGUILayout.HelpBox(errorMessage, MessageType.Info);
            return;
        }

        // Make TextField readonly
        GUI.enabled = false;
        EditorGUILayout.TextField("Current Animation", m_PreviewClip != null ? m_PreviewClip.name : "null");
        GUI.enabled = true;

        GUILayout.Space(10);
        bool preview = m_PreviewAnimation;
        m_PreviewAnimation = EditorGUILayout.Toggle("Preview Animation", m_PreviewAnimation);

        // Selection change
        if (m_PreviewAnimation != preview && !m_PreviewAnimation)
        {
            EnforceTPose();
            return;
        }

        if (GUILayout.Button("Enforce T-Pose"))
        {
            EnforceTPose();
        }

        if (m_PreviewAnimation)
        {
            EditorGUILayout.HelpBox("Please enable Preview in Animation window, and drag LaunchTime to see realtime animation change in Scene.", MessageType.Info);
            PreviewAnimationClip(behaviour);
            GUILayout.Label($"Previewing at {m_PreviewTime}s", EditorStyles.helpBox);
        }
    }

    private void DrawBehaviorAttrs(AnimationEventTrigger behaviour)
    {
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.ObjectField("Script", behaviour, behaviour.GetType(), false);
        EditorGUI.EndDisabledGroup();        

        if (GUILayout.Button("Add Event"))
        {
            behaviour.events.Add(new AnimationEventInfo());
        }

        if (behaviour.events.Count <= 1)
        {
            if (GUILayout.Button("Remove Event") && behaviour.events.Count > 0)
            {
                behaviour.events.RemoveAt(behaviour.events.Count - 1);
            }
        }
        else
        {
            EditorGUILayout.BeginHorizontal();

            string[] eventItems = new string[behaviour.events.Count];
            float minWidth = 0;
            GUIStyle labelStyle = GUI.skin.label;
            for (int i = 0; i < eventItems.Count(); ++i)
            {
                eventItems[i] = new string($"Event{i}");
                Vector2 size = labelStyle.CalcSize(new GUIContent(eventItems[i]));
                if (size.x > minWidth) minWidth = size.x;
            }
            // add some padding
            minWidth += 50;

            if (m_RemoveSelection >= eventItems.Length)
                m_RemoveSelection = eventItems.Length - 1;

            m_RemoveSelection = EditorGUILayout.Popup(m_RemoveSelection, eventItems, GUILayout.Width(minWidth));
            if (GUILayout.Button($"Remove {eventItems[m_RemoveSelection]}", GUILayout.ExpandWidth(true)))
            {
                behaviour.events.RemoveAt(m_RemoveSelection);
            }           
            EditorGUILayout.EndHorizontal();
        }            

        if (behaviour.events.Count == 0)
            return;

        string[]items = new string[behaviour.events.Count];
        for (int i = 0; i < items.Count(); ++i)
            items[i] = new string($"Event{i}");
        m_PreviewSelection = EditorGUILayout.Popup("Preview Selection", m_PreviewSelection, items);

        EditorGUILayout.Space();
        for (int i = 0; i < behaviour.events.Count; ++i)
        { 
            var e = behaviour.events[i];            
            e.type = (AnimationEventType)EditorGUILayout.EnumPopup($"Event{i}", e.type);
            e.launchTime = EditorGUILayout.Slider("LaunchTime", e.launchTime, 0f, 1f);
            EditorGUILayout.Space();
        }

        EditorGUILayout.HelpBox("Please click Sort to make the events ordered by launch time", MessageType.Info);
        if (GUILayout.Button("Sort") && behaviour.events.Count > 1)
        {
            behaviour.events.Sort((a, b) => a.CompareTo(b));
        }

        if (GUILayout.Button("Clear All Events"))
        {
            behaviour.events.Clear();
        }
    }

    private bool Validate(AnimationEventTrigger behaviour, out string errorMessage)
    {
        AnimatorController controller = GetValidAnimatorController(out errorMessage);
        if (controller == null) return false;

        ChildAnimatorState matchingState = new ChildAnimatorState();
        foreach (AnimatorControllerLayer layer in controller.layers)
        {
            foreach (ChildAnimatorState state in layer.stateMachine.states)
            {
                if (state.state.behaviours.Contains(behaviour))
                {
                    matchingState = state;
                    break;
                }
            }

            FindMatchingStateInChildStateMachine(layer.stateMachine.stateMachines, behaviour, ref matchingState);
        }

        m_PreviewClip = matchingState.state?.motion as AnimationClip;
        if (m_PreviewClip == null)
        {
            errorMessage = "No valid AnimationClip found for the current state";
            return false;
        }
        return true;
    }

    private AnimatorController GetValidAnimatorController(out string errorMessage)
    {
        errorMessage = string.Empty;

        GameObject targetGo = Selection.activeGameObject;
        if (targetGo == null)
        {
            errorMessage = "Please select a GameObject with animator to preview";
            return null;
        }

        Animator animator = targetGo.GetComponent<Animator>();
        if (animator == null)
        {
            errorMessage = "The selected gameobject doesn't have an Animator component.";
            return null;
        }

        var animatorController = animator.runtimeAnimatorController as AnimatorController;
        if (animatorController == null)
        {
            errorMessage = "The selected Animator doesn't have a valid AnimatorController.";
            return null;
        }
        return animatorController;
    }

    private bool FindMatchingStateInChildStateMachine(ChildAnimatorStateMachine[] stateMachines, AnimationEventTrigger behaviour, ref ChildAnimatorState matchingTarget)
    {
        foreach (ChildAnimatorStateMachine machine in stateMachines)
        {
            if (machine.stateMachine.stateMachines.Length != 0 &&
                FindMatchingStateInChildStateMachine(machine.stateMachine.stateMachines, behaviour, ref matchingTarget))
            {
                return true;
            }

            foreach (ChildAnimatorState state in machine.stateMachine.states)
            {
                if (state.state.behaviours.Contains(behaviour))
                {
                    matchingTarget = state;
                    return true;
                }
            }
        }
        return false;
    }

    private void EnforceTPose()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null || !selected.TryGetComponent(out Animator animator) || animator.avatar == null)
            return;

        SkeletonBone[] skeletonBones = animator.avatar.humanDescription.skeleton;
        foreach (HumanBodyBones hbb in Enum.GetValues(typeof(HumanBodyBones)))
        {
            if (hbb == HumanBodyBones.LastBone) continue;

            Transform boneTransform = animator.GetBoneTransform(hbb);
            if (!boneTransform) continue;

            SkeletonBone skeletonBone = skeletonBones.FirstOrDefault(sb => sb.name == boneTransform.name);
            if (skeletonBone.name == null) continue;

            if (hbb == HumanBodyBones.Hips) boneTransform.localPosition = skeletonBone.position;
            boneTransform.localRotation = skeletonBone.rotation;
        }

        Debug.Log($"T-Pose enforced successfully on {selected.name}");
    }

    private void PreviewAnimationClip(AnimationEventTrigger behaviour)
    {
        if (m_PreviewClip == null) return;

        if (m_PreviewSelection < 0 || m_PreviewSelection >= behaviour.events.Count)
        {
            Debug.LogError($"err! invalid selection[{m_PreviewSelection}] for event list with [{behaviour.events.Count}] items");
            return;
        }

        m_PreviewTime = behaviour.events[m_PreviewSelection].launchTime;
        AnimationMode.StartAnimationMode();
        AnimationMode.SampleAnimationClip(Selection.activeGameObject, m_PreviewClip, m_PreviewTime * m_PreviewClip.length);
        AnimationMode.StopAnimationMode();
    }
}
