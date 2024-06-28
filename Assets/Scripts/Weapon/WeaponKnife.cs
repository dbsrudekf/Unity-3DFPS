using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class WeaponKnife : WeaponBase
{
    [SerializeField]
    private WeaponKnifeCollider weaponKnifeCollider;


    private void OnEnable()
    {
        isAttack = false;

        onMagazineEvent.Invoke(weaponSetting.currentMagazine);
        onAmmoEvent.Invoke(weaponSetting.currentAmmo, weaponSetting.maxAmmo);
    }

    private void Awake()
    {
        base.Setup();

        weaponSetting.currentMagazine = weaponSetting.maxMagazine;
        weaponSetting.currentAmmo = weaponSetting.maxAmmo;
    }
    public override void StartWeaponAction(int type = 0)
    {
        if (isAttack == true) return;

        if(weaponSetting.isAutomaticAttack == true)
        {
            StartCoroutine("OnAttackLoop", type);
        }
        else
        {
            StartCoroutine("OnAttack", type);
        }
    }
    public override void StopWeaponAction(int type = 0)
    {
        isAttack = false;
        StopCoroutine("OnAttackLoop");
    }

    public override void StartReload()
    {

    }

    private IEnumerator OnAttackLoop(int type)
    {
        while(true)
        {
            yield return StartCoroutine("OnAttack", type);
        }
    }

    private IEnumerator OnAttack(int type)
    {
        isAttack = true;

        animator.Setfloat("attackType", type);
        animator.Play("Fire", -1, 0);

        yield return new WaitForEndOfFrame();

        while(true)
        {
            if(animator.CurrentAnimationIs("Movement"))
            {
                isAttack = false;
                yield break;

            }

            yield return null;
        }
    }

    public void StartWeaponKnifeCollider()
    {
        weaponKnifeCollider.StartCollider(weaponSetting.damage);
    }
}
