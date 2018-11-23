using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEditor.Presets;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioReverbZone))]
public class ReverbUnit : MonoBehaviour {

    private AudioReverbZone effect;
    //public Preset OutdoorReverb;

    public void Awake()
    {
        effect = GetComponent<AudioReverbZone>();
        StepOutside();
    }

    public void StepOutside()
    {
        // make an outdoors preset for the AudioReverb zone on the player to default to when not in a room.
       // OutdoorReverb.ApplyTo(effect);
    }
}
