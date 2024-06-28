using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class WeaponAssultRifle : WeaponBase
{
    [Header("Fire Effects")]
    [SerializeField]
    private GameObject muzzleFlashEffect;

    [Header("Spawn Points")]
    [SerializeField]
    private Transform casingSpawnPoint;
    [SerializeField]
    private Transform bulletSpawnPoint;


    [Header("Audio Clips")]
    [SerializeField]
    private AudioClip audioClipTakeOutWeapon;
    [SerializeField]
    private AudioClip audioClipFire;
    [SerializeField]
    private AudioClip audioClipReload;

    [Header("Aim UI")]
    [SerializeField]
    private Image imageAim;

    [Header("Recoil")]
    [SerializeField]
    private Transform camRecoil;
    [SerializeField]
    private Vector3 recoilKickback;
    [SerializeField]
    private float recoilAmount;

    private bool  isModeChange = false;
    private float defaultModeFOV = 60;
    private float aimModeFOV = 30;

    [SerializeField]
    private CasingMemoryPool casingMemoryPool;
    [SerializeField]
    private ImpactMemoryPool impactMemoryPool;
    private Camera mainCamera;

    public bool bIsClear = false;

    private void Awake()
    {
        base.Setup();
        mainCamera = Camera.main;

        weaponSetting.currentMagazine = weaponSetting.maxMagazine;

        weaponSetting.currentAmmo = weaponSetting.maxAmmo;

        ResetVariables();

    }

    private void OnEnable()
    {
        PlaySound(audioClipTakeOutWeapon);

        muzzleFlashEffect.SetActive(false);

        onMagazineEvent.Invoke(weaponSetting.currentMagazine);

        onAmmoEvent.Invoke(weaponSetting.currentAmmo, weaponSetting.maxAmmo);
    }

    private void Update()
    {
        RecoilBack();
    }
    public override void StartWeaponAction(int type = 0)
    {
        if (isReload == true) return;

        if (isModeChange == true) return;

        if(type == 0)
        {
            if(weaponSetting.isAutomaticAttack == true)
            {
                isAttack = true;
                StartCoroutine("OnAttackLoop");
            }
            else
            {
                OnAttack();
            }
        }
        else
        {
            if (isAttack == true) return;

            StartCoroutine("OnModeChange");
        }
    }

    public override void StartReload()
    {
        if (isReload == true || weaponSetting.currentMagazine <= 0) return;

        StopWeaponAction();

        StartCoroutine("OnReload");
    }

    private IEnumerator OnReload()
    {
        isReload = true;

        animator.OnReload();
        PlaySound(audioClipReload);

        while(true)
        {
            if(audioSource.isPlaying == false && animator.CurrentAnimationIs("Movement"))
            {
                isReload = false;

                weaponSetting.currentMagazine--;
                onMagazineEvent.Invoke(weaponSetting.currentMagazine);

                weaponSetting.currentAmmo = weaponSetting.maxAmmo;
                onAmmoEvent.Invoke(weaponSetting.currentAmmo, weaponSetting.maxAmmo);

                yield break;
            }

            yield return null;
        }
    }

    public override void StopWeaponAction(int type = 0)
    {
        if(type == 0)
        {
            isAttack = false;
            StopCoroutine("OnAttackLoop");
        }
    }

    private IEnumerator OnAttackLoop()
    {
        while(true)
        {
            if(bIsClear)
            {
                //Debug.Log("Attack" + bIsClear);
                StopCoroutine("OnAttackLoop");
            }

            //
            OnAttack();
            
            yield return null;
        }
    }

    public void OnAttack()
    {
        if(Time.time - lastAttackTime > weaponSetting.attackRate)
        {
            if(animator.MoveSpeed > 0.5f)
            {
                return;
            }

            lastAttackTime = Time.time;

            if(weaponSetting.currentAmmo <= 0)
            {
                return;
            }
            weaponSetting.currentAmmo--;

            onAmmoEvent.Invoke(weaponSetting.currentAmmo, weaponSetting.maxAmmo);

            //animator.Play("Fire", -1, 0);
            string animation = animator.AimModeIs == true ? "AimFire" : "Fire";
            animator.Play(animation, -1, 0);

            if (animator.AimModeIs == false) StartCoroutine("OnMuzzleFlashEffect");

            PlaySound(audioClipFire);

            casingMemoryPool.SpawnCasing(casingSpawnPoint.position, transform.right);

            TwoStepRaycast();

            Recoil();
        }
    }

    private void TwoStepRaycast()
    {
        Ray ray;
        RaycastHit hit;
        Vector3 targetPoint = Vector3.zero;

        ray = mainCamera.ViewportPointToRay(Vector2.one * 0.5f);

        if(Physics.Raycast(ray, out hit, weaponSetting.attackDistance))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.origin + ray.direction * weaponSetting.attackDistance;
        }
        Debug.DrawRay(ray.origin, ray.direction * weaponSetting.attackDistance, Color.red);

        Vector3 attackDirection = (targetPoint - bulletSpawnPoint.position).normalized;

        if(Physics.Raycast(bulletSpawnPoint.position, attackDirection, out hit, weaponSetting.attackDistance))
        {
            impactMemoryPool.SpawnImpact(hit);

            if(hit.transform.CompareTag("ImpactEnemy"))
            {
                if(hit.collider.transform.CompareTag("ImpactEnemyHead"))
                {
                    if(hit.transform.name == "Enemy(Clone)")
                    {
                        //Debug.Log("Head");
                        hit.transform.GetComponent<EnemyFSM>().TakeDamage(weaponSetting.damage * 5);
                    }
                    else if (hit.transform.name == "BoomEnemy(Clone)")
                    {
                        hit.transform.GetComponent<BoomEnemyFSM>().TakeDamage(weaponSetting.damage * 5);
                    }
                    else if (hit.transform.name == "BossEnemy")
                    {
                        //Debug.Log("BossEnemyFSM Head");
                        hit.transform.GetComponent<BossEnemyFSM>().TakeDamage(weaponSetting.damage * 5);
                    }
                }
                else
                {
                    if (hit.transform.name == "Enemy(Clone)")
                    {
                        //Debug.Log("EnemyFSM Body");
                        hit.transform.GetComponent<EnemyFSM>().TakeDamage(weaponSetting.damage);
                    }
                    else if(hit.transform.name == "BoomEnemy(Clone)")
                    {
                        //Debug.Log("BoomEnemyFSM Body");
                        hit.transform.GetComponent<BoomEnemyFSM>().TakeDamage(weaponSetting.damage);
                    }
                    else if(hit.transform.name == "BossEnemy")
                    {
                        //Debug.Log("BossEnemyFSM Body");
                        hit.transform.GetComponent<BossEnemyFSM>().TakeDamage(weaponSetting.damage);
                    }

                }
                
            }
            else if(hit.transform.CompareTag("InteractionObject"))
            {
                hit.transform.GetComponent<InteractionObject>().TakeDamage(weaponSetting.damage);
            }
        }

        Debug.DrawRay(bulletSpawnPoint.position, attackDirection * weaponSetting.attackDistance, Color.blue);
    }

    private IEnumerator OnMuzzleFlashEffect()
    {
        muzzleFlashEffect.SetActive(true);

        yield return new WaitForSeconds(weaponSetting.attackRate * 0.3f);

        muzzleFlashEffect.SetActive(false);
    }

    private IEnumerator OnModeChange()
    {
        float current = 0;
        float percent = 0;
        float time = 0.35f;

        animator.AimModeIs = !animator.AimModeIs;
        imageAim.enabled = !imageAim.enabled;

        float start = mainCamera.fieldOfView;
        float end = animator.AimModeIs == true ? aimModeFOV : defaultModeFOV;

        isModeChange = true;

        while(percent < 1)
        {
            current += Time.deltaTime;
            percent = current / time;

            mainCamera.fieldOfView = Mathf.Lerp(start, end, percent);

            yield return null;
        }

        isModeChange = false;
    }

    private void ResetVariables()
    {
        isReload = false;
        isAttack = false;
        isModeChange = false;
    }

    private void Recoil()
    {

        Vector3 recoilVector = new Vector3(Random.Range(-recoilKickback.x, recoilKickback.x), recoilKickback.y, recoilKickback.z);
        Vector3 recoilCamVector = new Vector3(-recoilVector.y * 400f, recoilVector.x * 200f, 0);
        
        camRecoil.localRotation = Quaternion.Slerp(camRecoil.localRotation, Quaternion.Euler(camRecoil.localEulerAngles + recoilCamVector), recoilAmount);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(transform.localEulerAngles + recoilCamVector), recoilAmount);

    }

    private void RecoilBack()
    {

        camRecoil.localRotation = Quaternion.Slerp(camRecoil.localRotation, Quaternion.identity, Time.deltaTime * 2f);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.identity, Time.deltaTime * 2f);
    }

    //public void IncreaseMagazine(int magazine)
    //{
    //    weaponSetting.currentMagazine = CurrentMagazine + magazine > MaxMagazine ? MaxMagazine : CurrentMagazine + magazine;
    //
    //    onMagazineEvent.Invoke(CurrentMagazine);
    //}
}
