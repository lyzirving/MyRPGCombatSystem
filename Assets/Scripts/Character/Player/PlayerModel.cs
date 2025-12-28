using UnityEngine;

public class PlayerModel : MonoBehaviour
{
    private Animator m_Animator;

    private void Awake()
    {
        m_Animator = GetComponent<Animator>();
        if (m_Animator == null)
            throw new System.Exception("Err, Animator hasn't been assigned");
    }

    public void StartAnimation(int hash)
    {
        m_Animator?.SetBool(hash, true);
    }

    public void StopAnimation(int hash)
    {
        m_Animator?.SetBool(hash, false);
    }
}
