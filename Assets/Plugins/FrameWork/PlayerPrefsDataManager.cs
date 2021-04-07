using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SkyFrameWork
{
    public class PlayerPrefsDataManager
    {
        private static PlayerPrefsDataManager instance = new PlayerPrefsDataManager();
        
        public static PlayerPrefsDataManager Instance { get => instance; }

        public void SaveImmediately()
        {
            PlayerPrefs.Save();
        }
        
        /// <summary>
        /// 存储数据
        /// </summary>
        /// <param name="key">唯一键值</param>
        /// <param name="data">数据类</param>
        public void SaveData(string key,object data)
        {
            var dataType = data.GetType();
            var infos = dataType.GetFields();

            foreach (var item in infos)
            {
                // 键名_数据类型_字段类型_字段名
                var keyName = $"{key}_{dataType.Name}_{item.FieldType.Name}_{item.Name}";
                SaveValue(keyName,item.GetValue(data));
            }
        }
        
        /// <summary>
        /// 存储键值对
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        private void SaveValue(string key,object value)
        {
            var fieldType = value.GetType();
            #if SkyFrameWork_Debug
            Debug.Log($"[PlayerPrefsDataManager]储存 {key} 中。");
            #endif
            if(fieldType == typeof(int))
            {
                PlayerPrefs.SetInt(key, (int)value);
            }
            else if (fieldType == typeof(float))
            {
                PlayerPrefs.SetFloat(key, (float)value);
            }
            else if (fieldType == typeof(string))
            {
                PlayerPrefs.SetString(key, value.ToString());
            }
            else if (fieldType == typeof(bool))
            {
                PlayerPrefs.SetInt(key, (bool)value ? 1 : 0);
            }
            else if (value is IList list)
            {
                PlayerPrefs.SetInt(key,list.Count);
                var index = 0;
                foreach (var item in list)
                {
                    SaveValue($"{key}[{index}]_value",item);
                    index++;
                }
            }
            else if (value is IDictionary dictionary)
            {
                PlayerPrefs.SetInt(key,dictionary.Count);
                var index = 0;
                foreach (var item in dictionary.Keys)
                {
                    SaveValue($"{key}[{index}]_key",item);
                    SaveValue($"{key}[{index}]_value",dictionary[item]);
                    index++;
                }
            }
            else if (value is ValueType)
            {
                Debug.LogError($"[PlayerPrefsDataManager]储存 {key} 失败！，{fieldType.ToString()}不在储存数据类型范围里。");
            }
            else
            {
                SaveData(key,value);
            }
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="key">唯一键值</param>
        /// <param name="dataType">数据类型</param>
        /// <returns></returns>
        public object LoadData(string key,Type dataType)
        {
            var infos = dataType.GetFields();
            var obj = Activator.CreateInstance(dataType,true);
            foreach (var item in infos)
            {
                // 键名_数据类型_字段类型_字段名
                var keyName = $"{key}_{dataType.Name}_{item.FieldType.Name}_{item.Name}";
                item.SetValue(obj,LoadValue(keyName,item.FieldType));
            }
            return obj;
        }

        private object LoadValue(string key, Type fieldType)
        {
            
            object value = null;
            if(fieldType == typeof(int))
            {
                value = PlayerPrefs.GetInt(key,0);
            }
            else if (fieldType == typeof(float))
            {
                value = PlayerPrefs.GetFloat(key,0f);
            }
            else if (fieldType == typeof(string))
            {
                value = PlayerPrefs.GetString(key,"");
            }
            else if (fieldType == typeof(bool))
            {
                value = PlayerPrefs.GetInt(key,0) != 0;
            }
            else if (typeof(IList).IsAssignableFrom(fieldType))
            {
                var count = PlayerPrefs.GetInt(key,0);
                value = Activator.CreateInstance(fieldType);
                if (value is IList list)
                {
                    var genericArgument = fieldType.GetGenericArguments()[0];
                    for (int i = 0; i < count; i++)
                    {
                        
                        list.Add(LoadValue($"{key}[{i}]_value", genericArgument));
                    }
                }
            }
            else if (typeof(IDictionary).IsAssignableFrom(fieldType))
            {
                var count = PlayerPrefs.GetInt(key,0);
                value = Activator.CreateInstance(fieldType);
                if (value is IDictionary dic)
                {
                    var genericArguments = fieldType.GetGenericArguments();
                    for (int i = 0; i < count; i++)
                    {
                        
                        dic.Add(LoadValue($"{key}[{i}]_key",genericArguments[0]),LoadValue($"{key}[{i}]_value", genericArguments[1]));
                    }
                }
            }
            else if (typeof(ValueType).IsAssignableFrom(fieldType))
            {
                Debug.LogError($"[PlayerPrefsDataManager]读取 {key} 失败！，{fieldType.ToString()}不在储存数据类型范围里。");
            }
            else
            {
                value = LoadData(key, fieldType);
            }
            #if SkyFrameWork_Debug
            Debug.Log($"[PlayerPrefsDataManager]读取 {key} ，结果为 {(value != null ? value.ToString() : "null")}");
            #endif
            return value;
        }
    }
}