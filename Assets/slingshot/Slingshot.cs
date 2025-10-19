using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slingshot : MonoBehaviour
{
    [Header("Inscribed")]
    public GameObject projectilePrefab;
    public GameObject heavyProjectilePrefab;
    public GameObject projLinePrefab;
    public float velocityMult = 5f;

    [Header("BandSettings")]
    public LineRenderer bandL;
    public LineRenderer bandR;
    public Vector3 leftFork;
    public Vector3 rightFork;

    [Header("Dynamic")]
    public GameObject launchPoint;
    public Vector3 launchPos;
    public GameObject projectile;
    public bool aimingMode;

    [Header("VSfx")]
    private bool _useHeavy = true;
    public AudioSource audioSource;
    public AudioClip snapSound;

    void Awake()
    {
        Transform launchPointTrans = transform.Find("LaunchPoint");
        launchPoint = launchPointTrans.gameObject;
        launchPoint.SetActive(false);
        launchPos = launchPointTrans.position;

        InitBand(bandL);
        InitBand(bandR);
    }
    void InitBand(LineRenderer lr)
    {
        if(!lr) return;
        lr.useWorldSpace = true;
        lr.positionCount = 2;
        lr.enabled = false;

    }
    void OnMouseEnter()
    {
        launchPoint.SetActive(true);
    }

    void OnMouseExit()
    {
        launchPoint.SetActive(false);
    }

    void OnMouseDown()
    {
        aimingMode = true;
        GameObject prefabToUse;
        if (_useHeavy)
        {
            prefabToUse = heavyProjectilePrefab;
            _useHeavy = false;
        }
        else
        {
            prefabToUse = projectilePrefab;
            _useHeavy = true;
        }        
        projectile = Instantiate(prefabToUse);
        projectile.transform.position = launchPos;
        projectile.GetComponent<Rigidbody>().isKinematic = true;

        if (bandL)
        {
            bandL.enabled = true;
            bandL.SetPosition(0, leftFork);
            bandL.SetPosition(1, launchPos);
        }
        if (bandR)
        {
            bandR.enabled = true;
            bandR.SetPosition(0, rightFork);
            bandR.SetPosition(1, launchPos);
        }
    }

    void Update ()
    {
        if (!aimingMode) return;
        Vector3 mousePos2D = Input.mousePosition;
        mousePos2D.z = -Camera.main.transform.position.z;
        Vector3 mousePos3D = Camera.main.ScreenToWorldPoint(mousePos2D);
        Vector3 mouseDelta = mousePos3D - launchPos;
        float maxMagnitude = this.GetComponent<SphereCollider>().radius;

        if (mouseDelta.magnitude > maxMagnitude)
        {
            mouseDelta.Normalize();
            mouseDelta *= maxMagnitude;
        }

        Vector3 projPos = launchPos + mouseDelta;
        projectile.transform.position = projPos;

        if (bandL) bandL.SetPosition(1, projPos);
        if (bandR) bandR.SetPosition(1, projPos);
        
        if (Input.GetMouseButtonUp(0))
        {
            aimingMode = false;
            Rigidbody projRB = projectile.GetComponent<Rigidbody>();
            projRB.isKinematic = false;
            projRB.collisionDetectionMode = CollisionDetectionMode.Continuous;
            projRB.velocity = -mouseDelta * velocityMult;
            FollowCam.SWITCH_VIEW(FollowCam.eView.slingshot);
            FollowCam.POI = projectile;
            Instantiate<GameObject>(projLinePrefab, projectile.transform);
            projectile = null;
            MissionDemolition.SHOT_FIRED();

            if (audioSource && snapSound) audioSource.PlayOneShot(snapSound);
    
        }
        
    }

}
