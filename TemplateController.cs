using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemplateController : MonoBehaviour
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
    public float nextFireBallTime = 4.0f;
    protected bool dead = false;

    [SerializeField]
    public GameObject FireBallPrefab;

    public bool Grounded
    {
        get => grounded;
        set
        {
            if (grounded != value && !grounded && !burning)
            {
                if (!frozenMovement)
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
        Grounded = groundHit.collider != null;
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

        if (!Mathf.Approximately(movement.normalized.x, 0.0f))
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
        
        PlaySound(punch);

        freezeFor(0.2f);
    }

    protected void ChangeHealth(float amount)
    {
        this.health = Mathf.Clamp(health + amount, 0.0f, maxHealth);
        if(health <= .0f)
        {
            Die();
        }
        Debug.Log("Health changed to : " + health);
    }

    protected void Die()
    {
        if(!dead)
        {
            animator.SetTrigger("Dead");
            freezeFor(float.PositiveInfinity);
            PlaySound(layed);
            dead = true;
            ScoreManager.IncreaseScore();
        }
    }

    //Knockdown Animation
    protected void KnockDown()
    {
        animator.SetTrigger("KnockedDown");
        freezeFor(2.0f);
        MakeInvincibleFor(3.0f);
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
        if (hitResetTimer <= 0.0f)
        {
            hitCount = 0;
            hitResetTimer = 2.5f;
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
