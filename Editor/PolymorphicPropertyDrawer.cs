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

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AfterEver.Utilities.PolymorphicProperties
{
    public class PolymorphicPropertyDrawer<T> : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return PolymorphicPropertyManager.GenericPickerHeight(
                typeof(T),
                property.FindPropertyRelative("Property"),
                property.FindPropertyRelative("Editor").stringValue);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var e = property.FindPropertyRelative("Editor");
            var p = property.FindPropertyRelative("Property");

            e.stringValue = PolymorphicPropertyManager.DisplayGenericPicker(
                position,
                typeof(T),
                p,
                e.stringValue,
                property.displayName);
        }
    }
}
