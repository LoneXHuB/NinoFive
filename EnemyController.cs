using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : TemplateController
{
    public GameObject player;
    protected Vector2 playerPosition;

    public float maxWalkRange;
    public float minWalkRange;

    bool invertMovement = false;
    float punchTimer = 0.5f;

    private void OnEnable()
    {
        playerPosition = player.gameObject.GetComponent<Rigidbody2D>().position;
    }
    // Update is called once per frame
    void Update()
    {
        base.RefreshCountdowns();

        IsGrounded();
    }

    void FixedUpdate()
    {
        playerPosition = player.gameObject.GetComponent<Rigidbody2D>().position;

        punchTimer -= Time.deltaTime;

        movement.y = rigid.velocity.y;

        if (invertMovement)
            movement.x = -1;
        else
            movement.x = 1;
        
        if (rigid.position.x >= maxWalkRange && !invertMovement)
            invertMovement = true;
        if (rigid.position.x <= minWalkRange && invertMovement)
            invertMovement = false;



        Vector2 distance = playerPosition - base.rigid.position;
        if (dead)
            movement.x = .0f;
        else
        {
            if (distance.magnitude < 1.5f)
            {
                movement.x = .0f;
                base.lookDirectionX = distance.x > 0.0f ? 1 : -1;
                if (punchTimer <= .0f && distance.magnitude < 0.5f)
                {
                    punchTimer = 0.5f;
                    Punch();
                }
            }
        }
       
        

        Move(movement);

        animator.SetBool("Grounded", IsGrounded());
        animator.SetFloat("LookDirection", lookDirectionX);
        animator.SetFloat("Move", movement.x);
        animator.SetFloat("Speed", movement.magnitude);
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        float damage = -1.0f;
        PlayerController player = collider.GetComponent<PlayerController>();

        if (player != null && collider.gameObject.CompareTag("Player") && lastPunchTime > player.lastPunchTime)
        {
            player.TakePunch(damage, this.lookDirectionX);
        }
        
    }

    public void TakePunch(float damage, float sourceLookDirection)
    {
        this.lookDirectionX = -sourceLookDirection;
        this.animator.SetFloat("LookDirection", lookDirectionX);
        if (!this.isInvincible)
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


}
