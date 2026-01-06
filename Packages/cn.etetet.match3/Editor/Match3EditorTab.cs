using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ET.Match3.Editor
{
    /// <summary>
    /// 编辑器Tab基类
    /// </summary>
    public abstract class Match3EditorTab
    {
        protected Match3KitEditor parentEditor;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="editor">父编辑器窗口</param>
        public Match3EditorTab(Match3KitEditor editor)
        {
            parentEditor = editor;
        }

        /// <summary>
        /// 当Tab被选中时调用
        /// </summary>
        public virtual void OnTabSelected()
        {
        }

        /// <summary>
        /// 绘制Tab内容
        /// </summary>
        public virtual void Draw()
        {
        }

        /// <summary>
        /// 创建并初始化可重排序列表
        /// </summary>
        /// <typeparam name="T">列表元素类型</typeparam>
        /// <param name="headerText">列表标题</param>
        /// <param name="elements">元素列表</param>
        /// <param name="currentElement">当前选中元素引用</param>
        /// <param name="drawElement">绘制元素回调</param>
        /// <param name="selectElement">选中元素回调</param>
        /// <param name="createElement">创建元素回调</param>
        /// <param name="removeElement">删除元素回调</param>
        /// <returns>可重排序列表</returns>
        public static ReorderableList SetupReorderableList<T>(
            string headerText,
            List<T> elements,
            ref T currentElement,
            Action<Rect, T> drawElement,
            Action<T> selectElement,
            Action createElement,
            Action<T> removeElement)
        {
            var list = new ReorderableList(elements, typeof(T), true, true, true, true)
            {
                drawHeaderCallback = (Rect rect) => { EditorGUI.LabelField(rect, headerText); },
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    if (index >= 0 && index < elements.Count)
                    {
                        var element = elements[index];
                        drawElement(rect, element);
                    }
                }
            };

            list.onSelectCallback = l =>
            {
                if (l.index >= 0 && l.index < elements.Count)
                {
                    var selectedElement = elements[l.index];
                    selectElement(selectedElement);
                }
            };

            if (createElement != null)
            {
                list.onAddDropdownCallback = (buttonRect, l) =>
                {
                    createElement();
                };
            }

            list.onRemoveCallback = l =>
            {
                if (!EditorUtility.DisplayDialog("警告!", "确定要删除这个项目吗?", "是", "否"))
                {
                    return;
                }
                if (l.index >= 0 && l.index < elements.Count)
                {
                    var element = elements[l.index];
                    removeElement(element);
                    ReorderableList.defaultBehaviours.DoRemoveButton(l);
                }
            };

            return list;
        }
    }
}
