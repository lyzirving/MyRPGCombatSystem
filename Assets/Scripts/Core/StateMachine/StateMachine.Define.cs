using System;

public enum EFootStep : UInt16
{
    None = 0,
    LeftFoot,
    RightFoot
}

public interface IStateMachineOwner { }

public struct ChangeStateArgs
{
    /// <summary>
    /// Whether we should refresh state if current state doesn't change.
    /// </summary>
    public bool reCurrstate;
    /// <summary>
    /// Mode of foot step
    /// </summary>
    public EFootStep footStep;

    public class Builder
    {
        private bool m_ReCurrstate = false;
        private EFootStep m_FootStep = EFootStep.None;

        public Builder Refresh(bool refresh)
        {
            m_ReCurrstate = refresh;
            return this;
        }

        public Builder Footstep(EFootStep footStep)
        {
            m_FootStep = footStep;
            return this;
        }

        public ChangeStateArgs Build()
        {
            ChangeStateArgs args = new ChangeStateArgs();
            args.reCurrstate = m_ReCurrstate;
            args.footStep = m_FootStep;
            return args;
        }
    }
}
