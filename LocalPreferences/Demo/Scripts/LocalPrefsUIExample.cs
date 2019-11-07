using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Neonagee.Tests
{
    public class LocalPrefsUIExample : MonoBehaviour
    {
        [System.Serializable]
        public struct BoolUI
        {
            public string key;
            public bool value;
            public GameObject parent;
            public InputField input;
            public Toggle toggle;
            public Button removeButton;
        }
        [System.Serializable]
        public struct PrefsUI
        {
            public RectTransform content;
            public Button clearAllButton;
            public Button addButton;
        }
        public List<BoolUI> bools;

        public PrefsUI boolsUI;
        public GameObject boolPrefab;
        void Start()
        {
            boolsUI.clearAllButton.onClick.AddListener(() =>
            {
                for (int i = 0; i < bools.Count; i++)
                    DestroyBool(bools[i]);
            });
            boolsUI.addButton.onClick.AddListener(() =>
            {
                int keysCount = bools.Count;
                string newItemKey = "New " + "Boolean" + " ";
                while (LocalPrefs.HasKey<bool>(newItemKey + keysCount))
                    keysCount++;

                string newKey = newItemKey + keysCount;
                LocalPrefs.Set(newKey, false);
                InstatiateBoolUI(newKey);
            });

            string[] allBooleans = LocalPrefs.AllKeys<bool>();
            bools = new List<BoolUI>(allBooleans.Length);
            for (int i = 0; i < allBooleans.Length; i++)
                bools.Add(InstatiateBoolUI(allBooleans[i]));

            SetBoolListeners();
        }
        void SetBoolListeners()
        {
            for (int i = 0; i < bools.Count; i++)
            {
                int m_i = i;
                var boolUI = bools[m_i];

                bools[i].input.onEndEdit.RemoveAllListeners();
                bools[i].toggle.onValueChanged.RemoveAllListeners();
                bools[i].removeButton.onClick.RemoveAllListeners();

                // On Input field
                bools[i].input.onEndEdit.AddListener((s) =>
                {
                    if (!LocalPrefs.HasKey<bool>(s))
                    {
                        boolUI.key = LocalPrefs.ChangeKey<bool>(boolUI.key, s);
                        bools[m_i] = boolUI;
                    }
                    else if (s != boolUI.key)
                    {
                        bools[m_i].input.text = boolUI.key;
                        Debug.Log("Boolean with key \"" + s + "\" is already presented in preferences.");
                    }
                    SetBoolListeners();
                });
                // On Toggle
                bools[i].toggle.onValueChanged.AddListener((b) =>
                {
                    LocalPrefs.Set(boolUI.key, b);
                    SetBoolListeners();
                });
                // On Remove button
                bools[i].removeButton.onClick.AddListener(() =>
                {
                    DestroyBool(boolUI);
                    SetBoolListeners();
                });
            }
        }
        BoolUI InstatiateBoolUI(string key)
        {
            GameObject prefab = Instantiate(boolPrefab, boolsUI.content);
            var newBool = new BoolUI()
            {
                key = key,
                parent = prefab,
                input = prefab.GetComponentInChildren<InputField>(),
                toggle = prefab.GetComponentInChildren<Toggle>(),
                removeButton = prefab.GetComponentInChildren<Button>(),
            };
            newBool.input.text = newBool.key;
            newBool.toggle.isOn = LocalPrefs.Get(newBool.key, false);
            return newBool;
        }
        void DestroyBool(BoolUI boolUI)
        {
            LocalPrefs.DeleteKey<bool>(boolUI.key);
            Destroy(boolUI.parent);
            bools.Remove(boolUI);
        }

        void Update()
        {

        }
    }
}