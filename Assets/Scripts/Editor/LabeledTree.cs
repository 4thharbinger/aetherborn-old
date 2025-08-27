

using System.Collections.Generic;
using System;
using System.Linq;
using System.Collections;

public class StringTree<TData> : LabeledTree<string, TData>
{
    /// <summary>
    ///     Follows a path separated by slashes through the tree.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public TData GetPath(string path)
    {
        var parts = path.Split('/');
        var current = this;
        foreach (var part in parts)
        {
            current = current.children.FirstOrDefault(c => c.label == part) as StringTree<TData>;
            if (current == null)
            {
                return default;
            }
        }
        return current.data;
    }

    /// <summary>
    ///     Follows a path separated by slashes through the tree and removes the node at the label.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public TData RemovePath(string path)
    {
        var parts = path.Split('/');
        var current = this;
        foreach (var part in parts)
        {
            current = current.children.FirstOrDefault(c => c.label == part) as StringTree<TData>;
            if (current == null)
            {
                return default;
            }
        }
        current.Remove(current);
        return current.data;
    }

    /// <summary>
    ///     Follows a path separated by slashes through the tree and sets the data to the new value.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public LabeledTree<string, TData> SetPath(string path, TData data)
    {
        var parts = path.Split('/');
        var current = this as LabeledTree<string, TData>;
        foreach (var part in parts)
        {
            var node = current.children.FirstOrDefault(c => c.label == part);
            node ??= current.AddBranch(part);
            current = node;
        }
        current.data = data;
        return current;
    }
}

/// <summary>
///     A tree with a label and data.
/// </summary>
/// <typeparam name="TLabel"></typeparam>
/// <typeparam name="TData"></typeparam>
[Serializable]
public class LabeledTree<TLabel, TData> : IEnumerable<TData>
{
    public TLabel label;
    public TData data;
    public LabeledTree<TLabel, TData> parent;
    public List<LabeledTree<TLabel, TData>> children = new();

    /// <summary>
    ///     Add a child node with the given label and data.
    /// </summary>
    /// <param name="label"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public virtual LabeledTree<TLabel, TData> Add(TLabel label, TData data)
    {
        var node = new LabeledTree<TLabel, TData> { label = label, data = data, parent = this };
        children.Add(node);
        return node;
    }

    /// <summary>
    ///     Adds unlabeled child nodes from the enumerable.
    /// </summary>
    /// <param name="data"></param>
    //public virtual void AddRange(IEnumerable<TData> data)
    //{
    //    children.AddRange(data.Select(d => new LabeledTree<TLabel, TData> { data = d, parent = this }));
    //}

    /// <summary>
    ///     Adds an empty branch with the label.
    /// </summary>
    /// <param name="label"></param>
    /// <returns></returns>
    public virtual LabeledTree<TLabel, TData> AddBranch(TLabel label)
    {
        var branch = new LabeledTree<TLabel, TData> { label = label, parent = this };
        children.Add(branch);
        return branch;
    }

    /// <summary>
    ///     Adds a branch with the label and data.
    /// </summary>
    /// <param name="label"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    //public virtual LabeledTree<TLabel, TData> AddBranch(TLabel label, IEnumerable<TData> data)
    //{
    //    var branch = new LabeledTree<TLabel, TData> { label = label, parent = this };
    //    branch.AddRange(data);
    //    children.Add(branch);
    //    return branch;
    //}

    public IEnumerator<TData> GetEnumerator()
    {
        foreach (var child in children)
        {
            if (child is LabeledTree<TLabel, TData> tree)
            {
                foreach (var node in tree)
                {
                    yield return node;
                }
            }
            yield return child.data;
        }
    }

    /// <summary>
    ///     Removes a node from the tree.
    /// </summary>
    /// <param name="node"></param>
    public void Remove(LabeledTree<TLabel, TData> node)
    {
        children.Remove(node);
        if (node.parent == this) node.parent = null;
    }

    /// <summary>
    ///     Removes this node from its parent tree if present.
    /// </summary>
    public void RemoveSelf()
    {
        if (parent == null)
        {
            return;
        }
        parent.Remove(this);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}