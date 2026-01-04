using System;
using System.Linq;
using AnimationDefine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

[CustomEditor(typeof(DoubleAnimationEventTriggerBehaviour))]
public class DoubleEventTriggerBehaviourEditor : Editor
{
    public enum PreviewStatus : uint
    {
        StopPreview = 0,
        PreviewEvent_0,
        PreviewEvent_1
    }
    
    private AnimationClip m_PreviewClip;    
    private PreviewStatus m_Preview = PreviewStatus.StopPreview;
    private float m_PreviewTime = 0f;

    public override void OnInspectorGUI()
    {
        // Use custom DrawBehaviorAttrs instead of DrawDefaultInspector
        //DrawDefaultInspector();        

        DoubleAnimationEventTriggerBehaviour behaviour = (DoubleAnimationEventTriggerBehaviour)target;

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
        PreviewStatus preview = (PreviewStatus)EditorGUILayout.EnumPopup("Preview Status", m_Preview);

        // Selection change
        if (m_Preview != preview)
        {
            m_Preview = preview;
            if (m_Preview == PreviewStatus.StopPreview)
                EnforceTPose();
        }

        if (m_Preview != PreviewStatus.StopPreview)
        {
            PreviewAnimationClip(behaviour);
            GUILayout.Label($"Previewing at {m_PreviewTime}s", EditorStyles.helpBox);
        }
    }

    private void DrawBehaviorAttrs(DoubleAnimationEventTriggerBehaviour behaviour)
    {
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.ObjectField("Script", behaviour, behaviour.GetType(), false);
        EditorGUI.EndDisabledGroup();

        behaviour.event0 = (PlayerAnimationEvent)EditorGUILayout.EnumPopup("Event0", behaviour.event0);
        behaviour.triggerTime0 = EditorGUILayout.Slider("TriggerTime0", behaviour.triggerTime0, 0f, 1f);

        behaviour.event1 = (PlayerAnimationEvent)EditorGUILayout.EnumPopup("Event1", behaviour.event1);
        behaviour.triggerTime1 = EditorGUILayout.Slider("TriggerTime1", behaviour.triggerTime1, 0f, 1f);
    }

    private void PreviewAnimationClip(DoubleAnimationEventTriggerBehaviour behaviour)
    {
        if (m_PreviewClip == null) return;

        m_PreviewTime = (m_Preview == PreviewStatus.PreviewEvent_0) ? behaviour.triggerTime0 : behaviour.triggerTime1;

        AnimationMode.StartAnimationMode();
        AnimationMode.SampleAnimationClip(Selection.activeGameObject, m_PreviewClip, m_PreviewTime);
        AnimationMode.StopAnimationMode();
    }

    private void EnforceTPose()
    {
        GameObject selected = Selection.activeGameObject;
        if(selected == null || !selected.TryGetComponent(out Animator animator) || animator.avatar == null)
            return;

        SkeletonBone[] skeletonBones = animator.avatar.humanDescription.skeleton;
        foreach (HumanBodyBones hbb in Enum.GetValues(typeof(HumanBodyBones)))
        { 
            if(hbb == HumanBodyBones.LastBone) continue;

            Transform boneTransform = animator.GetBoneTransform(hbb);
            if(!boneTransform) continue;

            SkeletonBone skeletonBone = skeletonBones.FirstOrDefault(sb => sb.name == boneTransform.name);
            if (skeletonBone.name == null) continue;

            if(hbb == HumanBodyBones.Hips) boneTransform.localPosition = skeletonBone.position;
            boneTransform.localRotation = skeletonBone.rotation;
        }

        Debug.Log($"T-Pose enforced successfully on {selected.name}");
    }    

    bool Validate(DoubleAnimationEventTriggerBehaviour behaviour, out string errorMessage)
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

    private bool FindMatchingStateInChildStateMachine(ChildAnimatorStateMachine[] stateMachines, DoubleAnimationEventTriggerBehaviour behaviour, ref ChildAnimatorState matchingTarget)
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
}
