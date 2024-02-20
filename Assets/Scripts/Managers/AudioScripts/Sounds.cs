using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sounds : MonoBehaviour
{
    public SoundGroup[] soundGroups;
    Dictionary<string, AudioClip[]> SoundGroupDictionary = new Dictionary<string, AudioClip[]>();

    private void Awake()
    {
        foreach (SoundGroup sound in soundGroups)
        {
            SoundGroupDictionary.Add(sound.groupName, sound.groupClips);
        }
    }

    public AudioClip GetAudioFromName(string name)
    {
        if (SoundGroupDictionary.ContainsKey(name))
        {
            AudioClip[] sounds = SoundGroupDictionary[name];
            return sounds[Random.Range(0, sounds.Length)];
        }
        return null;
    }

    #region Sound Group Class
    [System.Serializable]
    public class SoundGroup
    {
        public string groupName;
        public AudioClip[] groupClips;
    }
    #endregion
}
