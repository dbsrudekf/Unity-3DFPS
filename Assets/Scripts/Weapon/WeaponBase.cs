using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType { Main = 0, Sub, Melee, Throw }

[System.Serializable]
public class AmmoEvent : UnityEngine.Events.UnityEvent<int, int> { }
[System.Serializable]
public class MagazineEvent : UnityEngine.Events.UnityEvent<int> { }

public abstract class WeaponBase : MonoBehaviour
{
    [Header("WeaponBase")]
    [SerializeField]
    private WeaponType weaponType;
    [SerializeField]
    protected WeaponSetting weaponSetting;

    protected float lastAttackTime = 0;
    protected bool isReload = false;
    protected bool isAttack = false;
    protected AudioSource audioSource;
    protected PlayerAnimatorController animator;


    [HideInInspector]
    public AmmoEvent onAmmoEvent = new AmmoEvent();
    [HideInInspector]
    public MagazineEvent onMagazineEvent = new MagazineEvent();

    public PlayerAnimatorController Animator => animator;
    public WeaponName WeaponName => weaponSetting.weaponName;
    public int CurrentMagazine => weaponSetting.currentMagazine;
    public int MaxMagazine => weaponSetting.maxMagazine;

    public int CurrentAmmo => weaponSetting.currentAmmo;

    public void SetMagazine(int magazine)
    {
        weaponSetting.currentMagazine = magazine;
    }

    public void SetAmmo(int ammo)
    {
        weaponSetting.currentAmmo = ammo;
    }

    public abstract void StartWeaponAction(int type = 0);
    public abstract void StopWeaponAction(int type = 0);
    public abstract void StartReload();

    protected void PlaySound(AudioClip clip)
    {
        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }

    protected void Setup()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<PlayerAnimatorController>();
    }

    public virtual void IncreaseMagazine(int magazine)
    {
        weaponSetting.currentMagazine = CurrentMagazine + magazine > MaxMagazine ? MaxMagazine : CurrentMagazine + magazine;

        onMagazineEvent.Invoke(CurrentMagazine);
    }

}
