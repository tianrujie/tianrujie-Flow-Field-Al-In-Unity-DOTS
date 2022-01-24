using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TT : MonoBehaviour
{
    public GameObject src;
    
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 10000; i++)
        {
            var obj = GameObject.Instantiate(src);
            obj.transform.position = new Vector3(Random.Range(-175f,175f),0,Random.Range(-175f,175f));
            obj.name = obj.name + "i";
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void DrawCell()
    {
        //Graphics.
    }
}
