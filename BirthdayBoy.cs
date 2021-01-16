using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BirthdayBoy : TemplateController
{
    public GameObject player;
    protected Vector2 playerPosition;

    public GameObject dialogBox;
    public TextMeshProUGUI Text;
    public GameObject cake;
    public GameObject BlackMask;
    public GameObject bgMusic;

    bool playerIsClose = false;
    bool inPosition = false;
    float punchTimer = 0.5f;
    IList<string> speach = new List<string>();
    int currentIndex = 0;

    private void OnEnable()
    {
        playerPosition = player.gameObject.GetComponent<Rigidbody2D>().position;

        dialogBox.SetActive(false);
        
        speach.Add("Hey! come close.. let's have a lil talk ...");
        speach.Add("don't kill me ... I just wanna talk !");
        speach.Add("The king has something important to tell you...");
        speach.Add("You have faught bravely, but you can't defeat his majesty !");
        speach.Add("But as a token of respect, our king asks you to kindly come back home where you belong ...");
        speach.Add("He even humbly says, that he misses you !");
        speach.Add("That he loves and cherishes you...");
        speach.Add("So I hereby ask you again ! come to him ... after all, today is a special day ...");
        speach.Add("I hear a day like today 2 years ago was the day his majesty declared his flame to you !");
        speach.Add("So here is one last touch of his gratitude.");
        speach.Add("Happy anniversary Ahlouma !");
        speach.Add("I made this game on the side for you, even tho it sucks, but I suck too xD soo ... that's around 1000 lines of code for this shit!");
        speach.Add("GoodBye !!");
    }
    // Update is called once per frame
    void Update()
    {
        base.RefreshCountdowns();
        IsGrounded();
    }

    public void NextSpeach()
    {
        Debug.Log("NextSpeach called");
        if(!Input.GetKeyDown(KeyCode.Space))
        {
            
            if (playerIsClose)
            {
                if (currentIndex + 1 < speach.Count)
                    currentIndex++;
                else
                {
                    StartCoroutine(EndGame(3));
                }
                if (currentIndex == 9)
                {
                    PlaySound(landed);
                    animator.SetTrigger("Landed");
                    cake.SetActive(true);
                }
                Text.text = speach[currentIndex];
            }
            else
                Text.text = speach[0];
        }
        
    }


    IEnumerator EndGame(int seconds)
    {
        bgMusic.SetActive(false);
        BlackMask.gameObject.SetActive(true);
        yield return new WaitForSeconds(seconds);
        MainMenu.LoadEnding();
    }
    void FixedUpdate()
    {
        playerPosition = player.gameObject.GetComponent<Rigidbody2D>().position;

        punchTimer -= Time.deltaTime;

        movement.y = rigid.velocity.y;
        movement.x = 1f;

        if (rigid.position.x >= 1f)
            inPosition = true;

        if (inPosition)
        {
            movement.x = .0f;

            if (playerIsClose)
                Text.text = speach[currentIndex];
            else
                Text.text = speach[0];

            dialogBox.SetActive(true);
        }

        Debug.Log("in position :" + inPosition);
        Debug.Log("playerClose :" + playerIsClose);
        Vector2 distance = playerPosition - base.rigid.position;

        if (distance.magnitude < 1.5f)
        {
            inPosition = true;
            playerIsClose = true;
            movement.x = .0f;
            base.lookDirectionX = distance.x > 0.0f ? 1 : -1;
        }
        else
        {
            playerIsClose = false;
            inPosition = false;
        }
        Move(movement);

        animator.SetBool("Grounded", IsGrounded());
        animator.SetFloat("LookDirection", lookDirectionX);
        animator.SetFloat("Move", movement.x);
        animator.SetFloat("Speed", movement.magnitude);
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
        }
        else
            return;
    }


}
