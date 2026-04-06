using UnityEngine;


[System.Serializable]
public struct MusicTrack
{
    public string trackName;
    public AudioClip audioClip;
}

public class musicLibraryScript : MonoBehaviour
{

    public MusicTrack[] tracks;

    public AudioClip GetClipFromName(string trackName)
    {
        foreach (var track in tracks)
        {
            if (track.trackName == trackName)
            {
                return track.audioClip;
            }
        }
        Debug.LogWarning($"Track with name {trackName} not found in music library.");
        return null;
    }

}
