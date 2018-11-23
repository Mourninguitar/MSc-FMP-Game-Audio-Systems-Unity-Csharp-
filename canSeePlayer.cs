using UnityEngine;
using UnityEngine.Audio;

public class canSeePlayer : MonoBehaviour
{
    public Transform targetTransform;
    
    public bool LineOfSight;

    [HideInInspector] public Vector3 direction;

    public float LpfFilter;
    public float AmpFilter;

    [HideInInspector] public RaycastHit hit;
    [HideInInspector] public float distance;

    [HideInInspector] public float occlusionLpf;
    [HideInInspector] public float attenuation;

    private void Awake()
    {
        
    }

    void Update()
    {
        direction = targetTransform.position - transform.position;
        if (Physics.Raycast(transform.position, direction, out hit))
        {
            if (hit.transform.tag == "Player")
            {
                LineOfSight = true;
                Debug.DrawRay(transform.position, direction, Color.green);
                attenuation = 1f;
                occlusionLpf = 22000f;
            }
            else
            {
                Debug.DrawRay(transform.position, direction, Color.red);
                LineOfSight = false;
                if (hit.transform.gameObject.GetComponent<AcousticMaterial>())
                {
                    attenuation = hit.transform.gameObject.GetComponent<AcousticMaterial>().AmplitudeOcclusion;
                    occlusionLpf = hit.transform.gameObject.GetComponent<AcousticMaterial>().LowPassOcclusion ;
                }
            }
        }
        else
        {
            Debug.Log("ERROR: Player not found");
            attenuation = 1f - (Mathf.Log10(distance + 1));
            occlusionLpf = 22000f;
        }

        AmpFilter = attenuation;
        LpfFilter = occlusionLpf * 22000f;
    }
}