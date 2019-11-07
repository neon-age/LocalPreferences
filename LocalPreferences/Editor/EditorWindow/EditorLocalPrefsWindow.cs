﻿// Made by Neonagee https://github.com/Neonagee/LocalPreferences
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using Neonagee.LocalPreferences;

namespace Neonagee.EditorInternal
{
    [CustomEditor(typeof(EditorLocalPrefs))]
    internal sealed class EditorLocalPrefsWindow : LocalPrefsWindowBase
    {
        static EditorLocalPrefs script;
        static EditorLocalPrefsWindow window;

        static string searchFilter = "";
        static bool searching;
        static int shownTypesCount;

        bool isInitialized;

        public PrefGUI<bool> boolGUI;
        public PrefGUI<int> intGUI;
        public PrefGUI<float> floatGUI;
        public PrefGUI<Vector2> vector2GUI;
        public PrefGUI<Vector3> vector3GUI;
        public PrefGUI<Vector4> vector4GUI;
        public PrefGUI<string> stringGUI;
        List<IPrefGUI> prefGUIs = new List<IPrefGUI>();

        [MenuItem("Edit/Local Preferences/Editor", priority = 2)]
        static void Open()
        {
            EditorLocalPrefs.Save(EditorLocalPrefs.defaultFileName);
            window = GetWindow<EditorLocalPrefsWindow>();
            window.titleContent = new GUIContent("Editor Local Prefs");
        }
        static void ClearAll()
        {

        }

        private void OnEnable()
        {
            window = this;
            isInitialized = false;
        }
        void Initialize()
        {
            script = EditorLocalPrefs.Data;
            EditorLocalPrefs.Load(EditorLocalPrefs.defaultFileName);
            FindSaveFiles(script.filesPath);

            EditorApplication.update += Repaint;

            prefGUIs.Clear();
            prefGUIs.Add(boolGUI = new PrefGUI<bool>("Boolean"));
            prefGUIs.Add(intGUI = new PrefGUI<int>("Int"));
            prefGUIs.Add(floatGUI = new PrefGUI<float>("Float"));
            prefGUIs.Add(vector2GUI = new PrefGUI<Vector2>("Vector2"));
            prefGUIs.Add(vector3GUI = new PrefGUI<Vector3>("Vector3"));
            prefGUIs.Add(vector4GUI = new PrefGUI<Vector4>("Vector4"));
            prefGUIs.Add(stringGUI = new PrefGUI<string>("String"));
        }
        private void OnDisable()
        {
            EditorLocalPrefs.Save(files[selectedFile]);
            EditorApplication.update -= Repaint;

            isInitialized = false;
        }

        string[] files = new string[0];
        int m_selectedFile;
        int selectedFile { get { return m_selectedFile = Mathf.Clamp(m_selectedFile, 0, files.Length - 1); } set { m_selectedFile = value; } }
        void FindSaveFiles(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            FileInfo[] info = dir.GetFiles("*?*" + EditorLocalPrefs.filesExtension);
            files = new string[info.Length];
            for (int i = 0; i < info.Length; i++)
            {
                files[i] = info[i].Name.Replace(EditorLocalPrefs.filesExtension, "");
            }
        }

        public struct PrefGUI<T> : IPrefGUI
        {
            public struct Changes
            {
                public string currentKey;
                public string newKey;
                public T value;
                public bool keyChanged;
                public bool valueChanged;
                public bool remove;
                public int index;
                public Changes(string CurrentKey, string NewKey, T Value, bool KeyChanged, bool ValueChanged, bool Remove, int Index)
                {
                    currentKey = CurrentKey;
                    newKey = NewKey;
                    value = Value;
                    remove = Remove;
                    keyChanged = KeyChanged;
                    valueChanged = ValueChanged;
                    index = Index;
                }
            }
            AnimBool collapse;
            readonly string PN_collapse;
            readonly System.Type type;
            readonly string label;
            readonly string valueName;
            List<Changes> changes;
            int foundItemsCount;
            bool nothingFound;

