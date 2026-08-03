using System;

public class CombatDefine
{
    public enum EAttack : UInt16
    { 
        None = 0,
        LA,      //Light Attack
        HA,      //Heavy Attack
        SprintLA, //Sprint Light Attack
        SprintHA, //Sprint Heavy Attack
    }
}
