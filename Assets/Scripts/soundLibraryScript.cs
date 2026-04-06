using UnityEngine;

[System.Serializable]
public struct Sound
{
    public string soundName;
    public AudioClip audioClip;
}

[System.Serializable]
public struct SoundGroup
{
    public string groupName;
    public Sound[] sounds;
}

public class soundLibraryScript : MonoBehaviour
{
    public SoundGroup[] soundGroups;

    public AudioClip GetClipFromName(string soundName)
    {
        foreach (var group in soundGroups)
        {
            foreach (var sound in group.sounds)
            {
                if (sound.soundName == soundName)
                {
                    return sound.audioClip;
                }
            }
        }

        Debug.LogWarning($"Sound with name {soundName} not found in sound library.");
        return null;
    }

    public AudioClip GetClipFromGroup(string groupName, string soundName)
    {
        foreach (var group in soundGroups)
        {
            if (group.groupName == groupName)
            {
                foreach (var sound in group.sounds)
                {
                    if (sound.soundName == soundName)
                    {
                        return sound.audioClip;
                    }
                }
            }
        }

        Debug.LogWarning($"Sound {soundName} not found in group {groupName}.");
        return null;
    }
}