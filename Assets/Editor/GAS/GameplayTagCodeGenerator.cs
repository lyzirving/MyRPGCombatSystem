using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class GameplayTagCodeGenerator
{
    public static void GenerateCodeFile(string filePath = "Assets/Scripts/GAS/Tag/GameplayTag.Define.cs")
    {
        try
        {
            string code = GenerateCode();
            System.IO.File.WriteAllText(filePath, code);
            AssetDatabase.Refresh();
            Debug.Log($"succeed to generate GameplayTag.Define.cs: {filePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"fail to generate GUIDConsts.cs: {e.Message}");
        }
    }

    private static string GenerateCode()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        sb.AppendLine("// ===========================================");
        sb.AppendLine("// Auto Generated GameplayTag Definition");
        sb.AppendLine($"// Generated Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine("// Pelease do not manually modify this file.");
        sb.AppendLine("// ===========================================");
        sb.AppendLine();
        sb.AppendLine("using System;");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// GameplayTag definition");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("public partial struct GameplayTag");
        sb.AppendLine("{");

        HashSet<string> existingTags = new HashSet<string>();
        var tags = GameplayTagManager.instance.tags;
        foreach (var tag in tags)
        {
            if (string.IsNullOrEmpty(tag.simpleName))
                continue;

            string simpleName = string.Copy(tag.simpleName);
            string filed = MakeTagFiled(simpleName);
            if (existingTags.Contains(filed))
                throw new Exception($"filed[{filed}] already existed for tag[{tag.name}]");
            else
                existingTags.Add(filed);

            sb.AppendLine($"    /// <summary>");
            sb.AppendLine($"    /// Name:  {tag.name}");
            sb.AppendLine($"    /// Index: {tag.index}");
            sb.AppendLine($"    /// </summary>");
            sb.AppendLine($"    public static readonly string {filed} = \"{tag.name}\";");
            sb.AppendLine();
        }

        sb.AppendLine("}");// class ends

        return sb.ToString();
    }

    private static string MakeTagFiled(string name)
    {
        if(string.IsNullOrEmpty(name))
            return string.Empty;

        name = name.Replace(".", "_");
        return name.ToUpperInvariant();
    }
}
