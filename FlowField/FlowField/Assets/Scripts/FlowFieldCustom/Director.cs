using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Director : MonoBehaviour
{
    private static Director _director;

    public static Director Instance
    {
        get { return _director; }
    }
    public void Awake()
    {
        _director = this;
        SharedDataContainer.Init();
        SharedDataContainer.InitMapGrid();
        ArchyType.Initalize();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        SharedDataContainer.Clear();
    }
}
