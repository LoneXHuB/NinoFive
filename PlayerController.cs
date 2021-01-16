using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    protected Rigidbody2D rigid;
    public float speed;
    protected Animator animator;
    protected int punchCount = 0;
    protected float punchCountResetTimer = 0.7f;
    public int maxPunches = 1;
    protected Vector2 movement;
    protected float lookDirectionX = 1.0f;
    protected float frozenTimer = 0.5f;
    protected bool frozenMovement = false;
    public float maxHealth = 10.0f;
    protected float health;
    protected bool isVurnerable = false;
    protected float vulnerableTimer = 2.0f;
    protected int hitCount = 0;
    protected float hitResetTimer = 0.0f;
    protected float invincibleTimer = 0.0f;
    protected bool isInvincible = false;
    protected float invincibilityTime = 3.0f;
    protected new SpriteRenderer renderer;
    protected AudioSource audioSource;
    public AudioClip hitClip;
    public AudioClip knockDownClip;
    public AudioClip jump;
    public AudioClip landed;
    public AudioClip punch;
    public AudioClip burned;
    public AudioClip layed;
    protected RaycastHit2D[] hitBuffer = new RaycastHit2D[16];
    protected BoxCollider2D boxCollider;
    protected float lastX, lastY;
    private bool grounded;
    protected bool burning;
    protected bool enableControl = true;
    protected float noControlesTimer = .0f;
    
    public float lastPunchTime = float.NegativeInfinity;

    private float nextFireBallTimer = 4.0f;
    public float nextFireBallTime = 4.0f;

    [SerializeField]
    public GameObject FireBallPrefab;
    public TextMeshProUGUI Text;
    ScoreManager scoreManager;

    float congratsTimer = 3f;

    public bool Grounded
    {
        get => grounded;
        set
        {
            if (grounded != value && !grounded && !burning)
            {
                if(!frozenMovement)
                   PlaySound(landed);

                animator.SetTrigger("Landed");
            }
            else
                animator.ResetTrigger("Landed");

            grounded = value;
        }
    }


    [SerializeField]
    public LayerMask groundLayerMask;
    

    // Start is called before the first frame update
    protected void Start()
    {
        renderer = this.GetComponent<SpriteRenderer>();
        rigid = this.GetComponent<Rigidbody2D>();
        animator = this.GetComponent<Animator>();
        health = maxHealth;
        audioSource = this.GetComponent<AudioSource>();
        boxCollider = this.GetComponent<BoxCollider2D>();
        scoreManager = this.GetComponent<ScoreManager>();
    }

    public virtual void Update()
    {
        RefreshCountdowns();

        if (scoreManager.IsGameOverWin())
        {
            if (congratsTimer > 0f)
            {
                Text.text = "You WON !";
                congratsTimer -= Time.deltaTime;
            }
            else
                Text.gameObject.SetActive(false);
        }

        if (nextFireBallTimer > 0f )
        {
            nextFireBallTimer = Mathf.Clamp(nextFireBallTimer - Time.deltaTime, 0f, float.PositiveInfinity);
        }
        else
        {
            if(!scoreManager.IsGameOverWin())
                IncomingFireBall();
        }
        IsGrounded();
        if (Input.GetKeyDown(KeyCode.C) && IsGrounded() && enableControl)
        {
            Punch();
        }
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded() && !frozenMovement && enableControl)
        {
            Jump();
        }

        if (rigid.position.y <= -5f)
            ChangeHealth(-10f);

    }

    public virtual void FixedUpdate()
    {
        float horizontal = Input.GetAxis("Horizontal");
        if (!IsGrounded())
        {
            horizontal *= 0.5f;
        }
        else if (burning && rigid.velocity.y < 0f)
        {
            burning = false;
            rigid.gravityScale = 1f;
            Debug.Log("stopped burning");
            PlaySound(layed);
            DisableControlesFor(2f);
        }
        if (burning || !enableControl)
        {
            horizontal = 0f;
            movement = rigid.velocity;
        }
        else
        {
            movement = new Vector2(horizontal * speed * Time.deltaTime, rigid.velocity.y);
        }
        if (Mathf.Approximately(0f, horizontal))
        {
            movement = new Vector2(0f, rigid.velocity.y);
        }
        RaycastHit2D raycast = Physics2D.BoxCast(direction: new Vector2(horizontal, 0f), origin: boxCollider.bounds.center, size: boxCollider.bounds.size, angle: 0f, distance: 0.1f, layerMask: LayerMask.GetMask("Collidable"));

        RaycastHit2D stumbleDetected = Physics2D.BoxCast(direction: new Vector2(horizontal, 0f), origin: boxCollider.bounds.center, size: boxCollider.bounds.size, angle: 0f, distance: 0.1f, layerMask: groundLayerMask);
        if (raycast.collider != null || stumbleDetected.collider != null)
        {
            movement.x = rigid.velocity.x;
        }

        Move(movement);
        animator.SetBool("Grounded", IsGrounded());
        animator.SetFloat("LookDirection", lookDirectionX);
        animator.SetFloat("Move", movement.x);
        animator.SetFloat("Speed", movement.magnitude);
    }

    public void Burn(float push)
    {
        Grounded = false;
        burning = true;
        PlaySound(burned);
        animator.SetTrigger("Burned");
        lookDirectionX = -push;

        rigid.gravityScale = 0.5f;
        rigid.velocity = new Vector2(push, 1f) * 2f;

        ChangeHealth(-1.0f);
    }

    protected void DisableControlesFor(float seconds)
    {
        noControlesTimer = seconds;
        enableControl = false;
    }

    protected bool IsGrounded()
    {
        RaycastHit2D groundHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0.0f, new Vector2(0f, -1f), 0.1f, LayerMask.GetMask("ground"));
        RaycastHit2D landOnPlayer = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0.0f, new Vector2(0f, -1f), 0.1f, LayerMask.GetMask("Collidable"));

        Grounded = groundHit.collider != null || landOnPlayer.collider != null;
        animator.SetBool("Grounded", Grounded);
        return Grounded;
    }

    protected void Jump()
    {
        rigid.velocity = Vector2.up * 5f;
        animator.SetTrigger("Jump");
        PlaySound(jump);
    }

    protected void Move(Vector2 movement)
    {

        if (!Mathf.Approximately(movement.normalized.x,0.0f))
            this.lookDirectionX = movement.normalized.x;

        if (!frozenMovement)
            rigid.velocity = movement;
        else
            rigid.position = new Vector2(lastX, lastY);
    }

    protected void freezeFor(float seconds)
    {
        this.frozenTimer = seconds;
        lastX = rigid.position.x;
        lastY = rigid.position.y;
        this.frozenMovement = true;
    }

    protected void Punch()
    {
        if (punchCount == 0)
        {
            punchCountResetTimer = 2f;
        }
        lastPunchTime = Time.realtimeSinceStartup;

        animator.SetInteger("PunchCount", punchCount);
        animator.SetTrigger("Punch");

        if (punchCount <= maxPunches)
        {
            punchCount++;
            Debug.Log("punchCount incresed, next punch will be number : " + punchCount);
        }
        else
            punchCount = 0;

        Vector2 punchVelocity = new Vector2(lookDirectionX * 0.5f * speed * Time.deltaTime  , rigid.velocity.y);
        rigid.velocity = punchVelocity;
        PlaySound(punch);

        freezeFor(0.2f);
    }
    protected void IncomingFireBall()
    {
        Debug.Log("Incoming fireball !");
        nextFireBallTimer = nextFireBallTime;
        int incomingDirection = (int)UnityEngine.Random.Range(0.001f, 1.999f);
        Vector2 fireballPosition = new Vector2(-5f, rigid.position.y);
        if (incomingDirection == 1)
        {
            fireballPosition = new Vector2(-5f, rigid.position.y);
        }
        if (incomingDirection == 0)
        {
            fireballPosition = new Vector2(7f, rigid.position.y);
        }
        GameObject firballObject = Instantiate(FireBallPrefab, fireballPosition + Vector2.up * 0.2f, Quaternion.identity);
        firballObject.gameObject.SetActive(value: true);
        FireBallController fireBall = firballObject.GetComponent<FireBallController>();
        fireBall.Direction = ((incomingDirection == 1) ? 1 : (-1));
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        float damage = -1f;
        EnemyController player = collider.GetComponent<EnemyController>();
        Debug.Log("enemy attacked!");
        if (player != null && collider.gameObject.CompareTag("Enemy") && lastPunchTime > player.lastPunchTime)
        {
            player.TakePunch(damage, lookDirectionX);
        }
    }

    public void ChangeHealth(float amount)
    { 
        this.health = Mathf.Clamp(health + amount, 0.0f, maxHealth);
        UIHealthBar.instance.SetValue(health / (float)maxHealth);

        Debug.Log("Health changed to " + health / (float)maxHealth * 100 + "%");
        if (health <= .0f)
        {
            Die();
        }
    }

    void Die()
    {
        animator.SetTrigger("Dead");
        DisableControlesFor(float.PositiveInfinity);
        PlaySound(layed);
        StartCoroutine(BackToMainMenuIn(5));
        Text.text = "Ahlem you suck...";
        Debug.Log("You died!");
    }

    IEnumerator BackToMainMenuIn(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        MainMenu.BackToMainMenu();
        Debug.Log("back to main menu");
    }
    //Knockdown Animation
    protected void KnockDown()
    {
        animator.SetTrigger("KnockedDown");
        DisableControlesFor(1.8f);
        MakeInvincibleFor(5.0f);
        Debug.Log("Knocked Down!");
    }

    //Makes character invincible for n seconds
    protected void MakeInvincibleFor(float seconds)
    {
        isInvincible = true;
        invincibleTimer = seconds;
        invincibilityTime = seconds;
        Color transparent = new Color(1f, 1f, 1f, 0.5f);
        renderer.color = transparent;
    }

    //Makes character vulnerable for 2 seconds
    protected void MakeVulnerable()
    {
        PlaySound(hitClip);
        animator.SetTrigger("Vulnerable");
        freezeFor(3.0f);
        isVurnerable = true;
        vulnerableTimer = 3.0f;
        hitCount = 3;
    }

    //Character gets hit
    protected void Hit()
    {
        if ( hitResetTimer <= 0.0f )
        {
            hitCount = 0;
            hitResetTimer = 3.0f;
        }
        
        freezeFor(0.2f);

        if (hitCount <= 2)
        {
            PlaySound(hitClip);
            animator.SetTrigger("Hit");
            animator.SetInteger("HitCount", hitCount);
            Debug.Log("Small Hit ! hitcount : " + hitCount);
            hitCount++;
        }
        else
            hitCount = 0;
    
        Debug.Log("hitcount : " + hitCount);
    }

    public virtual void TakePunch(float damage, float sourceLookDirection)
    {
        this.lookDirectionX = -sourceLookDirection;
        this.animator.SetFloat("LookDirection", lookDirectionX);
        if (!this.isInvincible && enableControl && !frozenMovement)
        {
            if (this.hitCount == 2)
                this.MakeVulnerable();
            else if (this.isVurnerable)
            {
                damage *= 2;
                this.PlaySound(knockDownClip);
                this.KnockDown();
                this.hitCount = 0;
            }
            else
                this.Hit();
            this.ChangeHealth(damage);
        }
        else
            return;
    }

    protected void PlaySound(AudioClip clip)
    {
        this.audioSource.PlayOneShot(clip);
        if (clip == layed)
        {
            this.audioSource.clip = clip;
            this.audioSource.PlayDelayed(0.2f);
        }
    }
    //Any Timer goes here
    protected void RefreshCountdowns()
    {
        if (frozenTimer > 0.0f)
            frozenTimer = Mathf.Clamp(frozenTimer - Time.deltaTime, 0.0f, float.PositiveInfinity);
        else
            frozenMovement = false;

        if (noControlesTimer > 0.0f)
            noControlesTimer = Mathf.Clamp(noControlesTimer - Time.deltaTime, 0.0f, float.PositiveInfinity);
        else
            enableControl = true; 

        if (hitResetTimer > 0.0f)
            hitResetTimer = Mathf.Clamp(hitResetTimer - Time.deltaTime, 0.0f, 3.0f);

        if (punchCountResetTimer > 0.0f)
            punchCountResetTimer = Mathf.Clamp(punchCountResetTimer - Time.deltaTime, 0.0f, 5.0f);
        else
        {
            punchCount = 0;
            animator.SetInteger("PunchCount", punchCount);
        }

        if (vulnerableTimer > 0.0f)
        {
            vulnerableTimer = Mathf.Clamp(vulnerableTimer - Time.deltaTime, 0.0f, 2.0f);
            animator.SetFloat("VulnerableTimer", vulnerableTimer);
        }
        else
        {
            isVurnerable = false;
            animator.SetFloat("VulnerableTimer", 0.0f);
        }

        if (invincibleTimer > 0.0f)
            invincibleTimer = Mathf.Clamp(invincibleTimer - Time.deltaTime, 0.0f, invincibilityTime);
        else
        {
            isInvincible = false;
            renderer.color = Color.white;
        }
    }

}
