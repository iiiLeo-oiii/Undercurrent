using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fishingrodmanager : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.E)) 
        {
            //动画
            
            //gameObject.GetComponent<Animator>().enabled = false;
            gameObject.GetComponent<Animator>().enabled = true;

            if (gameObject.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
            {
                gameObject.GetComponent<Animator>().Play("抛竿", 0, 0);
            }
            
            //鱼鳔飞出
        }
    }

    public void function()
    {
        //gameObject.GetComponent<Animator>().SetBool("Reset", true);
    }
}
