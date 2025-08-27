using Codice.Client.BaseCommands.Merge.Xml;

using System;
using System.Linq;

using Unity.VisualScripting;

using UnityEditor;

using UnityEngine;

[CustomEditor(typeof(Part))]
public class PartEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var part = target as Part;
        //if (GUILayout.Button("Add Module"))
        //{
        //    var dropdown = SearchableDropdown<Module>.Create<Module>((Type resource) =>
        //    {
        //        part.part.modules.Add(resource.GetConstructor(Type.EmptyTypes).Invoke(Array.Empty<object>()) as Module);
        //    });

        //    dropdown.Show(new Rect(Event.current.mousePosition, new Vector2(300, 0)));
        //}

        part.modules = part.GetComponents<Module>().ToList();
    }
}
[CustomEditor(typeof(Vehicle))]
public class VehicleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var part = target as Vehicle;
        //if (GUILayout.Button("Add Module"))
        //{
        //    var dropdown = SearchableDropdown<Module>.Create<Module>((Type resource) =>
        //    {
        //        part.part.modules.Add(resource.GetConstructor(Type.EmptyTypes).Invoke(Array.Empty<object>()) as Module);
        //    });

        //    dropdown.Show(new Rect(Event.current.mousePosition, new Vector2(300, 0)));
        //}

        part.parts = part.GetComponentsInChildren<Part>().ToList();
    }
}

[CustomEditor(typeof(ResourceContainer))]
public class ContainerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var part = target as ResourceContainer;
        if (GUILayout.Button("Add Resource"))
        {
            var dropdown = SearchableDropdown<FluidType>.Create<FluidType>(resource =>
            {
                part.Expand(new Resource(resource));
            });

            dropdown.Show(new Rect(Event.current.mousePosition, new Vector2(300, 0)));
        }
    }
}

[CustomPropertyDrawer(typeof(SerializedIModule), true)]
public class ModulePropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(SerializedIModule.module)), label);
    }
}

[CustomPropertyDrawer(typeof(Resource))]
public class ResourceDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        position.xMax -= 50;
        EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(Resource.type)), label);

        var xMax = position.xMax;
        position.xMin = xMax + 5;
        position.xMax = xMax + 50;
        EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(Resource.mass)), GUIContent.none);
    }
}