using UnityEngine;

namespace Neonagee.LocalPreferences
{
    // Need to create subclasses for generic types to serialize them
    [System.Serializable] public class PrefsBool : Prefs<bool> { }
    [System.Serializable] public class PrefsInt : Prefs<int> { }
    [System.Serializable] public class PrefsFloat : Prefs<float> { }
    [System.Serializable] public class PrefsVector2 : Prefs<Vector2> { }
    [System.Serializable] public class PrefsVector3 : Prefs<Vector3> { }
    [System.Serializable] public class PrefsVector4 : Prefs<Vector4> { }
    [System.Serializable] public class PrefsString : Prefs<string> { }
}
