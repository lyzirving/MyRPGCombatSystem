using System;

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

    public struct Builder
    {
        private bool m_ReCurrstate;
        private EFootStep m_FootStep;

        public Builder(bool recreate = false)
        {
            m_ReCurrstate = recreate;
            m_FootStep = EFootStep.None;
        }

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
