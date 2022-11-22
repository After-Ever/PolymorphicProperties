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

using UnityEngine;
using UnityEditor;

namespace AfterEver.Utilities.PolymorphicProperties
{
    public class PolymorphicPropertyEditor : IPolymorphicPropertyEditor
    {
        readonly Type propertyType;
        readonly Func<object> constructor;
        readonly string[] defaultDisplayProps;

        public PolymorphicPropertyEditor(
            Type propertyType,
            Func<object> constructor,
            string[] defaultDisplayProps = null)
        {
            this.propertyType = propertyType
                ?? throw new ArgumentNullException(nameof(constructor));
            this.constructor = constructor
                ?? throw new ArgumentNullException(nameof(constructor));
            this.defaultDisplayProps = defaultDisplayProps;
        }

        public void DisplayPropertyGui(Rect rect, SerializedProperty property)
        {
            Init(property);
            EditorGUI.indentLevel++;

            if (defaultDisplayProps != null)
            {
                rect = EditorUtils.RelativePropertyFields(
                    property,
                    defaultDisplayProps,
                    rect);
            }

            OnGUI(rect, property);
            EditorGUI.indentLevel--;
        }

        public float GetPropertyHeight(SerializedProperty property)
        {
            Init(property);

            var h = 0f;
            if (defaultDisplayProps != null)
                h = EditorUtils.RelativePropertyFieldsHeight(property, defaultDisplayProps);
            return h + GetHeight(property);
        }

        protected virtual float GetHeight(SerializedProperty property)
            => 0;

        protected virtual void OnGUI(Rect rect, SerializedProperty property) { }

        void Init(SerializedProperty property)
        {
            var cur = EditorUtils.GetTargetObjectWithProperty(property);
            if (cur == null || cur.GetType() != propertyType)
                property.managedReferenceValue = constructor();
        }
    }
}