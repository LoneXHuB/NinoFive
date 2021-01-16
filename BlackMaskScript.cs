using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackMaskScript : MonoBehaviour
{
    Animator animator;

    private void Start()
    {
        animator = this.GetComponent<Animator>();
        StartCoroutine(desactivateIn(3));
    }
    public void FadeIn()
    {
        animator.SetTrigger("BlackMask");
    }
    IEnumerator desactivateIn(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        this.gameObject.SetActive(false);
    }
}
