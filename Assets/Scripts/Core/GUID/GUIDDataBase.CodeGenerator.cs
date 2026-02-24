using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public partial class GUIDDataBase
{
#if UNITY_EDITOR
    public void GenerateCodeFile(string filePath = "Assets/Scripts/Core/GUIID/GUIDConsts.cs")
    {
        try
        {
            string code = GenerateCode();
            System.IO.File.WriteAllText(filePath, code);
            AssetDatabase.Refresh();
            Debug.Log($"succeed to generate GUIDConsts.cs: {filePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"fail to generate GUIDConsts.cs: {e.Message}");
        }
    }

    // 生成 C# 代码
    private string GenerateCode()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        sb.AppendLine("// ===========================================");
        sb.AppendLine("// Auto Generated GUID Constant Class");
        sb.AppendLine($"// Generated Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine("// Pelease do not manually modify this file.");
        sb.AppendLine("// ===========================================");
        sb.AppendLine();
        sb.AppendLine("using System;");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// Global GUID constant");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("public static class GUIDConsts");
        sb.AppendLine("{");

        Dictionary<string, List<GUIDEntry>> groupedEntries = new Dictionary<string, List<GUIDEntry>>();
        foreach (var entry in m_GuidEntries)
        {
            if (!groupedEntries.ContainsKey(entry.category))
                groupedEntries[entry.category] = new List<GUIDEntry>();
            groupedEntries[entry.category].Add(entry);
        }

        foreach (var category in groupedEntries.Keys)
        {
            sb.AppendLine($"// ===== {category} =====");

            foreach (var entry in groupedEntries[category])
            {
                string fieldName = MakeValidIdentifier(entry.name);
                if (string.IsNullOrEmpty(fieldName))
                    fieldName = $"GUID_{entry.guid.ToString().Substring(0, 20)}";

                sb.AppendLine($"    /// <summary>");
                sb.AppendLine($"    /// {entry.description}");
                sb.AppendLine($"    /// Created Time: {entry.createdTime}");
                sb.AppendLine($"    /// </summary>");
                sb.AppendLine($"    public const int {fieldName} = {entry.guid};");
                sb.AppendLine();
            }
        }

        sb.AppendLine("}");// class ends

        return sb.ToString();
    }

    private string MakeValidIdentifier(string input)
    {
        if (string.IsNullOrEmpty(input)) return "Unnamed";

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        bool lastWasSeparator = true;

        foreach (char c in input)
        {
            if (char.IsLetter(c))
            {
                sb.Append(lastWasSeparator ? char.ToUpper(c) : c);
                lastWasSeparator = false;
            }
            else if (char.IsDigit(c) && sb.Length > 0)
            {
                sb.Append(c);
                lastWasSeparator = false;
            }
            else if (c == '_')
            {
                sb.Append('_');
                lastWasSeparator = false;
            }
            else
            {
                lastWasSeparator = true;
            }
        }

        string result = sb.ToString();
        if (result.Length == 0) result = "Unnamed";
        if (char.IsDigit(result[0])) result = "_" + result;

        return result;
    }
#endif
}
