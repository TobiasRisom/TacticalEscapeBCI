using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SharedDatastructures;

public class PlayerFeatures : MonoBehaviour
{
    private Image[] manaPoints;
    private Image[] healthPoints;
    public float health, maxHealth = 100;
    public float mana, maxMana;
    public float manaCost = 1;
    public float fixedRegenPoints;
    private float lastHealth;
    private Shake shake;
    public bool alive = true;
    
    [NonSerialized] public Gamemode gamemode;
    private Image[] manaUI;
    
    private LoggingManager _loggingManager;
    private int dmgTaken = 0;
    private string eventStr;
    private AmmoSpawn ammoSpawn;
    private LanguageVersionManager LVManager;
    
    void Start()
    {   
        // Get components
        _loggingManager = GameObject.Find("LoggingManager").GetComponent<LoggingManager>();
        manaPoints = GameObject.Find("Manapoints").GetComponentsInChildren<Image>();
        healthPoints = GameObject.Find("FillerHearts").GetComponentsInChildren<Image>();
        manaUI = GameObject.Find("Manabar").GetComponentsInChildren<Image>();
        ammoSpawn = GameObject.Find("Ammo").GetComponent<AmmoSpawn>();
        LVManager = GameObject.Find("Language/VersionManager").GetComponent<LanguageVersionManager>();

        health = maxHealth;
        lastHealth = health;
        // print(gamemode);
        
        /*if (gamemode == Gamemode.Interval)
        {
            maxMana = mana = 99999;
            HideManaUI();
        }*/
        if(LVManager != null)
        {
            if(LVManager.gamePref == true) // Battery
            {
                maxMana = 4;
            }
            else // Interval
            {
                maxMana = 1;
            }
        }

        if (gamemode == Gamemode.Battery)
        {
            HideManaUI();
        }
    }

    void Awake() {
        shake = GameObject.Find("Main Camera").GetComponent<Shake>();
    }

    // Update is called once per frame
    void Update()
    {
        HealthBarFiller();
        ManaBarFiller();
        
        if (lastHealth > health)
        {
            shake.ShakeOnce(1f);
            lastHealth = health;
            dmgTaken++;
            eventStr = "Player Hurt";
            logPlayerData();
        }

        if (health > maxHealth)
            health = maxHealth;
    }

    void HealthBarFiller()
    {
        for (int i = 0; i < healthPoints.Length; i++)
        {
            healthPoints[i].enabled = !DisplayHealthPoints(health, i);
        }
    }

    bool DisplayHealthPoints(float _health, int pointNumber)
    {
        return (pointNumber * (maxHealth / healthPoints.Length) >= _health);
    }


    void ManaBarFiller()
    {
        if (gamemode == Gamemode.Interval) return;
        for (int i = 0; i < manaPoints.Length; i++)
        {
            manaPoints[i].enabled = !DisplayManaPoints(mana, i);
        }
    }

    bool DisplayManaPoints(float _mana, int pointNumber)
    {
        return (pointNumber * (maxMana / manaPoints.Length) >= _mana);
    }

    public void Damage(float dmgPoints)
    {
        if (health > 0)
            health -= dmgPoints;
        HealthBarFiller();
    }

    public void Heal(float healPoints)
    {
        if (health < maxHealth)
            health += healPoints;
    }

    public void RegenMana(float regenPoints)
    {
        eventStr = "RegenMana";
        logPlayerData();
        if (mana >= maxMana) return;
        if (mana + regenPoints <= maxMana)
        {
            mana += regenPoints;
            ammoSpawn.DeleteAmmo();
            ammoSpawn.SpawnAmmo();
        } 
        else mana = maxMana;
    }

    public void RegenMana(){
        eventStr = "RegenMana";
        logPlayerData();
        if (mana >= maxMana) return;
        mana += fixedRegenPoints;
    }

    public void Expend()
    {
        // Debug.Log("Decrease mana " + manaCost);
        if (mana - manaCost >= 0) 
        {
            mana -= manaCost;
            ammoSpawn.DeleteAmmo();
            ammoSpawn.SpawnAmmo();
        }
        ManaBarFiller();
    }

    public bool ManaCheck() {
        bool manaCheck = manaCost <= mana;
        // Debug.Log("ManaCheck: " + manaCheck);
        //if (!manaCheck) StartCoroutine(Blink());
        return manaCheck;
    }

    private IEnumerator Blink() {   
        Color originalColor = Color.white;
        Color blinkColor = Color.red;
        float blinkDuration = .2f;
        float alpha = 0f;
        Image manabar = GameObject.Find("Manabar").GetComponent<Image>();

        while (alpha < 1f) {
            alpha += Time.deltaTime / blinkDuration;
            manabar.color = Color.Lerp(originalColor, blinkColor, alpha);
            yield return null;
        }

        while (alpha > 0f) {
            alpha -= Time.deltaTime / blinkDuration;
            manabar.color = Color.Lerp(originalColor, blinkColor, alpha);
            yield return null;
        }

        manabar.color = originalColor;
    }

    public void Alive(AudioManager audioManager) {
        if (health > 0) return;
        alive = false;
        audioManager.PlayCategory("Death");
        _loggingManager.Log("Game", "Event", "PlayerDeath");
    }

    public void HideManaUI()
    {
        foreach (var currentElement in manaUI)
        {
            currentElement.enabled = false;
        }
    }
    
    private void logPlayerData()
    {
        _loggingManager.Log("Game", new Dictionary<string, object>()
        {
            {"Player Health", health},
            {"Player Mana", mana},
            {"Take Damage", dmgTaken},
            {"Event", eventStr},
            // {"State", Enum.GetName(typeof(State), state)},
        });

    }
    
}
