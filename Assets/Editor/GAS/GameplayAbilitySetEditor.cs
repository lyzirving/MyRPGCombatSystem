using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(GameplayAbilitySet))]
public class GameplayAbilitySetEditor : Editor
{
    private List<GameplayAbility> m_Abilities = new List<GameplayAbility>();

    private void OnEnable()
    {
        var abilitySet = (GameplayAbilitySet)target;
        m_Abilities.Clear();
        foreach (var item in abilitySet.abilities.Values)
        {
            m_Abilities.Add(item);
        }
    }

    public override void OnInspectorGUI()
    {
        
    }
}
