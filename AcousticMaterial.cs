using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AcousticMaterial : MonoBehaviour
{

    [Range(0f, 1f)] public float LowFrequencyAttenuation;
    [Range(0f, 1f)] public float MidFrequencyAttenuation;
    [Range(0f, 1f)] public float HighFrequencyAttenuation;

    [Range(0f, 1f)] public float AmplitudeOcclusion;
    [Range(10f, 22000f)] public float LowPassOcclusion;

}