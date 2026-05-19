using System.Collections.Generic;
using System.Text;

internal class TagEditorNode
{
    private static TagEditorNode k_RootNode = null;

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
    public static TagEditorNode GetRootNode()
    {
        if (k_RootNode == null)
        {
            k_RootNode = new TagEditorNode();
            k_RootNode.fullName = k_RootNode.shortName = GameplayTag.RootName;
        }
        return k_RootNode;
    }

    public static void BuildEditorTree(TagEditorNode root)
    {
        if(root == null)
            return;

        BuildEditorTree(0, root);
    }

    public static void BuildEditorTree(int index, TagEditorNode node)
    {
        var indices = GameplayTagManager.instance.GetChildIndices(index);
        if (indices == null || indices.Length == 0)
            return;

        for (int i = 0; i < indices.Length; ++i)
        {
            TagEditorNode current = new TagEditorNode();
            current.parent = node;
            current.fullName = GameplayTagManager.instance.GetName(indices[i]);
            current.MakeShortName(current.fullName);
            node.children.Add(current);

            BuildEditorTree(indices[i], current);
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
