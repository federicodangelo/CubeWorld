using System.Collections.Generic;
using CubeWorld.World;
using UnityEngine;
using CubeWorld.Tiles;
using CubeWorld.World.Objects;
using CubeWorld.Items;

public class CWFxManagerUnity : ICWFxListener
{
    private GameObject goContainer;
    private GameManagerUnity gameManagerUnity;
    private Dictionary<string, AudioClip[]> sounds = new Dictionary<string, AudioClip[]>();
    private Dictionary<string, GameObject> effects = new Dictionary<string, GameObject>();
    private Dictionary<string, System.Type> effectsComponents = new Dictionary<string, System.Type>();

    public CWFxManagerUnity(GameManagerUnity gameManagerUnity)
    {
        this.gameManagerUnity = gameManagerUnity;

        goContainer = new GameObject();
        goContainer.name = "FxContainer";

        effects["explosion"] = Resources.Load("Effects/FxExplosion", typeof(GameObject)) as GameObject;
        sounds["explosion"] = new AudioClip[] { Resources.Load("Effects/SoundExplosion", typeof(AudioClip)) as AudioClip };
        sounds["hitmetal"] = new AudioClip[] { Resources.Load("Effects/SoundHitMetal", typeof(AudioClip)) as AudioClip, Resources.Load("Effects/SoundHitMetal2", typeof(AudioClip)) as AudioClip };
        sounds["hit"] = new AudioClip[] { Resources.Load("Effects/SoundHit", typeof(AudioClip)) as AudioClip };
        effectsComponents["vibration"] = typeof(FxVibration);
    }

    private AudioClip ChooseRandom(AudioClip[] clips)
    {
        return clips[Random.Range(0, clips.Length)];
    }

    public void PlaySound(string soundId, CubeWorld.Utils.Vector3 position)
    {
        if (sounds.ContainsKey(soundId))
            PlayAudioClip(soundId, GraphicsUnity.CubeWorldVector3ToVector3(position), 1.0f);
        else
            Debug.Log("Unknown sound: " + soundId);
    }

    public void PlaySound(string soundId, CWObject fromObject)
    {
        if (sounds.ContainsKey(soundId))
            PlayAudioClip(soundId, GraphicsUnity.CubeWorldVector3ToVector3(fromObject.position), 1.0f);
        else
            Debug.Log("Unknown sound: " + soundId);
    }

    private Dictionary<string, List<AudioSource>> activeAudioSources = new Dictionary<string, List<AudioSource>>();

    private AudioSource PlayAudioClip(string id, Vector3 position, float volume)
    {
        AudioClip clip = ChooseRandom(sounds[id]);

        if (activeAudioSources.ContainsKey(id) == false)
            activeAudioSources[id] = new List<AudioSource>();

        AudioSource freeAudioSource = null;
        foreach (AudioSource audio in activeAudioSources[id])
        {
            if (audio.isPlaying == false)
            {
                freeAudioSource = audio;
                break;
            }
        }

        if (freeAudioSource == null && activeAudioSources[id].Count < 2)
        {
            GameObject go = new GameObject("Audio Source #" + activeAudioSources[id].Count + " for " + id);
            go.transform.parent = goContainer.transform;
            freeAudioSource = go.AddComponent<AudioSource>();
            activeAudioSources[id].Add(freeAudioSource);
        }

        if (freeAudioSource != null)
        {
            freeAudioSource.clip = clip;
            freeAudioSource.gameObject.transform.position = position;
            freeAudioSource.volume = volume;
            freeAudioSource.Play();
        }

        //GameObject.Destroy(go, clip.length);
        return freeAudioSource;
    }

    public void PlayEffect(string effectId, CubeWorld.Utils.Vector3 position)
    {
        if (effects.ContainsKey(effectId))
            ((GameObject) GameObject.Instantiate(effects[effectId], GraphicsUnity.CubeWorldVector3ToVector3(position), Quaternion.identity)).transform.parent = goContainer.transform;
        else
            Debug.Log("Unknown effect: " + effectId);
    }

    public void PlayEffect(string effectId, CWObject fromObject)
    {
        if (effects.ContainsKey(effectId))
        {
            ((GameObject) GameObject.Instantiate(effects[effectId], GraphicsUnity.CubeWorldVector3ToVector3(fromObject.position), Quaternion.identity)).transform.parent = goContainer.transform;
        }
        else if (effectsComponents.ContainsKey(effectId))
        {
            GameObject go = gameManagerUnity.objectsManagerUnity.FindGameObject(fromObject);

            if (go)
                go.AddComponent(effectsComponents[effectId]);
            else
                Debug.Log("Effect " + effectId + " found but GameObject to add sound to not found");
        }
        else
            Debug.Log("Unknown effect: " + effectId);
    }
}

