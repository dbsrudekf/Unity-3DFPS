using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Weapon : MonoBehaviour
{
    public string weaponName;
    public int bulletsPerMag;
    public int bulletsTotal;
    public int currentBullets;
    public float frange;
    public float fireRate;
    private bool isReloading;

    //정조준
    public Vector3 aimPosition;
    private Vector3 originalPosition;
    private bool isAiming;

    //반동
    public Transform camRecoil;
    public Vector3 recoilKickback;
    public float recoilAmount;


    private float fireTimer;

    public Transform shootPoint;
    private Animator animator;

    public GameObject muzzleFlash;
    public Transform ShootEfxPos;
    private AudioSource audioSource;
    public AudioClip ShotSound;
    public AudioClip ReloadSound;

    public Text bulletsText;




    // Start is called before the first frame update
    void Start()
    {
        currentBullets = bulletsPerMag;
        isReloading = false;
        bulletsText.text = currentBullets + " / " + bulletsTotal;
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();

        originalPosition = transform.localPosition;

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButton("Fire1"))
        {
            if (currentBullets > 0)
            {
                if (!animator.GetBool("Walk"))
                {
                    Fire();
                }

            }
            else
            {
                DoReload();
            }
        }

        if (fireTimer < fireRate)
        {
            fireTimer += Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            DoReload();
        }

        AimDownSights();
        //RecoilBack();
    }

    public void Fire()
    {

        if (fireTimer < fireRate)
        {
            animator.SetBool("Shot", false);
            return;
        }

        Debug.Log("Shot fire");

        animator.SetBool("Shot", true);
        RaycastHit hit;
        if (Physics.Raycast(shootPoint.position, shootPoint.transform.forward, out hit, frange))
        {
            Debug.Log("Hit!");
        }
        currentBullets--;
        fireTimer = 0.0f;

        audioSource.PlayOneShot(ShotSound);
        //animator.CrossFadeInFixedTime("ar1(automatic rifle)_hands_Fire_ar1(automatic rifle)", 0.01f);
        Instantiate(muzzleFlash, new Vector3(ShootEfxPos.position.x, ShootEfxPos.position.y, ShootEfxPos.position.z), Quaternion.identity);
        bulletsText.text = currentBullets + " / " + bulletsTotal;

        Recoil();

    }

    public void DoReload()
    {
        if(!isReloading && currentBullets < bulletsPerMag && bulletsTotal >0)
        {
            animator.CrossFadeInFixedTime ("ar1(automatic rifle)_hands_Reload_ar1(automatic rifle)", 0.01f);

            //isReloading = true;
            int bulletsToReload = bulletsPerMag - currentBullets;

            if (bulletsToReload > bulletsTotal)
            {
                bulletsToReload = bulletsTotal;
            }
            currentBullets += bulletsToReload;
            bulletsTotal -= bulletsToReload;
            bulletsText.text = currentBullets + " / " + bulletsTotal;

            audioSource.PlayOneShot(ReloadSound);

        }
        
    }

    public void AimDownSights()
    {
        if (Input.GetButton("Fire2") && !isReloading)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, aimPosition, Time.deltaTime * 8f);
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 40f, Time.deltaTime * 8f);
            isAiming = true;
        }
        else
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition, Time.deltaTime * 5f);
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 60f, Time.deltaTime * 8f);
            isAiming = false;
        }
    }
    public void Recoil()
    {
        Vector3 recoilVector = new Vector3(Random.Range(-recoilKickback.x, recoilKickback.x), recoilKickback.y, recoilKickback.z);
        Vector3 recoilCamVector = new Vector3(-recoilVector.y * 400f, recoilVector.x * 200f, 0);

        transform.localPosition = Vector3.Lerp(transform.localPosition, transform.localPosition + recoilVector, recoilAmount / 2f); // position recoil
        camRecoil.localRotation = Quaternion.Slerp(camRecoil.localRotation, Quaternion.Euler(camRecoil.localEulerAngles + recoilCamVector), recoilAmount);
    }
    public void RecoilBack()
    {
        camRecoil.localRotation = Quaternion.Slerp(camRecoil.localRotation, Quaternion.identity, Time.deltaTime * 2f);
    }
}
