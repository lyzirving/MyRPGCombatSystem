using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// Global GameplayTag manager, which is responsible for tag registration, node building, and parent-child relationship building
/// </summary>
public class GameplayTagManager : Singleton<GameplayTagManager>
{
    public const string path = "Settings/GameplayTag/Database";

    private List<GameplayTag2> m_AllTags= new List<GameplayTag2>();

    // All tags' node relationship
    private Dictionary<GameplayTag2, TagNode> m_TagNodeMap = new Dictionary<GameplayTag2, TagNode>(GameplayTag2.EqualityComparer);
    // Dictionary for quick search
    private Dictionary<string, GameplayTag2> m_TagsByName = new Dictionary<string, GameplayTag2>();

    public override void OnInit()
    {
        var handle = Addressables.LoadAssetAsync<GameplayTagDatabase>(path);
        var database = handle.WaitForCompletion();
        if (database == null)
            throw new System.Exception($"Fail to find GameplayTagDatabase at: {path}");

        InsertTagsIntoTree(database.allTags);

        handle.Release();
    }

    public void Clear()
    {
        m_TagNodeMap.Clear();
        m_TagsByName.Clear();
        m_AllTags.Clear();
    }

    public void InsertTagsIntoTree(List<GameplayTag2> gameplayTags)
    {
        if (gameplayTags == null || gameplayTags.Count == 0)
            return;

        // Build tag tree
        foreach (var tag in gameplayTags)
        {
            if (!tag.isValid)
                continue;

            DoInsertTag(tag);
        }

        CalculateNodeParents();
    }

    public void InsertTagIntoTree(GameplayTag2 gameplayTag)
    {
        if (!gameplayTag.isValid)
            return;

        DoInsertTag(gameplayTag);
        CalculateNodeParents();
    }

    public bool HasTag(GameplayTag2 gameplayTag)
    {
        if (!gameplayTag.isValid)
            throw new System.Exception("input tag is invalid");
        return m_TagsByName.ContainsKey(gameplayTag.name);
    }

    public GameplayTag2[] GetParentTags(GameplayTag2 gameplayTag)
    {
        if (m_TagNodeMap.TryGetValue(gameplayTag, out var node))
        {
            return node.parentTags.ToArray();
        }
        return new GameplayTag2[0];
    }

    public GameplayTag2 RequestDirectParent(GameplayTag2 gameplayTag)
    {
        if (m_TagNodeMap.TryGetValue(gameplayTag, out var node) && node.parent != null)
            return node.parent.tag;
        return GameplayTag2.RootTag;
    }

    public GameplayTag2[] RequestAllChildren(GameplayTag2 gameplayTag)
    {
        if (m_TagNodeMap.TryGetValue(gameplayTag, out var node))
        {
            var children = new List<GameplayTag2>();
            CollectChildren(node, children);
            return children.ToArray();
        }
        return new GameplayTag2[0];
    }

    public GameplayTag2[] RequestDirectChildren(GameplayTag2 gameplayTag)
    {
        if (m_TagNodeMap.TryGetValue(gameplayTag, out var node))
        {
            var children = new List<GameplayTag2>();
            foreach (var child in node.children)
            {
                children.Add(child.tag);
            }
            return children.ToArray();
        }
        return new GameplayTag2[0];
    }

    private void DoInsertTag(GameplayTag2 gameplayTag)
    {
        if (HasTag(gameplayTag))
            return;

        var node = DoInsertTagNode(gameplayTag);
        m_TagNodeMap[gameplayTag] = node;
        m_TagsByName[gameplayTag.name] = gameplayTag;
        m_AllTags.Add(gameplayTag);
    }

    private TagNode DoInsertTagNode(GameplayTag2 tag)
    {
        string[] parts = tag.name.Split('.');
        TagNode currentNode = null;
        string currentPath = "";

        for (int i = 0; i < parts.Length; i++)
        {
            currentPath = (i == 0) ? parts[i] : currentPath + "." + parts[i];
            var partTag = new GameplayTag2(currentPath);

            if (!m_TagNodeMap.TryGetValue(partTag, out var existingNode))
            {
                existingNode = new TagNode(partTag);
                m_TagNodeMap[partTag] = existingNode;
            }

            if (currentNode != null)
            {
                existingNode.parent = currentNode;
                currentNode.AddChild(existingNode);
            }
            currentNode = existingNode;
        }

        return currentNode;
    }

    private void CalculateNodeParents()
    {
        foreach (var kvp in m_TagNodeMap)
        {
            kvp.Value.CacheParentTags();
        }
    }

    private void CollectChildren(TagNode node, List<GameplayTag2> list)
    {
        foreach (var child in node.children)
        {
            list.Add(child.tag);
            CollectChildren(child, list);
        }
    }

    public class TagNode
    {
        public GameplayTag2 tag { get; }
        public TagNode parent { get; set; }
        public List<TagNode> children { get; } = new List<TagNode>();

        /// <summary>
        /// parentTags[parentTags.Count - 1] is the root parent
        /// </summary>
        public List<GameplayTag2> parentTags { get; private set; } = new List<GameplayTag2>();

        public TagNode(GameplayTag2 gameplayTag)
        {
            tag = gameplayTag;
        }

        public void AddChild(TagNode node)
        {
            var result = children.Find(i => i.tag == node.tag);
            if (result == null)
                children.Add(node);
        }

        public void CacheParentTags()
        {
            parentTags.Clear();
            TagNode current = parent;
            while (current != null)
            {
                parentTags.Add(current.tag);
                current = current.parent;
            }
        }
    }
}
