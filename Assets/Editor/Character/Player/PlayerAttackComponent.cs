using UnityEditor;

[CustomEditor(typeof(PlayerAttackComponent))]
public class PlayerAttackComponentEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        PlayerAttackComponent script = (PlayerAttackComponent)target;
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Choose AttackHotspot");
        EditorGUILayout.IntField("Current Index: ", script.index);
        AttackHotspot[] hotspots = script.hotspots;
        if (hotspots != null && hotspots.Length > 0)
        {
            for (int i = 0; i < script.hotspots.Length; i++)
            {
                bool isSelected = i == script.index;
                bool newSelected = EditorGUILayout.ToggleLeft(script.hotspots[i].name, isSelected);

                if (newSelected && !isSelected)
                {
                    script.index = i;
                }
            }
        }        
    }
}
