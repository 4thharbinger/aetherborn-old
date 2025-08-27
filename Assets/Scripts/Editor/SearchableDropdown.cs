using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Unity.VisualScripting;

using UnityEditor.IMGUI.Controls;

using UnityEngine;

public class SearchableDropdown<T> : AdvancedDropdown
{
    private readonly StringTree<T> options;
    private readonly Action<T> onSelected;
    private readonly Dictionary<int, T> ids = new();

    public SearchableDropdown(AdvancedDropdownState state, StringTree<T> options, Action<T> onSelected)
        : base(state)
    {
        this.options = options;
        this.onSelected = onSelected;
        minimumSize = new Vector2(300, 200);
    }

    /// <summary>
    ///     Creates a dropdown for the given type of Unity Objects.
    /// </summary>
    /// <typeparam name="TUnity"></typeparam>
    /// <param name="onSelected"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static SearchableDropdown<TUnity> Create<TUnity>(Action<TUnity> onSelected, string name = null)
        where TUnity : UnityEngine.Object
    {
        var obj = Resources.LoadAll<TUnity>("/");
        var tree = new StringTree<TUnity> { label = name ?? typeof(TUnity).Name };

        foreach (var o in obj)
        {
            if (o is ISearchable searchable && searchable.label != "")
            {
                tree.SetPath(o.GetType().Name + "/" + searchable.label + "/" + o.name, o);
            }
            else
            {
                tree.SetPath(o.GetType().Name + "/" + o.name, o);
            }
        }

        return new SearchableDropdown<TUnity>(new AdvancedDropdownState(), tree, onSelected);
    }

    private static readonly Dictionary<Type, Type[]> types = new(); // <Type, Types that inherit from it>

    /// <summary>
    ///     Creates a dropdown for the given type and all types that inherit from it.
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    /// <param name="onSelected"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static SearchableDropdown<Type> Create<TType>(Action<Type> onSelected, string name = null)
    {
        if (!SearchableDropdown<Type>.types.TryGetValue(typeof(TType), out var types))
        {
            types = Assembly.GetAssembly(typeof(TType)).ExportedTypes.Where(type => typeof(TType).IsAssignableFrom(type)).ToArray();
            SearchableDropdown<Type>.types.Add(typeof(TType), types);
        }
        var tree = new StringTree<Type> { label = name ?? typeof(TType).Name };

        foreach (var type in types)
        {
            if (type is ISearchable searchable && searchable.label != "")
            {
                tree.SetPath(searchable.label + "/" + type.Name, type);
            }
            else
            {
                tree.SetPath(type.Name, type);
            }
        }

        return new SearchableDropdown<Type>(new AdvancedDropdownState(), tree, onSelected);
    }

    protected override AdvancedDropdownItem BuildRoot()
    {
        var root = new AdvancedDropdownItem(options.label);

        var id = 0;

        Append(root, options, ref id);

        return root;
    }

    private void Append(AdvancedDropdownItem item, LabeledTree<string, T> objects, ref int id)
    {
        foreach (var obj in objects.children)
        {
            if (obj.children.Count > 0)
            {
                var i = new AdvancedDropdownItem(obj.label) { id = obj.GetHashCode() + id };
                Append(i, obj, ref id);
                item.AddChild(i);
            }
            else
            {
                item.AddChild(new AdvancedDropdownItem(obj.data is UnityEngine.Object uobj ? uobj.name : obj.data.ToString()) { id = obj.GetHashCode() + id });
                ids.Add(obj.GetHashCode() + id, obj.data);
                // If this errors, you just found a hash collision!
            }
            id++;
        }
    }

    protected override void ItemSelected(AdvancedDropdownItem item)
    {
        onSelected?.Invoke(ids[item.id]);
    }
}
public interface ISearchable
{
    public string label { get; }
}
