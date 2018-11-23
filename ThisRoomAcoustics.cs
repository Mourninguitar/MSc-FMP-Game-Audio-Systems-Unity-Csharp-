using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThisRoomAcoustics : MonoBehaviour {

    public bool debugMode;
    public AudioReverbPreset OutsidePreset;

    [HideInInspector] public int Room;
    [HideInInspector] public int RoomHF;
    [HideInInspector] public int RoomLF;
    [HideInInspector] public float DecayTime;
    [HideInInspector] public float DecayHfRatio;
    [HideInInspector] public int Reflections;
    [HideInInspector] public float ReflectionsDelay;
    [HideInInspector] public int Reverb;
    [HideInInspector] public float ReverbDelay;
    [HideInInspector] public float HfReference;
    [HideInInspector] public float LfReference;
    [HideInInspector] public float Diffusion;
    [HideInInspector] public float Density;

    private GameObject[] ComponentSabines;
    private Collider[] colliders1;
    public LayerMask whatToUpdateFor;
    private bool containsPlayer;
    public GameObject RoomMaster;
    private Transform player;
    private Collider roomSpaceCollider;
    private Transform[] roomSpace;
    public float roomVolume;
    private float roomSurfaceArea;
    // private float objectsSurfaceArea;        This variable is disabled since it currently is not assigned to anything, but is here as a reminder to myself to implement this at a later date

    [HideInInspector] [Range(-1f, 0f)] public float lowMean;
    [HideInInspector] [Range(-1f, 0f)] public float midMean;
    [HideInInspector] [Range(-1f, 0f)] public float highMean;

    private void Start()
    {
        roomSpaceCollider = GetComponent<Collider>();
        UpdateContainedObjects();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        RefreshAcoustics();
    }

    public void Update()
    {
        if (debugMode)
        {
            DebugExtension.DebugBounds(roomSpaceCollider.bounds, Time.deltaTime);
        }
    }

    public void RefreshAcoustics()
    {
        lowMean = 0f;
        midMean = 0f;
        highMean = 0f;

        ComponentSabines = new GameObject[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).GetComponent<AcousticMaterial>() != null)
            {
                ComponentSabines[i] = transform.GetChild(i).gameObject;

                lowMean += transform.GetChild(i).GetComponent<AcousticMaterial>().LowFrequencyAttenuation;
                midMean += transform.GetChild(i).GetComponent<AcousticMaterial>().MidFrequencyAttenuation;
                highMean += transform.GetChild(i).GetComponent<AcousticMaterial>().HighFrequencyAttenuation;
            }
        }

        lowMean /= transform.childCount * 2f;
        midMean /= transform.childCount * 2f;
        highMean /= transform.childCount * 2f;

        float scaleX = RoomMaster.transform.localScale.x;
        float scaleY = RoomMaster.transform.localScale.y;
        float scaleZ = RoomMaster.transform.localScale.z;

        roomVolume = Mathf.Abs(RoomMaster.transform.localScale.x * RoomMaster.transform.localScale.y * RoomMaster.transform.localScale.z);
        roomSurfaceArea = Mathf.Abs((2 * scaleX * scaleY) + (2 * scaleY * scaleZ) + (2 * scaleZ * scaleX));

        Room = (int)(midMean/(midMean + 3f) * -10000f);
        RoomLF = (int)(lowMean/(lowMean + 3f) * -10000f);
        RoomHF = (int)(highMean/(highMean + 3f) * -10000f);
        DecayTime = (0.161f * 3f * roomVolume) / (((lowMean + midMean + highMean) / 3f) * roomSurfaceArea);
        DecayHfRatio = 1f - (0.33f * highMean);
        Reflections = (int)(-2000f - (roomVolume / (roomVolume + 600f)));
        ReflectionsDelay = (Mathf.Min(Mathf.Min(scaleX, scaleY), scaleZ) / 343f);
        Reverb = (int)(-2000f - (roomVolume / (roomVolume + 300f)));
        ReverbDelay = (((scaleX + scaleY + scaleZ) / 3) / 343f); ;
        HfReference = (1500f);
        LfReference = (375f);
        Diffusion = Mathf.Clamp((1f - ((roomSurfaceArea / roomVolume) / 6f)) * 100f, 0f, 100f);
        Density = 400f * (ReverbDelay - ReflectionsDelay);
    }

    private void UpdateContainedObjects()
    {
        colliders1 = Physics.OverlapBox(roomSpaceCollider.transform.position, roomSpaceCollider.transform.lossyScale + new Vector3(0.1f, 0.1f, 0.1f), roomSpaceCollider.transform.rotation, whatToUpdateFor, QueryTriggerInteraction.Collide);
        player = GameObject.Find("Player").transform;
        if (containsPlayer)
        {
            for (int i = 0; i < colliders1.Length; i++)
            {
                if (colliders1[i].gameObject.GetComponent<AudioSource>() != null)
                {
                    colliders1[i].gameObject.GetComponent<AudioSource>().reverbZoneMix = 1;
                }
                
            }
        }
        else
        {
            for (int i = 0; i < colliders1.Length; i++)
            {
                if (colliders1[i].gameObject.GetComponent<AudioSource>() != null)
                {
                    colliders1[i].gameObject.GetComponent<AudioSource>().reverbZoneMix = 0;
                }

            }
        }

        // The following is incomplete, please see my write-up for an explanation of what I would like to do here
        
        // objectsSurfaceArea = 0f;
        // for (int i = 0; i < colliders1.Length; i++)
        // {
        //      This is where code to figure out the surface area of contained objects would go
        //      objectSurfaceArea += (result of line 136);
        // }

        // end of incomplete code

        RefreshAcoustics();
    }

    private void OnTriggerEnter (Collider other)
    {
        player.gameObject.GetComponentInChildren<AudioReverbZone>().reverbPreset = AudioReverbPreset.User;
        if (other.gameObject.tag == "Player")
        {
            containsPlayer = true;
        }
        UpdateContainedObjects();
        RefreshAcoustics();
        if (other.gameObject.tag == "Player" && other.gameObject.GetComponentInChildren<ReverbUnit>() != null)
        {
            AudioReverbZone PlayerReverb = other.gameObject.GetComponentInChildren<AudioReverbZone>();

            PlayerReverb.room = Room;
            PlayerReverb.roomHF= RoomHF;
            PlayerReverb.roomLF= RoomLF;
            PlayerReverb.decayTime = DecayTime;
            PlayerReverb.decayHFRatio = DecayHfRatio;
            PlayerReverb.reflections = Reflections;
            PlayerReverb.reflectionsDelay = ReflectionsDelay;
            PlayerReverb.reverb = Reverb;
            PlayerReverb.reverbDelay = ReverbDelay;
            PlayerReverb.HFReference = HfReference;
            PlayerReverb.LFReference = LfReference;
            PlayerReverb.diffusion = Diffusion;
            PlayerReverb.density = Density;

        }
        else
        {
            return;
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            containsPlayer = false;
            other.GetComponentInChildren<ReverbUnit>().StepOutside();
        }
        UpdateContainedObjects();
    }
}
