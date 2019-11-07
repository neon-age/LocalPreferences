// Made by Neonagee https://github.com/Neonagee/LocalPreferences
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Neonagee.LocalPreferences;

public sealed class EditorLocalPrefs : ScriptableObject
{
    public static readonly string defaultFileName = "EditorPrefs";
    public static readonly string filesExtension = ".xg";
    public string filesPath
    {
        get
        {
            string path = Application.dataPath;
            path = path.Remove(path.Length - 6, 6);
            path += "Library/";
            return path;
        }
    }
    public static string currentFile;

    public static void Save(string fileName)
    {
        string filePath = Data.filesPath + fileName + filesExtension;
        string json = JsonUtility.ToJson(m_Data);
        File.WriteAllText(filePath, json);
    }
    public static void Load(string fileName)
    {
        string filePath = Data.filesPath + fileName + filesExtension;
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            JsonUtility.FromJsonOverwrite(json, m_Data);
        }
        else
            Save(fileName);
        currentFile = fileName;
    }
    public static void DeleteFile(string fileName)
    {
        string filePath = Data.filesPath + fileName + filesExtension;
        if (File.Exists(filePath))
            File.Delete(filePath);
    }

    static EditorLocalPrefs m_Data;
    public static EditorLocalPrefs Data
    {
        get
        {
            if (m_Data != null)
                return m_Data;

            m_Data = CreateInstance<EditorLocalPrefs>();

            m_Data.Initialize();
            Load(defaultFileName);

            return m_Data;
        }
    }

    private void OnDisable()
    {
        Save(currentFile);
    }

    public Dictionary<Type, IPrefs> prefs = new Dictionary<Type, IPrefs>();

    public void Initialize()
    {
        AddPref(bools);
        AddPref(ints);
        AddPref(floats);
        AddPref(vector2);
        AddPref(vector3);
        AddPref(vector4);
        AddPref(strings);
    }
    void AddPref<T>(T pref) where T : IPrefs
    {
        prefs.Add(typeof(T), pref);
    }
    public PrefsBool bools = new PrefsBool();
    public PrefsInt ints = new PrefsInt();
    public PrefsFloat floats = new PrefsFloat();
    public PrefsVector2 vector2 = new PrefsVector2();
    public PrefsVector3 vector3 = new PrefsVector3();
    public PrefsVector4 vector4 = new PrefsVector4();
    public PrefsString strings = new PrefsString();

    // Bool
    public static bool GetBool(string key, bool defaultValue = default)
    {
        return Data.bools.GetPref(key, defaultValue);
    }
    public static bool SetBool(string key, bool value)
    {
        return Data.bools.SetPref(key, value);
    }

    // Integer
    public static int GetInt(string key, int defaultValue = default)
    {
        return Data.ints.GetPref(key, defaultValue);
    }
    public static int SetInt(string key, int value)
    {
        return Data.ints.SetPref(key, value);
    }

    // Float
    public static float GetFloat(string key, float defaultValue = default)
    {
        return Data.floats.GetPref(key, defaultValue);
    }
    public static float SetFloat(string key, float value)
    {
        return Data.floats.SetPref(key, value);
    }

    // Vector2
    public static Vector2 GetVector2(string key, Vector2 defaultValue = default)
    {
        return Data.vector2.GetPref(key, defaultValue);
    }
    public static Vector2 SetVector2(string key, Vector2 value)
    {
        return Data.vector2.SetPref(key, value);
    }

    // Vector3
    public static Vector3 GetVector3(string key, Vector3 defaultValue = default)
    {
        return Data.vector3.GetPref(key, defaultValue);
    }
    public static Vector3 SetVector3(string key, Vector3 value)
    {
        return Data.vector3.SetPref(key, value);
    }

    // Vector4
    public static Vector4 GetVector4(string key, Vector4 defaultValue = default)
    {
        return Data.vector4.GetPref(key, defaultValue);
    }
    public static Vector4 SetVector4(string key, Vector4 value)
    {
        return Data.vector4.SetPref(key, value);
    }

    // String
    public static string GetString(string key, string defaultValue = default)
    {
        return Data.strings.GetPref(key, defaultValue);
    }
    public static string SetString(string key, string value = default)
    {
        return Data.strings.SetPref(key, value);
    }

    /// <summary>Returns true if key exist in preferences.</summary>
    public static bool HasKey(string key)
    {
        foreach (var pref in Data.prefs.Values)
            if (pref.ContainsKey(key))
                return true;
        return false;
    }
    /// <summary>Delete all keys and values from preferences. Use with caution.</summary>
    public static void DeleteAll()
    {
        foreach (var pref in Data.prefs.Values)
            pref.ClearAll();
    }
    /// <summary>Delete key and it's value from preferences.</summary>
    public static bool DeleteKey(string key)
    {
        bool deleted = false;
        foreach (var pref in Data.prefs.Values)
            if (pref.DeleteKey(key))
                deleted = true;
        return deleted;
    }


    // Generic functions
    /// <summary>Returns true if key exist in given preference.</summary>
    public static bool HasKey<T>(string key)
    {
        Type t = typeof(T);
        bool isSupported = false;
        if (Data.prefs.TryGetValue(t, out IPrefs pref))
        {
            isSupported = true;
            if (pref.ContainsKey(key))
                return true;
        }
        if (!isSupported)
            Debug.LogError(TypeIsNotSupported("LocalPrefs HasKey<T>", t));
        return false;
    }
    /// <summary>Delete key and it's value from given preference.</summary>
    public static bool DeleteKey<T>(string key)
    {
        Type t = typeof(T);
        if (Data.prefs.TryGetValue(t, out IPrefs pref))
        {
            if (pref.DeleteKey(key))
            {
                pref.DeleteKey(key);
                return true;
            }
        }
        Debug.LogError(TypeIsNotSupported("LocalPrefs DeleteKey<T>", t));
        return false;
    }
    /// <summary>Delete all keys and values from given preference. Use with caution.</summary>
    public static void DeleteAll<T>()
    {
        Type t = typeof(T);
        if (Data.prefs.TryGetValue(t, out IPrefs pref))
        {
            pref.ClearAll();
            return;
        }
        Debug.LogError(TypeIsNotSupported("LocalPrefs DeleteAll", t));
    }
    /// <summary>Find key in all preferences and change it.</summary>
    public static string ChangeKey(string oldKey, string newKey)
    {
        foreach (var pref in Data.prefs.Values)
            pref.ChangeKey(oldKey, newKey);
        return newKey;
    }
    /// <summary>Find key in given preference and change it.</summary>
    public static string ChangeKey<T>(string oldKey, string newKey)
    {
        Type t = typeof(T);
        if (Data.prefs.TryGetValue(t, out IPrefs pref))
        {
            return pref.ChangeKey(oldKey, newKey);
        }
        Debug.LogError(TypeIsNotSupported("LocalPrefs ChangeKey", t));
        return oldKey;
    }
    /// <summary>Find first key with given value and change it to new one.
    /// <para>This operation is much slower that changing by previous key. Use only if performance is not a consideration.</para></summary>
    public static string ChangeKey<T>(T value, string newKey)
    {
        Type t = typeof(T);
        if (Data.prefs.TryGetValue(t, out IPrefs pref))
        {
            string key = pref.KeyByValue(value);
            return pref.ChangeKey(key, newKey);
        }
        Debug.LogError(TypeIsNotSupported("LocalPrefs ChangeKey<T>", t));
        return default;
    }
    /// <summary>Look up for one key with given value and deletes it.<para />
    /// Returns true if key is found.<para />
    /// This operation is slow, don't use it constantly.</summary>
    public static bool DeleteKeyByValue<T>(T value)
    {
        Type t = typeof(T);
        if (Data.prefs.TryGetValue(t, out IPrefs pref))
        {
            string key = pref.KeyByValue(value);
            if (key != default)
                pref.DeleteKey(key);
            return key != default;
        }
        Debug.LogError(TypeIsNotSupported("LocalPrefs DeleteKeyByValue", t));
        return false;
    }
    /// <summary>Look up for keys with given value and delete them.<para /> 
    /// Returns true if at least one key is found.
    /// <para>This operation is slow, don't use it constantly.</para></summary>
    public static bool DeleteKeysByValue<T>(T value)
    {
        Type t = typeof(T);
        if (Data.prefs.TryGetValue(t, out IPrefs pref))
        {
            List<string> keys = pref.KeysByValue(value);
            pref.DeleteKeys(keys);
            return keys.Count > 0;
        }
        Debug.LogError(TypeIsNotSupported("LocalPrefs DeleteKeysByValue", t));
        return false;
    }
    /// <summary>Find value in this preference by key and set new value to it. 
    public static T Set<T>(string key, T value)
    {
        Type t = typeof(T);
        if (Data.prefs.TryGetValue(t, out IPrefs pref))
        {
            pref.SetPref(key, value);
            return value;
        }
        Debug.LogError("LocalPrefs: Trying to Set not supported type (" + t.Name + ")");
        return default;
    }
    /// <summary>Return value of key in this preference.
    public static T Get<T>(string key, T defaultValue = default)
    {
        Type t = typeof(T);
        if (Data.prefs.TryGetValue(t, out IPrefs pref))
        {
            return (T)pref.GetPref(key, defaultValue);
        }
        Debug.LogError("LocalPrefs: Trying to Get not supported type (" + t.Name + ")");
        return default;
    }
    /// <summary>Returns the count of keys presented in this preference.</summary>
    public static int KeysCount<T>()
    {
        Type t = typeof(T);
        if (Data.prefs.TryGetValue(t, out IPrefs pref))
        {
            return pref.Count;
        }
        Debug.LogError(TypeIsNotSupported("LocalPrefs KeysCount", t));
        return 0;
    }
    /// <summary>Returns every existing key in this preference. Consider to use it only once.</summary>
    public static string[] AllKeys<T>()
    {
        Type t = typeof(T);
        if (Data.prefs.TryGetValue(t, out IPrefs pref))
        {
            return pref.AllKeys(t);
        }
        Debug.LogError(TypeIsNotSupported("LocalPrefs GetAllKeys", t));
        return null;
    }
    /// <summary>Returns every existing value of this type. It's a very slow operation, consider to use it only once.</summary>
    public static List<T> AllValues<T>()
    {
        Type t = typeof(T);
        if (Data.prefs.TryGetValue(t, out IPrefs pref))
        {
            List<object> tempValues = pref.AllValues(t);
            List<T> allValues = new List<T>(tempValues.Count);
            for (int v = 0; v < tempValues.Count; v++)
                allValues.Add((T)tempValues[v]);
            return allValues;
        }
        Debug.LogError(TypeIsNotSupported("LocalPrefs GetAllKeys", t));
        return null;
    }
    static string TypeIsNotSupported(string methodName, Type t)
    {
        return methodName + ": Type \"" + t.Name + "\" is not supported.";
    }
}