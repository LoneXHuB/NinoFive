using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBallController : MonoBehaviour
{
    Rigidbody2D rigid;
    Animator animator;
    AudioSource audioSource;
    float direction;
    bool frozen = false;

    public float speed;
    public AudioClip fireBallShot;
    public AudioClip fireballHit;

    public float Direction { set => direction = value; }

    void OnEnable()
    {
        rigid = this.GetComponent<Rigidbody2D>();
        animator = this.GetComponent<Animator>();
        audioSource = this.GetComponent<AudioSource>();

        PlaySound(fireBallShot);
    }


    // Update is called once per frame
    void Update()
    {
        Vector2 movement;

        if (direction > 0.0f)
            movement = new Vector2(speed, 0.0f);
        else
            movement = new Vector2(-speed, 0.0f);

        if (frozen)
            movement = Vector2.zero;

        Move(movement);

        if(rigid.position.x > 20f || rigid.position.x < -20f)
        {
            Destroy(this.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!frozen && other.gameObject.name.Equals("Player"))
        {
            PlaySound(fireballHit);
            animator.SetTrigger("Hit");
            
            frozen = true;

            PlayerController player = other.gameObject.GetComponent<PlayerController>();
            if( player != null )
            {
                float push = direction < 0.0f ? -1 : 1;
                player.Burn(push);
            }
        }
    }

    void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }

    void Move(Vector2 movement)
    {
        rigid.velocity = movement;
        animator.SetFloat("Direction", movement.normalized.x);
    }


}
