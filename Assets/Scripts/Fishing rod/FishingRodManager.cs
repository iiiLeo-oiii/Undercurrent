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
            //鱼鳔飞出
        }
    }
}