            public PrefGUI(string customLabel = null, string customValueName = null)
            {
                type = typeof(T);
                label = customLabel != null ? customLabel : type.Name;
                valueName = customValueName != null ? customValueName : label;
                PN_collapse = script.GetType().Name + " " + type.Name + " Collapsed";
                collapse = new AnimBool(window.Repaint) { speed = 3.5f, target = EditorLocalPrefs.GetBool(PN_collapse) };
                changes = new List<Changes>();
                foundItemsCount = 0;
                nothingFound = false;
            }
            public void Expand()
            {
                EditorLocalPrefs.SetBool(PN_collapse, true);
            }
            public void Collapse()
            {
                EditorLocalPrefs.SetBool(PN_collapse, false);
            }
            public Prefs<T> DoLayout(Prefs<T> prefs)
            {
                string filter = searchFilter.ToLower();
                nothingFound = false;
                if (!searching)
                    foundItemsCount = prefs.Count;
                else
                {
                    foundItemsCount = 0;
                    foreach (var value in prefs.dictionary)
                        if (value.Key.ToLower().Contains(filter))
                            foundItemsCount++;
                    nothingFound = foundItemsCount == 0;
                }
                if (window == null || script == null || nothingFound)
                    return prefs;
                shownTypesCount++;
                changes.Clear();
                EditorGUILayout.BeginVertical(regionBg, GUILayout.MaxHeight(18));
                EditorGUILayout.BeginHorizontal();
                collapse.target = GUILayout.Toggle(EditorLocalPrefs.GetBool(PN_collapse), label, foldout, GUILayout.ExpandWidth(true));
                GUILayout.FlexibleSpace();
                GUILayout.Button("[" + foundItemsCount + "]", EditorStyles.centeredGreyMiniLabel);
                EditorLocalPrefs.SetBool(PN_collapse, collapse.target);
                if (GUILayout.Button(new GUIContent("", !searching ? "Clear All" : "Clear Only Found Items"), "WinBtnCloseMac"))
                {
                    if (!searching)
                        prefs.ClearAll();
                    else
                    {
                        foreach (var value in prefs.dictionary)
                            if (!value.Key.ToLower().Contains(filter))
                                continue;
                            else
                                changes.Add(new Changes(value.Key, "", default, false, false, true, 0));
                    }
                    EditorUtility.SetDirty(script);
                }
                if (GUILayout.Button(new GUIContent("", "Add New Item"), "WinBtnMaxMac"))
                {
                    int keysCount = prefs.Length;
                    string newItemKey = "New " + valueName + " ";
                    while (prefs.ContainsKey(newItemKey + keysCount))
                        keysCount++;

                    prefs.Add(newItemKey + keysCount, default);
                    EditorUtility.SetDirty(script);
                }
                EditorGUILayout.EndHorizontal();
                if (EditorGUILayout.BeginFadeGroup(searching ? 1 : collapse.faded))
                {
                    if (prefs.Count > 0)
                    {
                        int i = 0;
                        foreach (var value in prefs.dictionary)
                        {
                            if (!value.Key.ToLower().Contains(filter))
                                continue;
                            EditorGUILayout.BeginHorizontal();
                            bool keyChanged = false;
                            bool valueChanged = false;
                            EditorGUI.BeginChangeCheck();
                            var newKey = EditorGUILayout.DelayedTextField(value.Key);
                            if (EditorGUI.EndChangeCheck())
                                keyChanged = true;

                            dynamic newValue = value.Value;
                            EditorGUI.BeginChangeCheck();

                            if (type == typeof(bool))
                                newValue = GUILayout.Toggle(newValue, newValue ? "True" : "False", EditorStyles.miniButton);

                            if (type == typeof(int))
                                newValue = EditorGUILayout.IntField(newValue);

                            if (type == typeof(float))
                                newValue = EditorGUILayout.FloatField(newValue);

                            if (type == typeof(Vector2))
                                newValue = EditorGUILayout.Vector2Field(GUIContent.none, newValue);

                            if (type == typeof(Vector3))
                                newValue = EditorGUILayout.Vector3Field(GUIContent.none, newValue);

                            if (type == typeof(Vector4))
                                newValue = EditorGUILayout.Vector4Field(GUIContent.none, newValue);

                            if (type == typeof(string))
                                newValue = EditorGUILayout.TextField(newValue);

                            if (EditorGUI.EndChangeCheck())
                                valueChanged = true;

                            // Remove button
                            bool remove = GUILayout.Button("", macRemoveButton);
                            EditorGUILayout.EndHorizontal();
                            // Write changes
                            if (keyChanged || valueChanged || remove)
                                changes.Add(new Changes(value.Key, newKey, newValue, keyChanged, valueChanged, remove, i));
                            i++;
                        }
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("List is empty.", MessageType.Info);
                    }
                }
                // Apply changes
                for (int c = 0; c < changes.Count; c++)
                {
                    var change = changes[c];
                    if (change.remove)
                    {
                        prefs.dictionary.Remove(change.currentKey);
                        continue;
                    }
                    if (change.keyChanged)
                    {
                        prefs.dictionary.Remove(change.currentKey);
                        prefs.dictionary.Add(change.newKey, change.value);
                    }
                    if (change.valueChanged)
                    {
                        prefs.dictionary[change.newKey] = change.value;
                    }
                }
                if (changes.Count > 0)
                    EditorUtility.SetDirty(script);
                EditorGUILayout.EndFadeGroup();
                EditorGUILayout.EndVertical();
                return prefs;
            }
        }

