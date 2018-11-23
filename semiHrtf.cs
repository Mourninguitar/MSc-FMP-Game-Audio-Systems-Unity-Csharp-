using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;


// Post-Mortem Notes:
// This script was concerned with approximating HRTF and delaying sounds depending on how far they were being emitted from compared to the listener.
// Looking back I could have abstracted many parts of this code into separate funtions and called them from Update()
// There are far too many references, 

public class semiHrtf : MonoBehaviour {

    public bool usePathLength;
    [Range(0.001f, 5f)] public float DistanceScaling;
    public AudioSource LeftAudioSource, RightAudioSource;
    public Transform LeftEar, RightEar;
    public AudioSystemManager audioSystemManager;

    [HideInInspector] public float StereoWidth;
    [HideInInspector] public Transform target, Parent;
    [HideInInspector] public DangerPathFinder ParentPathFinder; // This was a custom script that could be referenced in the Unity Engine
    [HideInInspector] public AudioLowPassFilter leftLpfFilter, rightLpfFilter;
    [HideInInspector] public AudioHighPassFilter leftHpfFilter, rightHpfFilter;
    [HideInInspector] public AudioEchoFilter leftAmpFilter, rightAmpFilter;

    [HideInInspector] public canSeePlayer los;
    [HideInInspector] public float conicalFiltering;
    [HideInInspector] public float leftDelay, rightDelay;
    [HideInInspector] public float delayDiff;
    [HideInInspector] public float distance;
    [HideInInspector] public float AmplitudeOverDistanceSqrd;
    [HideInInspector] public float distanceHpf;
    
    void Awake()
    {
        target = GetComponentInParent<canSeePlayer>().targetTransform;
        Parent = GetComponentInParent<Transform>();
        ParentPathFinder = GetComponentInParent<DangerPathFinder>();
        los = GetComponentInParent<canSeePlayer>();


        leftLpfFilter = LeftAudioSource.GetComponent<AudioLowPassFilter>();
        rightLpfFilter = RightAudioSource.GetComponent<AudioLowPassFilter>();
        leftHpfFilter = LeftAudioSource.GetComponent<AudioHighPassFilter>();
        rightHpfFilter = RightAudioSource.GetComponent<AudioHighPassFilter>();
        leftAmpFilter = LeftAudioSource.GetComponent<AudioEchoFilter>();
        rightAmpFilter = RightAudioSource.GetComponent<AudioEchoFilter>();
        StereoWidth = audioSystemManager.StereoWidth;
    }

    void Update () {

        if (usePathLength)
        {
            distance = ParentPathFinder.pathLength;
        }
        else if (!usePathLength)
        {
            distance = Vector3.Magnitude(target.position - Parent.position);
        }


        leftDelay = Vector3.Magnitude(LeftEar.position - Parent.position);
        rightDelay = Vector3.Magnitude(RightEar.position - Parent.position);
        delayDiff = (rightDelay - leftDelay) * StereoWidth;

        // This used a built-in delay effect in Unity to acheive the Haas effect.
        AmplitudeOverDistanceSqrd = 1 - (distance / (distance + DistanceScaling));
        leftAmpFilter.delay = leftDelay;
        rightAmpFilter.delay = rightDelay;


        // Conical Filtering
        {
            // This calculated the rotation of the player compared to the emitter regardless of world position.

            Vector2 PlayerPosition = new Vector2(target.position.x, target.position.z);
            Vector2 SourcePosition = new Vector2(transform.position.x, transform.position.z);
            Vector3 DirectiontoSource = new Vector3(SourcePosition.x - PlayerPosition.x, 0f, SourcePosition.y - PlayerPosition.y);

            Debug.DrawRay(target.position, target.forward * 100f, Color.green);
            Debug.DrawRay(target.position, DirectiontoSource * 100f, Color.red);

            float RelativeHeadAngle = Vector3.Angle(target.forward, DirectiontoSource);
            if(RelativeHeadAngle < 55f)
            {
                conicalFiltering = 1f;
            }
            else if (RelativeHeadAngle < 120f)
            {
                conicalFiltering = 0.5f;
            }
            else if (RelativeHeadAngle >= 120f)
            {
                conicalFiltering = 0.33f;
            }
            else
            {
                Debug.LogWarning("ERROR when calculating conicalFiltering");
            }
 
            leftLpfFilter.cutoffFrequency  = ((los.occlusionLpf / (los.occlusionLpf + (8800f * conicalFiltering))) * conicalFiltering * 22000f) * (1f + delayDiff);
            rightLpfFilter.cutoffFrequency = ((los.occlusionLpf / (los.occlusionLpf + (8800f * conicalFiltering))) * conicalFiltering * 22000f) / (1f + delayDiff);

            leftAmpFilter.wetMix = (AmplitudeOverDistanceSqrd * los.AmpFilter) * (1f + delayDiff);
            rightAmpFilter.wetMix = (AmplitudeOverDistanceSqrd * los.AmpFilter) / (1f + delayDiff);

            distanceHpf = 5f / AmplitudeOverDistanceSqrd;
            leftHpfFilter.cutoffFrequency = distanceHpf;
            rightHpfFilter.cutoffFrequency = distanceHpf;
        }
    }
}
