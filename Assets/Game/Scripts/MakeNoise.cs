using UnityEngine;
using System.Collections;

public class MakeNoise : MonoBehaviour {

    AudioSource[] sources = new AudioSource[0];
    AudioTag[] audiotagss;
    readonly int sourceCount = 18;

    [SerializeField] float sountMult = 1;
    [SerializeField] AudioClips[] AudioList;
    
    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        sources = new AudioSource[sourceCount];
        audiotagss = new AudioTag[sourceCount];
        for (int i = 0; i < sources.Length; i++)
        {
            var clone = Instantiate(Resources.Load<GameObject>("AudioSource"), transform, true);
            clone.transform.localPosition = Vector3.zero;
            sources[i] = clone.GetComponent<AudioSource>();
            audiotagss[i] = sources[i].GetComponent<AudioTag>();
        }

        MusicOnOff((PlayerPrefs.GetInt("Music") == 1));
        SoundOnOff((PlayerPrefs.GetInt("Sound") == 1));

    }

    public void MusicOnOff(bool onOff)
    {
        transform.GetChild(0).gameObject.SetActive(onOff);
    }

    public void SoundOnOff(bool onOff)
    {
        for (int i = 0;i < sources.Length;i++)
        {
            sources[i].gameObject.SetActive(onOff);
        }
    }

    public void SetSoundVolume(float volume)
    {
        sountMult = volume;
    }

    public void PlaySFX(int id, float delay)
    {
        if(delay > 0)
            StartCoroutine(PlaySFXCor(id, delay));
        else
            PlayingSFX(id);
    }

    void PlayingSFX(int id)
    {
        foreach (AudioClips s in AudioList)
        {
            if (s.id == id)
            {
                AudioSource source = sources[0];
                for (int i = 0; i < sources.Length; i++)
                {
                    if (sources[i].isPlaying)
                    {
                        if (audiotagss[i]._id == id) // aynı tip ses zaten oynuyorsa uzerine yaz
                        {
                            source = sources[i];
                            source.Stop();
                            break;
                        }
                    }
                    else
                    {
                        source = sources[i];
                        audiotagss[i]._id = id;
                        break;
                    }
                }
                source.volume = s.volume;
                source.loop = s.loop;
                if (s.pitch >= .1f)
                    source.pitch = s.pitch;
                else
                    source.pitch = 1;
    
                int soundInd = 0;
                if (s.clip.Length > 1)
                {
                    soundInd = Random.Range(0, s.clip.Length);
                    while (soundInd == s.lastPlayed)
                    {
                        soundInd = Random.Range(0, s.clip.Length);
                    }
                    s.lastPlayed = soundInd;
                }

                source.volume *= sountMult;
                source.clip = s.clip[soundInd];
                source.Play();
            }
        }
    }

    IEnumerator PlaySFXCor(int id, float delay)
    {
        yield return new WaitForSeconds(delay);
        PlayingSFX(id);
    }

    void Update()
    {
        for (int i = 0; i < sources.Length; i++)
        {
            if (!sources[i].isPlaying)
                audiotagss[i]._id = -1;
        }
    }
}

[System.Serializable]
public class AudioClips
{
    public string name;
    public int id;
    public float volume;
    public float pitch;
    public bool loop;
    public int lastPlayed;
    public AudioClip[] clip;
}

