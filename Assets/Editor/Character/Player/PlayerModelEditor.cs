using UnityEditor;

[CustomEditor(typeof(PlayerModel))]
public class PlayerModelEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        PlayerModel script = (PlayerModel)target;
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Choose a Weapon");
        EditorGUILayout.IntField("Armed Weapon Index: ", script.armedWeaponIndex);
        WeaponController[] weapons = script.weapons;
        if (weapons != null && weapons.Length > 0)
        {
            for (int i = 0; i < script.weapons.Length; i++)
            {
                bool isSelected = i == script.armedWeaponIndex;
                bool newSelected = EditorGUILayout.ToggleLeft(script.weapons[i].name, isSelected);

                if (newSelected && !isSelected)
                {
                    script.armedWeaponIndex = i;
                }
            }
        }        
    }
}
