using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPrefsStressTest : MonoBehaviour
{
    public int randomPrefsCount = 1000;
    public float randomTime = 2;
    float m_randomTime;
    public string stressTestFileName = "PrefsStressTest";
    public bool encrypt = true;
    public bool testPlayerPrefs = true;
    public float[] localPrefs;
    public float[] playerPrefs;

    void Start()
    {
        // Local prefs load
        DateTime before = DateTime.Now;
        LocalPrefs.Load(stressTestFileName, encrypt);
        DateTime after = DateTime.Now;
        TimeSpan duration = after.Subtract(before);
        Debug.Log("LocalPrefs Load time: " + duration.TotalMilliseconds + " ms");

        // Local prefs get floats
        before = DateTime.Now;
        int prefsCount = LocalPrefs.Get<int>("RandomPrefsCount");
        localPrefs = new float[prefsCount];
        for (int i = 0; i < prefsCount; i++)
        {
            localPrefs[i] = LocalPrefs.Get<float>("RandomFloat" + i);
        }
        after = DateTime.Now;
        duration = after.Subtract(before);
        Debug.Log("LocalPrefs GetFloat: " + duration.TotalMilliseconds + " ms");

        if (testPlayerPrefs) 
        {
            // Player prefs get floats
            before = DateTime.Now;
            LocalPrefs.Load(stressTestFileName);
            prefsCount = PlayerPrefs.GetInt("RandomPrefsCount");
            localPrefs = new float[prefsCount];
            for (int i = 0; i < prefsCount; i++)
            {
                localPrefs[i] = PlayerPrefs.GetFloat("RandomFloat" + i);
            }
            after = DateTime.Now;
            duration = after.Subtract(before);
            Debug.Log("PlayerPrefs GetFloat: " + duration.TotalMilliseconds + " ms");
        }
        Application.wantsToQuit += Save;
        m_randomTime = Time.time + randomTime;
    }
    bool Save()
    {
        DateTime before = DateTime.Now;
        LocalPrefs.Save(stressTestFileName, encrypt);
        DateTime after = DateTime.Now;
        TimeSpan duration = after.Subtract(before);
        Debug.Log("LocalPrefs Save time: " + duration.TotalMilliseconds + " ms");
        return true;
    }

    void FixedUpdate()
    {
        if (Time.time >= m_randomTime)
        {
            // Local prefs test
            DateTime before = DateTime.Now;
            RandomLocalPrefs();
            DateTime after = DateTime.Now;
            TimeSpan duration = after.Subtract(before);
            Debug.Log("LocalPrefs SetFloat: " + duration.TotalMilliseconds + " ms");

            if (testPlayerPrefs)
            {
                // Player prefs test
                before = DateTime.Now;
                RandomPlayerPrefs();
                after = DateTime.Now;
                duration = after.Subtract(before);
                Debug.Log("PlayerPrefs SetFloat: " + duration.TotalMilliseconds + " ms");
            }

            m_randomTime = Time.time + randomTime;
        }
    }
    void RandomLocalPrefs()
    {
        localPrefs = new float[randomPrefsCount];
        LocalPrefs.SetInt("RandomPrefsCount", randomPrefsCount);
        for (int i = 0; i < randomPrefsCount; i++)
        {
            localPrefs[i] = LocalPrefs.Set<float>("RandomFloat" + i, UnityEngine.Random.Range(0, 100000));
        }
    }
    void RandomPlayerPrefs()
    {
        playerPrefs = new float[randomPrefsCount];
        PlayerPrefs.SetInt("RandomPrefsCount", randomPrefsCount);
        for (int i = 0; i < randomPrefsCount; i++)
        {
            float random = UnityEngine.Random.Range(0, 100000);
            PlayerPrefs.SetFloat("RandomFloat" + i, random);
            playerPrefs[i] = random;
        }
    }
}
