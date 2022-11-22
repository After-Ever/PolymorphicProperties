//MIT License
//
//Copyright (c) 2022 Ben Trotter
//
//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor;

namespace AfterEver.Utilities.PolymorphicProperties
{
    public interface IPolymorphicPropertyEditor
    {
        float GetPropertyHeight(SerializedProperty property);
        void DisplayPropertyGui(Rect rect, SerializedProperty property);
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class PolymorphicPropertyEditorAttribute : Attribute
    {
        public readonly Type rootPropertyType;
        public readonly string label;
        public readonly string docUrl;

        public PolymorphicPropertyEditorAttribute(
            Type rootPropertyType,
            string label,
            string docUrl = null)
        {
            this.rootPropertyType = rootPropertyType
                ?? throw new ArgumentNullException();
            this.label = label;
            this.docUrl = docUrl;
        }
    }

    public static class PolymorphicPropertyManager
    {
        class EditorInfo
        {
            public IPolymorphicPropertyEditor editor;
            public string docUrl;
        }

        static Dictionary<Type, Dictionary<string, EditorInfo>> editors;

        public static float GenericPickerHeight(
            Type t,
            SerializedProperty property,
            string selectedEditor)
        {
            Init();

            if (!editors.TryGetValue(t, out var typeEditors))
            {
                return EditorGUIUtility.singleLineHeight;
            }

            if (typeEditors.TryGetValue(selectedEditor, out var editorInfo))
                return editorInfo.editor.GetPropertyHeight(property)
                    + EditorGUIUtility.singleLineHeight;
            return EditorGUIUtility.singleLineHeight;
        }

        public static string DisplayGenericPicker(
            Rect r,
            Type t,
            SerializedProperty property,
            string selectedEditor,
            string label)
        {
            Init();

            if (!editors.TryGetValue(t, out var l))
            {
                EditorGUI.LabelField(r, "No editors for type " + t.Name);
                return "";
            }

            var labels = new string[l.Count + 1];
            var i = 0;
            labels[i++] = "None";

            int selected = 0;
            foreach (var k in l.Keys)
            {
                if (k == selectedEditor)
                    selected = i;
                labels[i++] = k;
            }

            r = r.SingleLineRect();
            selected = EditorGUI.Popup(r, label, selected, labels);
            if (selected == 0)
            {
                property.managedReferenceValue = null;
                return "";
            }

            selectedEditor = labels[selected];
            var editorInfo = l[selectedEditor];

            if (!string.IsNullOrEmpty(editorInfo.docUrl))
            {
                var contextMenu = new GenericMenu();
                contextMenu.AddItem(new GUIContent("Docs"), false, () =>
                {
                    System.Diagnostics.Process.Start(editorInfo.docUrl);
                });
                EditorUtils.ContextMenu(r, contextMenu);
            }

            r.AdvanceOneLine();
            editorInfo.editor.DisplayPropertyGui(r, property);

            return selectedEditor;
        }

        static void Init()
        {
            if (editors != null)
                return;
            editors = new Dictionary<Type, Dictionary<string, EditorInfo>>();

            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes());

            foreach (var t in types)
            {
                var editorAttribute = Attribute
                    .GetCustomAttribute(t, typeof(PolymorphicPropertyEditorAttribute))
                    as PolymorphicPropertyEditorAttribute;

                if (editorAttribute == null)
                    continue;

                var rootType = editorAttribute.rootPropertyType;
                Dictionary<string, EditorInfo> l;
                if (!editors.TryGetValue(rootType, out l))
                    l = editors[rootType] = new Dictionary<string, EditorInfo>();
                var constructor = t.GetConstructor(Type.EmptyTypes)
                    ?? throw new Exception("Generic property editors (or auto editor property) must have a constructor " +
                    "which takes no parameters. No such constructor for " + t.Name);

                if (typeof(IPolymorphicPropertyEditor).IsAssignableFrom(t))
                {
                    var editor = constructor.Invoke(Array.Empty<object>())
                        as IPolymorphicPropertyEditor;

                    l[editorAttribute.label] = new EditorInfo
                    {
                        editor = editor,
                        docUrl = editorAttribute.docUrl
                    };
                }
                else if (rootType.IsAssignableFrom(t))
                {
                    l[editorAttribute.label] = new EditorInfo
                    {
                        editor = CreateAutoEditor(t, constructor),
                        docUrl = editorAttribute.docUrl
                    };
                }
                else
                    throw new Exception($"{t.Name} has attribute: ${nameof(PolymorphicPropertyEditorAttribute)}, " +
                        $"but does not inherit from ${nameof(IPolymorphicPropertyEditor)} nor the root type: ${rootType.Name}");
            }
        }

        static IPolymorphicPropertyEditor CreateAutoEditor(
            Type propertyType,
            ConstructorInfo constructor)
        {
            var allFields = propertyType.GetFields(
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic);

            var serializable = allFields
                .Where(f => f.IsPublic || f.GetCustomAttribute<SerializeField>() != null)
                .Select(f => f.Name)
                .ToArray();

            return new PolymorphicPropertyEditor(
                propertyType,
                () => constructor.Invoke(Array.Empty<object>()),
                serializable);
        }
    }
}
