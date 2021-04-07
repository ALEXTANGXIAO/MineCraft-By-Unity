using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ItemPrototype))] 
public class ItemProtoDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        using (new EditorGUI.PropertyScope(position,label,property))
        {
            //设置属性名宽度
            EditorGUIUtility.labelWidth = 60;
            position.height = EditorGUIUtility.singleLineHeight;

            var iconRect = new Rect(position)
            {
                width = 64,
                height = 64
            };

            var prefabRect = new Rect(position)
            {
                width = position.width - 80,
                x = position.x + 80
            };

            var idRect = new Rect(iconRect) 
            {
                width = position.width - 80,
                height = EditorGUIUtility.singleLineHeight,
                x = position.x + 80,
                y = prefabRect.y
            };

            var bindRect = new Rect(idRect)
            {
                y = idRect.y + EditorGUIUtility.singleLineHeight + 5
            };
            
            var stackRect = new Rect(bindRect)
            {
                y = bindRect.y + EditorGUIUtility.singleLineHeight + 5
            };
            
            var iconProperty = property.FindPropertyRelative("icon");
            var idProperty = property.FindPropertyRelative("id");
            var bindProperty = property.FindPropertyRelative("bindBlock");
            var stackProperty = property.FindPropertyRelative("stackMax");

            iconProperty.objectReferenceValue = EditorGUI.ObjectField(iconRect, iconProperty.objectReferenceValue, typeof(Sprite), false);
            idProperty.stringValue = EditorGUI.TextField(idRect, idProperty.displayName,idProperty.stringValue);
            bindProperty.stringValue = EditorGUI.TextField(bindRect, bindProperty.displayName,bindProperty.stringValue);
            stackProperty.intValue = EditorGUI.IntSlider(stackRect, stackProperty.displayName,stackProperty.intValue,1,1024);
        }
    }
    
}