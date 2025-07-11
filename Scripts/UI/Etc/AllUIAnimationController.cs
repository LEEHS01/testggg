using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllUIAnimationController : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        this.GetComponent<Animator>().SetTrigger("UI_UnSellect");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnCompleteAni() {
        this.GetComponent<Animator>().ResetTrigger("UI_UnSellect");
        //this.GetComponent<Animator>().enabled = false;
    }
}
