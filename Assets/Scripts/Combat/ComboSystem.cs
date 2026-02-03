using UnityEngine;

public class ComboSystem
{
    public ComboSequence current;
    public int skill;
    public float startTime;

    public bool isComboStart { get => startTime > 0f; }

    public void BeginCombo(ComboSequence combo, int skillIndex)
    {
        current = combo;
        skill = skillIndex;
        startTime = Time.time;
    }

    public void EndCombo()
    {
        current = null;
        skill = -1;
        startTime = -1f;
    }

    public bool UpdateCombo()
    {
        if (!isComboStart)
            return false;

        return false;
    }
}
