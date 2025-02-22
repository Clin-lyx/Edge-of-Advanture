using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Character : MonoBehaviour, ISaveable
{
    [Header("Event Listener")]
    [SerializeField] VoidEventSO newGameEvent;

    [Header("Attributes")]
    [SerializeField]private float maxHealth;
    [SerializeField]private float currentHealth;
    private bool isNewgame;
    private Rigidbody2D rb;
    private PhysicsCheck physicsCheck;
    private GameObject enemy;

    [Header("Invulnerable")]
    public float invulnerableDuration;
    private float invulnerableCounter;
    public bool invulnerable;

    [Header("Events")]
    public UnityEvent<Character> OnHealthChange;
    public UnityEvent<Attack> OnTakeDamage;
    public UnityEvent IsDead;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        physicsCheck = GetComponent<PhysicsCheck>();
    }

    private void Start() {
        currentHealth = maxHealth;
    }

    private void NewGame() {
        isNewgame = true;
        TriggerInvulnerable();
        currentHealth = maxHealth;
        OnHealthChange?.Invoke(this);
    }

    private void OnEnable() {
        newGameEvent.OnEventRaised += NewGame;
        ISaveable saveable = this;
        saveable.RegisterSaveData();
    }

    private void OnDisable() {
        newGameEvent.OnEventRaised -= NewGame;
        ISaveable saveable = this;
        isNewgame = false;
        saveable.UnRegisterSaveData();
    }

    private void Update() {
        if (invulnerable) {
            invulnerableCounter -= Time.deltaTime;
            if (invulnerableCounter <= 0) {
                invulnerable = false;
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Death Trigger"))
        {
            if (currentHealth > 0 && !isNewgame)
            {
                // make character die, and update the health
                currentHealth = 0;
                OnHealthChange?.Invoke(this);
                IsDead?.Invoke();
            }
        }
    }

    public void TakeDamage(Attack attacker) {
        if (invulnerable) 
            return;

        if (this.gameObject.CompareTag("Player")) {
            PlayerController playerController = GetComponent<PlayerController>();
            if (playerController.IsPerfectDodge()) {
                playerController.perfect = true;
                enemy = attacker.transform.parent.gameObject;
                return;
            }
        }

        if (currentHealth - attacker.Damage() > 0) {
            this.currentHealth -= attacker.Damage();
            TriggerInvulnerable();
            OnTakeDamage?.Invoke(attacker);
        } else {
            currentHealth = 0;
            IsDead?.Invoke();
        }

        OnHealthChange?.Invoke(this);
    }

    // make character stop receiving damage
    private void TriggerInvulnerable() {
        if (!invulnerable) {
            invulnerable = true;
            invulnerableCounter = invulnerableDuration;
        }
    }

    public float HealthPercentage() {
        return currentHealth / maxHealth;
    }

    //Frame event for attack displacement
    public void MoveEvent(float disX)
    {
        int faceDir = (int)transform.localScale.x;
        if (!(physicsCheck.TouchLeftWall() && faceDir < 0) ||
            !(physicsCheck.TouchRightWall() && faceDir > 0)) 
        {
            rb.transform.Translate(faceDir * disX, 0, 0);
        }
    
    }

    public void ForceOnAir(float force)
    {
        rb.velocity = new Vector2(0f, -6f);
        rb.AddForce(transform.up * force, ForceMode2D.Impulse);
    }

    public GameObject GetEnemy(){
        return enemy;
    }

    public DataDefination GetDataID()
    {
        return GetComponent<DataDefination>();
    }

    public void GetSaveData(Data data)
    {
        if (data.characterPosDict.ContainsKey(GetDataID().ID))
        {
            data.characterPosDict[GetDataID().ID] = new SerializeVector3(transform.position);
            data.floatSavedData[GetDataID().ID + "health"] = this.currentHealth;
        }
        else
        {
            data.characterPosDict.Add(GetDataID().ID, new SerializeVector3(transform.position));
            data.floatSavedData.Add(GetDataID().ID + "health", this.currentHealth);
        }
    }

    public void LoadData(Data data)
    {
        Debug.Log(GetDataID());
        if (data.characterPosDict.ContainsKey(GetDataID().ID))
        {
            transform.position = data.characterPosDict[GetDataID().ID].ToVector3();
            this.currentHealth = data.floatSavedData[GetDataID().ID + "health"];
            
            // UI update
            OnHealthChange?.Invoke(this);
        }
    }
}
