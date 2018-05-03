﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

using Newtonsoft.Json;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ModIO
{
    public static class Utility
    {
        // Author: @jon-hanna of StackOverflow (https://stackoverflow.com/users/400547/jon-hanna)
        // URL: https://stackoverflow.com/a/5419544
        public static bool Like(this string toSearch, string toFind)
        {
            return new Regex(@"\A" + new Regex(@"\.|\$|\^|\{|\[|\(|\||\)|\*|\+|\?|\\").Replace(toFind, ch => @"\" + ch).Replace('_', '.').Replace("%", ".*") + @"\z", RegexOptions.Singleline).IsMatch(toSearch);
        }

        public static bool IsURL(string toCheck)
        {
            // URL Regex adapted from https://regex.wtf/url-matching-regex-javascript/
            string protocol = "^(http(s)?(://))?(www.)?";
            string domain = "[a-zA-Z0-9-_.]+";
            Regex urlRegex = new Regex(protocol + domain, RegexOptions.IgnoreCase);

            return urlRegex.IsMatch(toCheck);
        }

        public static string GetMD5ForFile(string path)
        {
            Debug.Assert(System.IO.File.Exists(path));
            return GetMD5ForData(System.IO.File.ReadAllBytes(path));
        }

        public static string GetMD5ForData(byte[] data)
        {
            string hashString = "";
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                hashString = BitConverter.ToString(md5.ComputeHash(data)).Replace("-", "").ToLowerInvariant();
            }
            return hashString;
        }

        public static bool TryParseJsonFile<T>(string filePath,
                                               out T targetObject)
        {
            try
            {
                targetObject = JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath));
            }
            #pragma warning disable CS0168
            catch(Exception e)
            {
                targetObject = default(T);
                return false;
            }
            #pragma warning restore CS0168

            return true;
        }

        public static bool TryParseJsonString<T>(string jsonObject,
                                                 out T targetObject)
        {
            try
            {
                targetObject = JsonConvert.DeserializeObject<T>(jsonObject);
            }
            #pragma warning disable CS0168
            catch(Exception e)
            {
                targetObject = default(T);
                return false;
            }
            #pragma warning restore CS0168

            return true;
        }

        public static bool TryReadAllFileBytes(string filePath,
                                               out byte[] data)
        {
            bool retVal = false;
            try
            {
                data = File.ReadAllBytes(filePath);
                retVal = true;
            }
            #pragma warning disable CS0168
            catch(Exception e)
            {
                data = null;
                retVal = false;
            }
            #pragma warning restore CS0168
            return retVal;
        }

        public static bool TryLoadTextureFromFile(string filePath,
                                                  out Texture2D texture)
        {
            bool retVal;
            try
            {
                texture = new Texture2D(0, 0);
                texture.LoadImage(File.ReadAllBytes(filePath));
                retVal = true;
            }
            #pragma warning disable CS0168
            catch(Exception e)
            {
                texture = null;
                retVal = false;
            }
            #pragma warning restore CS0168
            return retVal;
        }

        public static bool TryWriteTextureToPNG(string filePath,
                                                Texture2D texture)
        {
            bool retVal = true;
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                File.WriteAllBytes(filePath, texture.EncodeToPNG());
            }
            #pragma warning disable CS0168
            catch(Exception e)
            {
                retVal = false;
            }
            #pragma warning restore CS0168
            return retVal;
        }

        public static T[] CollectionToArray<T>(ICollection<T> collection)
        {
            T[] array = new T[collection.Count];
            collection.CopyTo(array, 0);
            return array;
        }

        public static T[] SafeCopyArrayOrZero<T>(T[] array)
        {
            if(array == null)
            {
                return new T[0];
            }

            var retVal = new T[array.Length];
            Array.Copy(array, retVal, array.Length);
            return retVal;
        }
        
        public static void SafeMapArraysOrZero<T1, T2>(T1[] sourceArray,
                                                       Func<T1, T2> mapElementDelegate,
                                                       out T2[] destinationArray)
        {
            if(sourceArray == null) { destinationArray = new T2[0]; }
            else
            {
                destinationArray = new T2[sourceArray.Length];
                for(int i = 0;
                    i < sourceArray.Length;
                    ++i)
                {
                    destinationArray[i] = mapElementDelegate(sourceArray[i]);
                }
            }
        }
    }

    // TODO(@jackson): Remove after ModMediaObject.Equals is removed
    // Author: @ohad-schneider of StackOverflow (https://stackoverflow.com/users/67824/ohad-schneider)
    // URL: https://stackoverflow.com/a/3790621
    public class MultiSetComparer<T> : IEqualityComparer<IEnumerable<T>>
    {
        private readonly IEqualityComparer<T> m_comparer;
        public MultiSetComparer(IEqualityComparer<T> comparer = null)
        {
            m_comparer = comparer ?? EqualityComparer<T>.Default;
        }

        public bool Equals(IEnumerable<T> first, IEnumerable<T> second)
        {
            if (first == null)
                return second == null;

            if (second == null)
                return false;

            if (ReferenceEquals(first, second))
                return true;

            var firstCollection = first as ICollection<T>;
            var secondCollection = second as ICollection<T>;
            if (firstCollection != null && secondCollection != null)
            {
                if (firstCollection.Count != secondCollection.Count)
                    return false;

                if (firstCollection.Count == 0)
                    return true;
            }

            return !HaveMismatchedElement(first, second);
        }

        private bool HaveMismatchedElement(IEnumerable<T> first, IEnumerable<T> second)
        {
            int firstNullCount;
            int secondNullCount;

            var firstElementCounts = GetElementCounts(first, out firstNullCount);
            var secondElementCounts = GetElementCounts(second, out secondNullCount);

            if (firstNullCount != secondNullCount || firstElementCounts.Count != secondElementCounts.Count)
                return true;

            foreach (var kvp in firstElementCounts)
            {
                var firstElementCount = kvp.Value;
                int secondElementCount;
                secondElementCounts.TryGetValue(kvp.Key, out secondElementCount);

                if (firstElementCount != secondElementCount)
                    return true;
            }

            return false;
        }

        private Dictionary<T, int> GetElementCounts(IEnumerable<T> enumerable, out int nullCount)
        {
            var dictionary = new Dictionary<T, int>(m_comparer);
            nullCount = 0;

            foreach (T element in enumerable)
            {
                if (element == null)
                {
                    nullCount++;
                }
                else
                {
                    int num;
                    dictionary.TryGetValue(element, out num);
                    num++;
                    dictionary[element] = num;
                }
            }

            return dictionary;
        }

        public int GetHashCode(IEnumerable<T> enumerable)
        {
            if (enumerable == null) throw new ArgumentNullException();

            int hash = 17;

            foreach (T val in enumerable.OrderBy(x => x))
                hash = hash * 23 + (val == null ? 42 : val.GetHashCode());

            return hash;
        }
    }

    #if UNITY_EDITOR
    public static class EditorUtilityExtensions
    {
        public static string[] GetSerializedPropertyStringArray(SerializedProperty arrayProperty)
        {
            Debug.Assert(arrayProperty.isArray);
            Debug.Assert(arrayProperty.arrayElementType.Equals("string"));
            
            var retVal = new string[arrayProperty.arraySize];
            for(int i = 0;
                i < arrayProperty.arraySize;
                ++i)
            {
                retVal[i] = arrayProperty.GetArrayElementAtIndex(i).stringValue;
            }
            return retVal;
        }

        public static void SetSerializedPropertyStringArray(SerializedProperty arrayProperty,
                                                            string[] value)
        {
            Debug.Assert(arrayProperty.isArray);
            Debug.Assert(arrayProperty.arrayElementType.Equals("string"));

            arrayProperty.arraySize = value.Length;
            for(int i = 0;
                i < value.Length;
                ++i)
            {
                arrayProperty.GetArrayElementAtIndex(i).stringValue = value[i];
            }
        }
    }

    public static class EditorGUIExtensions
    {
        public static string MultilineTextField(Rect position, string content)
        {
            bool wasWordWrapEnabled = GUI.skin.textField.wordWrap;
            
            GUI.skin.textField.wordWrap = true;

            string retVal = EditorGUI.TextField(position, content);

            GUI.skin.textField.wordWrap = wasWordWrapEnabled;

            return retVal;
        }

    }

    public static class EditorGUILayoutExtensions
    {
        public static void ArrayPropertyField(SerializedProperty arrayProperty,
                                              string arrayLabel,
                                              ref bool isExpanded)
        {
            CustomLayoutArrayPropertyField(arrayProperty, arrayLabel, ref isExpanded,
                                           (p) => EditorGUILayout.PropertyField(p));
        }

        public static void CustomLayoutArrayPropertyField(SerializedProperty arrayProperty,
                                                          string arrayLabel,
                                                          ref bool isExpanded,
                                                          Action<SerializedProperty> customLayoutFunction)
        {
            isExpanded = EditorGUILayout.Foldout(isExpanded, arrayLabel, true);

            if(isExpanded)
            {
                EditorGUI.indentLevel += 3;
         
                EditorGUILayout.PropertyField(arrayProperty.FindPropertyRelative("Array.size"),
                                              new GUIContent("Size"));

                for (int i = 0; i < arrayProperty.arraySize; ++i)
                {
                    SerializedProperty prop = arrayProperty.FindPropertyRelative("Array.data[" + i + "]");
                    customLayoutFunction(prop);
                }

                EditorGUI.indentLevel -= 3;
            }
        }

        // TODO(@jackson): Add a clear button
        public static bool BrowseButton(string path, GUIContent label)
        {
            bool doBrowse = false;

            if(String.IsNullOrEmpty(path))
            {
                path = "Browse...";
            }

            EditorGUILayout.BeginHorizontal();
                if(label != null && label != GUIContent.none)
                {
                    EditorGUILayout.PrefixLabel(label);
                }

                if(Event.current.type == EventType.Layout)
                {
                    EditorGUILayout.TextField(path, "");
                }
                else
                {
                    doBrowse = GUILayout.Button(path, GUI.skin.textField);
                }
            EditorGUILayout.EndHorizontal();

            return doBrowse;
        }

        private static GUILayoutOption[] buttonLayout = new GUILayoutOption[]{ GUILayout.Width(EditorGUIUtility.singleLineHeight), GUILayout.Height(EditorGUIUtility.singleLineHeight) };
        public static bool UndoButton(bool isEnabled = true)
        {
            using (new EditorGUI.DisabledScope(!isEnabled))
            {
                return GUILayout.Button(UISettings.Instance.EditorTexture_UndoButton,
                                        GUI.skin.label,
                                        buttonLayout);
            }
        }

        public static string MultilineTextField(string content)
        {
            Rect controlRect = EditorGUILayout.GetControlRect(false, 130.0f, null);
            return EditorGUIExtensions.MultilineTextField(controlRect, content);
        }
    }
    #endif
}