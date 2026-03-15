
using UnityEngine;

public struct ChangeStateArgs
{
    /// <summary>
    /// Whether we should refresh state if current state doesn't change.
    /// </summary>
    public bool reEnterState;
    /// <summary>
    /// Current footStep of animation
    /// </summary>
    public EFootstep footStep;
    /// <summary>
    /// Hit position in one attack
    /// </summary>
    public Vector3 hitPos;
    /// <summary>
    /// source ICharacterBehavior who triggers the attack
    /// </summary>
    public ICharacterBehavior source;
    /// <summary>
    /// Skill data of one attack
    /// </summary>
    public readonly SkillData skillData;

    public ChangeStateArgs(bool reEnterState)
    { 
        this.reEnterState = reEnterState;
        this.footStep = EFootstep.None;
        this.hitPos = Vector3.zero;
        this.source = null;
        this.skillData = null;
    }

    public ChangeStateArgs(EFootstep footStep)
    {
        this.reEnterState = false;
        this.footStep = footStep;
        this.hitPos = Vector3.zero;
        this.source = null;
        this.skillData = null;
    }

    public ChangeStateArgs(bool reEnterState, in ICharacterBehavior source, in SkillData skillData, Vector3 hitPos)
    {
        this.reEnterState = reEnterState;
        this.footStep = EFootstep.None;
        this.hitPos = hitPos;
        this.source = source;
        this.skillData = skillData;
    }
}

public interface IStateMachineOwner 
{
    public void ChangeState(ECharacterState state, ChangeStateArgs args = default(ChangeStateArgs));
    public void ExitCurrentState();
}