        Vector2 scrollPos;
        public void OnGUI()
        {
            if (files.Length == 0)
                isInitialized = false;
            if (!isInitialized)
            {
                Initialize();
                GetGUIStyles();
                isInitialized = true;
            }
            Undo.RecordObject(script, "Editor Local Prefs Window");

            EditorGUILayout.BeginHorizontal();
            if (EditorGUILayout.DropdownButton(new GUIContent(" " + files[selectedFile] + " "), FocusType.Passive, EditorStyles.toolbarDropDown))
            {
                GenericMenu menu = new GenericMenu();
                for (int i = 0; i < files.Length; i++)
                {
                    int index = i;
                    if (i != selectedFile)
                        menu.AddItem(new GUIContent(files[i]), false, () =>
                        {
                            EditorLocalPrefs.Save(files[selectedFile]);
                            selectedFile = index;
                            EditorLocalPrefs.Load(files[index]);
                        });
                    else
                        menu.AddDisabledItem(new GUIContent(files[i]));
                }
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Refresh"), false, () =>
                {
                    FindSaveFiles(script.filesPath);
                });
                menu.AddItem(new GUIContent("Open Files Folder"), false, () =>
                {
                    EditorUtility.RevealInFinder(script.filesPath + files[selectedFile] + EditorLocalPrefs.filesExtension);
                });
                menu.ShowAsContext();
            }
            GUILayout.Space(6);
            searchFilter = EditorGUILayout.TextField(searchFilter, searchTextField, GUILayout.MinWidth(0));
            searchFilter = GUILayout.Button("", searchCancelButton) ? "" : searchFilter;
            searching = searchFilter != "";

            if (GUILayout.Button(" Save ", EditorStyles.toolbarButton))
            {
                EditorLocalPrefs.Save(EditorLocalPrefs.defaultFileName);
            }
            if (GUILayout.Button(GUIContent.none, EditorStyles.toolbarDropDown, GUILayout.Width(17)))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Save As New"), false, () =>
                {
                    EditorLocalPrefs.Save(files[selectedFile] + " (New)");
                    FindSaveFiles(script.filesPath);
                    EditorLocalPrefs.Load(files[selectedFile]);
                });
                menu.AddItem(new GUIContent("Delete File"), false, () =>
                {
                    EditorLocalPrefs.DeleteFile(files[selectedFile]);
                    FindSaveFiles(script.filesPath);
                    EditorLocalPrefs.Load(files[selectedFile]);
                });
                menu.ShowAsContext();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            shownTypesCount = 0;
            script.bools = (PrefsBool)boolGUI.DoLayout(script.bools);
            script.ints = (PrefsInt)intGUI.DoLayout(script.ints);
            script.floats = (PrefsFloat)floatGUI.DoLayout(script.floats);
            script.vector2 = (PrefsVector2)vector2GUI.DoLayout(script.vector2);
            script.vector3 = (PrefsVector3)vector3GUI.DoLayout(script.vector3);
            script.vector4 = (PrefsVector4)vector4GUI.DoLayout(script.vector4);
            script.strings = (PrefsString)stringGUI.DoLayout(script.strings);
            if (shownTypesCount == 0)
                EditorGUILayout.LabelField(new GUIContent("Nothing found."), notificationBoxSmall);
            EditorGUILayout.EndScrollView();
        }
    }
}