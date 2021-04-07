using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(ItemManager))]
public class ItemManagerEditor : Editor
{
    private ReorderableList itemList;

    private void OnEnable()
    {
        SerializedProperty prop = serializedObject.FindProperty("itemCreator");
        itemList = new ReorderableList(serializedObject, prop
            , true, true, true, true);

        //自定义列表名称
        itemList.drawHeaderCallback = (Rect rect) =>
        {
            GUI.Label(rect, prop.displayName);
        };

        //定义元素的高度
        itemList.elementHeight = 68;

        //自定义绘制列表元素
        itemList.drawElementCallback = (Rect rect,int index,bool selected,bool focused) =>
        {
            //根据index获取对应元素 
            SerializedProperty item = itemList.serializedProperty.GetArrayElementAtIndex(index);
            rect.height -=4;
            rect.y += 2;
            EditorGUI.PropertyField(rect, item,new GUIContent("Index "+index));
        };

        //当删除元素时候的回调函数，实现删除元素时，有提示框跳出
        itemList.onRemoveCallback = (ReorderableList list) =>
        {
            // if (EditorUtility.DisplayDialog("Warnning","Do you want to remove this element?","Remove","Cancel"))
            // {
            //     ReorderableList.defaultBehaviours.DoRemoveButton(list);
            // }
            ReorderableList.defaultBehaviours.DoRemoveButton(list);
        };
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();
        //自动布局绘制列表
        itemList.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
    }
}
