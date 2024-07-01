using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using JetBrains.Annotations;
using System.Threading;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class PlayerHUD : MonoBehaviour, ISaveable
{
    [Header("Input KeyCode")]
    [SerializeField]
    private KeyCode keyCodeMenu = KeyCode.Escape;
    private KeyCode keyCodeEnter = KeyCode.Return;
    private WeaponBase weapon;
    [SerializeField]
    private WeaponBase[] playerWeapons;

    private const string defaultSaveFile = "save";


    [Header("Components")]
    [SerializeField]
    private Status status;
    private PlayerController PlayerCtrl;
    private WeaponSwitchSystem weaponSwitchSystem;

    [Header("Weapon Base")]
    [SerializeField]
    private TextMeshProUGUI textWeaponName;
    [SerializeField]
    private Image imageWeaponIcon;
    [SerializeField]
    private Sprite[] spriteWeaponIcons;
    [SerializeField]
    private Vector2[] sizeWeaponIcons;

    [Header("Ammo")]
    [SerializeField]
    private TextMeshProUGUI textAmmo;

    [Header("Magazine")]
    [SerializeField]
    private GameObject magazineUIPrefab;
    [SerializeField]
    private Transform magazineParent;
    [SerializeField]
    private int maxMagazineCount;
    private int saveCurrentMagazine;

    [Header("HP & BloodScreen UI")]
    [SerializeField]
    private TextMeshProUGUI textHP;
    [SerializeField]
    private Image imageBloodScreen;
    [SerializeField]
    private AnimationCurve curveBloodScreen;

    [Header("Score")]
    [SerializeField]
    private TextMeshProUGUI textScore;

    [Header("Menu")]
    [SerializeField]
    private GameObject MenuUI;
    private AudioSource PlayerAudioSource;

    [Header("SHOP")]
    [SerializeField]
    private GameObject ShopUI;

    [Header("Gold")]
    [SerializeField]
    private TextMeshProUGUI textGold;

    [Header("Mission")]
    [SerializeField]
    private GameObject MissionUI;
    [SerializeField]
    private int ClearScore;
    private bool bIsMission;

    [Header("MemoryPool")]
    [SerializeField]
    private CasingMemoryPool casingMemoryPool;
    [SerializeField]
    private ImpactMemoryPool impactMemoryPool;

    [Header("BossKey")]
    [SerializeField]
    private GameObject BossKeyUI;

    private List<GameObject> magazineList;
    Dictionary<string, object> data = new Dictionary<string, object>();

    private void Awake()
    {
        PlayerCtrl = GameObject.Find("Player").GetComponent<PlayerController>();
        PlayerAudioSource = PlayerCtrl.GetComponent<AudioSource>();
        weaponSwitchSystem = GameObject.Find("Player").GetComponent<WeaponSwitchSystem>();
        status.onHPEvent.AddListener(UpdateHPHUD);
        status.onGoldEvent.AddListener(UpdateGoldHUD);
        status.onScoreEvent.AddListener(UpdateScoreHUD);

        playerWeapons = weaponSwitchSystem.GetWeapon();

        UpdateScoreHUD(status.GetScore());
        bIsMission = false;
        PlayerCtrl.bIsSetUI = false;
    }

    private void Start()
    {
        SavingSystem saving = FindObjectOfType<SavingSystem>();
        saving.Load(defaultSaveFile);
    }

    private void Update()
    {
        UIActive();
        KeyInput();
    }

    private void UIActive()
    {
        if(Input.GetKeyDown(keyCodeMenu))
        {
            if(MenuUI.activeSelf == false && ShopUI.activeSelf == false)
            {
                MenuUI.SetActive(true);
                Time.timeScale = 0;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                PlayerCtrl.bIsSetUI = true;
                PlayerAudioSource.Stop();


            }
            else if(MenuUI.activeSelf == false && ShopUI.activeSelf == true)
            {
                ShopUI.SetActive(false);
                MenuUI.SetActive(true);
                PlayerCtrl.bIsSetUI = true;
            }
            else
            {
                MenuUI.SetActive(false);
                Time.timeScale = 1;
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                PlayerCtrl.bIsSetUI = false;
            }
          
        }
    }

    public void SetupAllWeapons(WeaponBase[] weapons)
    {
        SetupMagazine();

        for(int i = 0; i< weapons.Length; ++i)
        {
            weapons[i].onAmmoEvent.AddListener(UpdateAmmoHUD);
            weapons[i].onMagazineEvent.AddListener(UpdateMagazineHUD);
        }
    }

    public void SwitchingWeapon(WeaponBase newWeapon)
    {
        weapon = newWeapon;

        SetupWeapon();
    }

    private void SetupWeapon()
    {
        textWeaponName.text = weapon.WeaponName.ToString();
        imageWeaponIcon.sprite = spriteWeaponIcons[(int)weapon.WeaponName];
        imageWeaponIcon.rectTransform.sizeDelta = sizeWeaponIcons[(int)weapon.WeaponName];
    }

    private void UpdateAmmoHUD(int currentAmmo, int maxAmmo)
    {
        textAmmo.text = $"<size=40>{currentAmmo}/</size>{maxAmmo}";
    }
    private void SetupMagazine()
    {
        magazineList = new List<GameObject>();
        for(int i = 0; i < maxMagazineCount; ++i)
        {
            GameObject clone = Instantiate(magazineUIPrefab);
            clone.transform.SetParent(magazineParent);
            clone.SetActive(false);

            magazineList.Add(clone);
        }
    }

    private void UpdateMagazineHUD(int currentMagazine)
    {
        saveCurrentMagazine = currentMagazine;
        for(int i = 0; i < magazineList.Count; ++i)
        {
            magazineList[i].SetActive(false);
        }
        for(int i = 0; i < currentMagazine; ++i)
        {
            magazineList[i].SetActive(true);
        }
    }

    private void UpdateHPHUD(int previous, int current)
    {
        textHP.text = "HP " + current;

        if (previous <= current) return;

        if(previous - current > 0)
        {
            StopCoroutine("OnBloodScreen");
            StartCoroutine("OnBloodScreen");
        }
    }

    private IEnumerator OnBloodScreen()
    {
        float percent = 0;

        while(percent < 1)
        {
            percent += Time.deltaTime;

            Color color = imageBloodScreen.color;
            color.a = Mathf.Lerp(1, 0, curveBloodScreen.Evaluate(percent));
            imageBloodScreen.color = color;

            yield return null;
        }
    }

    private void UpdateGoldHUD(int gold)
    {
        textGold.text = "Gold " + gold;
    }

    private void UpdateScoreHUD(int score)
    {
        textScore.text = "Score " + score + " / " + ClearScore;

        if(score >= ClearScore)
        {
            playerWeapons[0].GetComponent<WeaponAssultRifle>().bIsClear = true;
            MissionUI.SetActive(true);
            Time.timeScale = 0;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            PlayerCtrl.bIsSetUI = true;
            bIsMission = true;

        }
    }
   
    public void OnClickShopBtn()
    { 
        ShopUI.SetActive(true);
        MenuUI.SetActive(false);
    }

    public void OnClickHPItemBtn()
    {
        int gold = status.GetGold();
        if(500 <= gold)
        {
            Debug.Log("hpitem");
            status.IncreaseHP(50);
            status.DecreaseGold(500);
        }
    }

    public void OnClickMagazineItemBtn()
    {
        int gold = status.GetGold();
        if (200 <= gold)
        {
            Debug.Log("Magazine");
            weaponSwitchSystem.IncreaseMagazine(WeaponType.Main, 2);
            status.DecreaseGold(200);
        }
        
    }


    private void KeyInput()
    {
        if (bIsMission)
        {
            if (Input.GetKeyDown(keyCodeEnter))
            {
                SavingSystem saving = FindObjectOfType<SavingSystem>();
                status.SetScore(0);
                PlayerCtrl.gameObject.transform.position = new Vector3(0, 1.2f, 0);
                PlayerCtrl.gameObject.transform.eulerAngles = new Vector3(0, 0, 0);
                saving.Save(defaultSaveFile);
                SceneManager.LoadScene("BossStage");
                Time.timeScale = 1;
                bIsMission = false;
                
            }
        }
    }

    public object CaptureState()
    {
        Debug.Log("capture");            
        data["HP"] = status.CurrentHP;
        data["Score"] = status.GetScore();
        data["Gold"] = status.GetGold();
        data["Magazine"] = saveCurrentMagazine;
        data["position"] = new SerializableVector3(PlayerCtrl.gameObject.transform.position);
        data["rotation"] = new SerializableVector3(PlayerCtrl.gameObject.transform.eulerAngles);

        return data;
    }

    public void RestoreState(object state)
    {
        Debug.Log("restore");
        data = (Dictionary<string, object>)state;
        status.SetHP((int)data["HP"]);
        status.SetGold((int)data["Gold"]);
        status.SetScore((int)data["Score"]);
        PlayerCtrl.gameObject.transform.position = ((SerializableVector3)data["position"]).ToVector();
        PlayerCtrl.gameObject.transform.eulerAngles = ((SerializableVector3)data["rotation"]).ToVector();

        UpdateHPHUD((int)data["HP"], (int)data["HP"]);
        UpdateScoreHUD(status.GetScore());
        UpdateGoldHUD(status.GetGold());
        UpdateMagazineHUD((int)data["Magazine"]);

    }

    public void SaveBtn()
    {
        SavingSystem saving = FindObjectOfType<SavingSystem>();
        saving.Save(defaultSaveFile);
    }

    public void LoadBtn()
    {
        SavingSystem saving = FindObjectOfType<SavingSystem>();

        saving.LoadLastScene(defaultSaveFile);
    }

}
