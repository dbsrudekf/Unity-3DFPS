using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Input KeyCode")]
    [SerializeField]
    private KeyCode keyCodeRun = KeyCode.LeftShift;
    [SerializeField]
    private KeyCode keyCodeJump = KeyCode.Space;
    [SerializeField]
    private KeyCode keyCodeReload = KeyCode.R;

    [Header("Audio Clips")]
    [SerializeField]
    private AudioClip audioClipWalk;
    [SerializeField]
    private AudioClip audioClipRun;

    public bool bIsSetUI;
    public bool bIsStartKey;

    private RotateToMouse rotateToMouse;
    private MovementCharacterController movement;
    private Status status;

    private AudioSource audioSource;
    private WeaponBase weapon;



    private void Awake()
    {

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        bIsSetUI = false;
        bIsStartKey = false;

        rotateToMouse = GetComponent<RotateToMouse>();
        movement = GetComponent<MovementCharacterController>();
        status = GetComponent<Status>();
        audioSource = GetComponent<AudioSource>();


    }


    private void Update()
    {   
        if(!bIsSetUI)
        {
            UpdateRotate();
            UpdateMove();
            UpdateJump();
            UpdateWeaponAction();
        }

    }

    private void UpdateRotate()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");


        rotateToMouse.UpdateRotate(mouseX, mouseY);
        
    }

    private void UpdateMove()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        if(x != 0 || z != 0)
        {
            bool isRun = false;

            if (z > 0) isRun = Input.GetKey(keyCodeRun);

            movement.MoveSpeed = isRun == true ? status.RunSpeed : status.WalkSpeed;
            weapon.Animator.MoveSpeed = isRun == true ? 1 : 0.5f; //애니메이션 발동
            audioSource.clip = isRun == true ? audioClipRun : audioClipWalk;

            if(audioSource.isPlaying == false)
            {
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        else
        {
            movement.MoveSpeed = 0;
            weapon.Animator.MoveSpeed = 0;

            if(audioSource.isPlaying == true)
            {
                audioSource.Stop();
            }
        }

        movement.MoveTo(new Vector3(x, 0, z));
    }

    private void UpdateJump()
    {
        if(Input.GetKeyDown(keyCodeJump))
        {
            movement.Jump();
        }
    }

    private void UpdateWeaponAction()
    {
        if(Input.GetMouseButtonDown(0))
        {
            weapon.StartWeaponAction();
        }
        else if(Input.GetMouseButtonUp(0))
        {
            weapon.StopWeaponAction();
        }

        if(Input.GetMouseButtonDown(1))
        {
            weapon.StartWeaponAction(1);
        }
        else if(Input.GetMouseButtonUp(1))
        {
            weapon.StopWeaponAction(1);
        }

        if(Input.GetKeyDown(keyCodeReload))
        {
            weapon.StartReload();
        }
    }

    public void TakeDamage(int damage)
    {
        bool isDie = status.DecreaseHP(damage);

        if(isDie == true)
        {
            Debug.Log("GameOver");
        }
    }

    public void IncreaseGold(int gold)
    {
        status.IncreaseGold(gold);
    }

    public void SwitchingWeapon(WeaponBase newWeapon)
    {
        weapon = newWeapon;
    }

}
