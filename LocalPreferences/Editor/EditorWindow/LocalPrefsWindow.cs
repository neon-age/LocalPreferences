// Made by Neonagee https://github.com/Neonagee/LocalPreferences
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using Neonagee.LocalPreferences;
using System.Text.RegularExpressions;

namespace Neonagee.EditorInternal
{
    [CustomEditor(typeof(LocalPrefs))]
    internal sealed class LocalPrefsWindow : LocalPrefsWindowBase
    {
        static LocalPrefs script;
        static LocalPrefsWindow window;

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

        [MenuItem("Edit/Local Preferences/Player", priority = 1)]
        static void Open()
        {
            window = GetWindow<LocalPrefsWindow>();
            window.titleContent = new GUIContent("Local Prefs");
        }

        private void OnEnable()
        {
            window = this;
            isInitialized = false;
            script = LocalPrefs.Data;
            EditorApplication.playModeStateChanged += Initialize;
        }
        void Initialize(PlayModeStateChange state) => Initialize();
        void Initialize()
        {
            FindSaveFiles(script.FilesPath);
            if (!Application.isPlaying)
                LocalPrefs.Load(files[selectedFile]);

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
        string[] files = new string[0];
        int m_selectedFile;
        int selectedFile { get { return m_selectedFile = Mathf.Clamp(m_selectedFile, 0, files.Length - 1); } set { m_selectedFile = value; } }
        void FindSaveFiles(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            FileInfo[] info = dir.GetFiles("*?*" + LocalPrefs.filesExtension);
            files = new string[info.Length];
            for(int i = 0; i < info.Length; i++)
            {
                files[i] = info[i].Name.Replace(LocalPrefs.filesExtension, "");
            }
            if (files.Length == 0)
                files = new string[1] { LocalPrefs.defaultFileName };
        }
        public static void MoveElement<T>(ref List<T> list, int from, int to)
        {
            var obj = list[from];
            list.RemoveAt(from);
            while (to < 0) to++;
            while (to > list.Count) to--;
            list.Insert(to, obj);
        }
        private void OnDisable()
        {
            if(!EditorApplication.isPlayingOrWillChangePlaymode)
                LocalPrefs.Save(files[selectedFile]);
            EditorApplication.update -= Repaint;

            isInitialized = false;
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
                showItem = new bool[0];
            }
            public void Expand()
            {
                EditorLocalPrefs.SetBool(PN_collapse, true);
            }
            public void Collapse()
            {
                EditorLocalPrefs.SetBool(PN_collapse, false);
            }
            bool[] showItem;
            public Prefs<T> DoLayout(Prefs<T> prefs)
            {
                string filter = searchFilter.ToLower();
                nothingFound = false;
                if (!searching)
                    foundItemsCount = prefs.Count;
                else
                {
                    showItem = new bool[prefs.dictionary.Count];
                    foundItemsCount = 0;
                    int i = 0;
                    foreach (var value in prefs.dictionary)
                    {
                        if (value.Key.ToLower().Contains(filter))
                        {
                            showItem[i] = true;
                            foundItemsCount++;
                        }
                        i++;
                    }
                    nothingFound = foundItemsCount == 0;
                }
                if (window == null || script == null || nothingFound)
                    return prefs;
                shownTypesCount++;
                changes.Clear();
                EditorGUILayout.BeginVertical(regionBg, GUILayout.MaxHeight(18));
                EditorGUILayout.BeginHorizontal();

                EditorGUI.BeginChangeCheck();
                collapse.target = GUILayout.Toggle(searching ? true : EditorLocalPrefs.GetBool(PN_collapse), 
                                  label, foldout, GUILayout.ExpandWidth(true));
                if (EditorGUI.EndChangeCheck())
                {
                    EditorLocalPrefs.SetBool(PN_collapse, collapse.target);
                }

                GUILayout.FlexibleSpace();
                GUILayout.Button("[" + foundItemsCount + "]", EditorStyles.centeredGreyMiniLabel);
                
                if (GUILayout.Button(new GUIContent("", !searching ? "Clear All" : "Remove Found Items"), "WinBtnCloseMac"))
                {
                    if (!searching)
                        prefs.ClearAll();
                    else
                    {
                        int i = 0;
                        foreach (var value in prefs.dictionary)
                        {
                            if (!showItem[i])
                            {
                                i++;
                                continue;
                            }
                            else
                                changes.Add(new Changes(value.Key, "", default, false, false, true, 0));
                            i++;
                        }
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
                if (EditorGUILayout.BeginFadeGroup(collapse.faded))
                {
                    if (prefs.Count > 0)
                    {
                        int i = 0;
                        foreach (var value in prefs.dictionary)
                        {
                            if (searching && !showItem[i])
                            {
                                i++;
                                continue;
                            }
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
        void Refresh()
        {
            FindSaveFiles(script.FilesPath);
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
            if (script == null)
                script = LocalPrefs.Data;
            Undo.RecordObject(script, "Local Prefs Window");

            EditorGUILayout.BeginHorizontal();
            if (EditorGUILayout.DropdownButton(!Application.isPlaying ?
                new GUIContent(" " + files[selectedFile] + " ") : new GUIContent(" " + LocalPrefs.currentFile + " "), 
                FocusType.Passive, EditorStyles.toolbarDropDown))
            {
                GenericMenu menu = new GenericMenu();
                if (!Application.isPlaying)
                {
                    for (int i = 0; i < files.Length; i++)
                    {
                        int index = i;
                        if (i != selectedFile)
                            menu.AddItem(new GUIContent(files[i]), false, () =>
                            {
                                LocalPrefs.Save(files[selectedFile]);
                                selectedFile = index;
                                LocalPrefs.Load(files[index]);
                                Refresh();
                            });
                        else
                            menu.AddDisabledItem(new GUIContent(files[i]));
                    }
                    menu.AddSeparator("");
                    menu.AddItem(new GUIContent("Refresh"), false, () =>
                    {
                        Refresh();
                    });
                    menu.AddItem(new GUIContent("Open Files Folder"), false, () =>
                    {
                        EditorUtility.RevealInFinder(script.FilesPath + files[selectedFile] + LocalPrefs.filesExtension);
                    });
                }
                else
                {
                    menu.AddDisabledItem(new GUIContent("Please Exit Playmode to Change File"));
                    menu.AddSeparator("");
                    for (int i = 0; i < files.Length; i++)
                    {
                        menu.AddDisabledItem(new GUIContent(files[i]));
                    }
                    menu.AddSeparator("");
                    menu.AddItem(new GUIContent("Open Files Folder"), false, () =>
                    {
                        EditorUtility.RevealInFinder(script.FilesPath + files[selectedFile] + LocalPrefs.filesExtension);
                    });
                }
                menu.ShowAsContext();
            }
            GUILayout.Space(6);
            searchFilter = EditorGUILayout.TextField(searchFilter, searchTextField, GUILayout.MinWidth(0));
            searchFilter = GUILayout.Button("", searchCancelButton) ? "" : searchFilter;
            searching = searchFilter != "";

            if (GUILayout.Button(" Save ", EditorStyles.toolbarButton))
            {
                LocalPrefs.Save(files[selectedFile]);
                Refresh();
            }
            if (GUILayout.Button(GUIContent.none, EditorStyles.toolbarDropDown, GUILayout.Width(17)))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Save As New"), false, () =>
                {
                    LocalPrefs.Save(files[selectedFile] + " (New)");
                    Refresh();
                    LocalPrefs.Load(files[selectedFile]);
                });
                menu.AddItem(new GUIContent("Delete File"), false, () =>
                {
                    if(EditorUtility.DisplayDialog("Delete " + files[selectedFile], "Are you sure you want to delete this file?\nThis action cannot be undone.", "Yes", "Cancel"))
                    {
                        LocalPrefs.DeleteFile(files[selectedFile]);
                        Refresh();
                        LocalPrefs.Load(files[selectedFile]);
                    }
                });
                menu.ShowAsContext();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            script.enableEncryption = EditorGUILayout.Toggle("Encrypt Data", script.enableEncryption);
            script.autoSaveOnQuit = EditorGUILayout.Toggle("Auto Save On Quit", script.autoSaveOnQuit);

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