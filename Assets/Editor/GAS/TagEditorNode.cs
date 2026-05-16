using System.Collections.Generic;
using System.Text;
using UnityEngine;

internal class TagEditorNode
{
    public string fullName;
    public string shortName;
    public TagEditorNode parent = null;
    public List<TagEditorNode> children = new List<TagEditorNode>();
    public bool expand = false;

    // if there isn't '.' in text, Split('.') return an array of length 1.
    // root node's depth is always 1.
    public int depth => string.IsNullOrEmpty(fullName) ? 0 : fullName.Split('.').Length;
    public bool isRoot => parent == null;
    public bool isValid => depth != 0;

    #region Static Methods
    public static TagEditorNode BuildEditorTreeFromDatabase(GameplayTagDatabase database)
    {
        return BuildEditorTreeFromTagList(database.allTags);
    }

    public static TagEditorNode BuildEditorTreeFromTagList(List<GameplayTag> tags)
    {
        if (tags.Count > 0)
        {
            var rootTag = tags[0];
            var root = new TagEditorNode();
            BuildEditorTree(root, null, ref rootTag);
            return root;
        }
        else
        {
            Debug.Log($"TagEditorNode: tag count[{tags.Count}] is invalid");
            return null;
        }
    }

    public static TagEditorNode BuildEditorTreeFromTagList(IReadOnlyList<GameplayTag> tags)
    {
        if (tags.Count > 0)
        {
            var rootTag = tags[0];
            var root = new TagEditorNode();
            BuildEditorTree(root, null, ref rootTag);
            return root;
        }
        else
        {
            Debug.Log($"TagEditorNode: tag count[{tags.Count}] is invalid");
            return null;
        }
    }

    private static void BuildEditorTree(TagEditorNode node, TagEditorNode parent, ref GameplayTag tag)
    {
        node.parent = parent;
        node.fullName = tag.name;
        node.MakeShortName(tag.name);

        var childTagList = GameplayTagManager.instance.RequestDirectChildren(tag);
        if (childTagList == null || childTagList.Length <= 0)
            return;

        for (int i = 0; i < childTagList.Length; ++i)
        {
            var childTag = childTagList[i];
            var childNode = new TagEditorNode();

            BuildEditorTree(childNode, node, ref childTag);

            node.children.Add(childNode);
        }
    }
    #endregion

    #region Filed Methods
    public void ApplyShortNameChange()
    {
        if (isRoot)
            return;

        int lastDot = fullName.LastIndexOf('.');
        fullName = new StringBuilder(fullName.Substring(0, lastDot + 1)).Append(shortName).ToString();
    }

    public void MakeShortName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return;

        int lastDot = name.LastIndexOf('.');
        if (lastDot == -1)
            shortName = name;
        else if (lastDot <= name.Length - 2)
            shortName = name.Substring(lastDot + 1);
    }

    public void DeleteChild(TagEditorNode child)
    {
        for (int i = 0; i < children.Count; ++i)
        {
            if (children[i] == child)
            {
                children.RemoveAt(i);
                break;
            }
        }

        if (children == null || children.Count == 0)
            expand = false;
    }
    #endregion
}
