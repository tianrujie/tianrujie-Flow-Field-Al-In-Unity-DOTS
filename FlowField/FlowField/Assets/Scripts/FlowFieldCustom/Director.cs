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
        if (Input.GetMouseButtonDown(0))
        {
            var worldPos = SharedDataContainer.TouchPos2WorldPointByLayer(Input.mousePosition,"Default");
            Debug.Log($"Click World Pos: {worldPos}");
            if (SharedDataContainer.WorldPos2Idx(worldPos, out var idx, out var idx2))
            {
                Debug.Log($"Clicked [{idx},{idx2}]");
                SharedDataContainer.TargetCell = idx;
                SharedDataContainer.TargetCellVersion++;
            }
        }
    }

    private void OnDestroy()
    {
        SharedDataContainer.Clear();
    }
}
