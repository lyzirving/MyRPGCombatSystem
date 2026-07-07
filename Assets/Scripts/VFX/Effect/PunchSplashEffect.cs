using DG.Tweening;
using UnityEngine;

/// <summary>
/// Implementation of Arts/VFX/Prefabs/VFX_Punch.prefab
/// </summary>
public class PunchSplashEffect : VFXEffect
{
    private float m_EnterRatio = 0f;
    private float m_FadeOutRatio = 0f;

    private Tweener m_EnterTween;
    private Tweener m_FadeOutTween;

    private Material m_Material;

    private void Start()
    {
        m_Material = GetComponent<MeshRenderer>().material;
    }

    public override void StartPlay()
    {
        this.gameObject.SetActive(true);

        m_EnterRatio = 0f;        
        m_EnterTween?.Kill();
        m_FadeOutTween?.Kill();

        SetClipPos(0f);
        SetMaskStrength(0f);

        m_EnterTween = DOTween.To(() => { return m_EnterRatio; }, (val) => { m_EnterRatio = val; }, 1f, duration)
            .SetEase(Ease.InOutSine)
            .OnUpdate(OnEnterProgress)
            .OnComplete(OnEnterComplete);
        m_EnterTween.Play();
    }

    public override void StopPlay()
    {        
    }

    private void OnEnterProgress()
    {
        SetClipPos(m_EnterRatio);
        SetMaskStrength(m_EnterRatio);
    }

    private void OnEnterComplete()
    {
        m_FadeOutRatio = 0f;
        m_FadeOutTween?.Kill();
        m_FadeOutTween = DOTween.To(() => { return m_FadeOutRatio; }, (val) => { m_FadeOutRatio = val; }, 1f, duration * 0.5f)
            .SetEase(Ease.OutExpo)
            .OnUpdate(OnFadeOutProgress)
            .OnComplete(OnFadeOutComplete);
        m_FadeOutTween.Play();
    }

    private void OnFadeOutProgress()
    {
        SetMaskStrength(1f - m_FadeOutRatio);
    }

    private void OnFadeOutComplete()
    {
        this.gameObject.SetActive(false);
    }

    private void SetClipPos(float value)
    {
        m_Material?.SetFloat("_ClipPos", Mathf.Clamp01(value));
    }

    private void SetMaskStrength(float value)
    {
        m_Material?.SetFloat("_MaskStrength", Mathf.Clamp01(value));
    }
}
