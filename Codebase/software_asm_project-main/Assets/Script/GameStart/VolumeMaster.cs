using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;


public class VolumeMaster : MonoBehaviour
{

    public Slider master_vol, music_vol, sfx_vol;
    public AudioMixer MainMixer;

    

    public void ChangeMasterVolume()
    {
        MainMixer.SetFloat("MasterVol", master_vol.value);
    }

    public void ChangeMusicVolume()
    {
        MainMixer.SetFloat("MusicVol", music_vol.value);
    }

    public void ChangeSFXVolume()
    {
        MainMixer.SetFloat("SFXVol", sfx_vol.value);
    }
}
