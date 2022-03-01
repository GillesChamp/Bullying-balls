using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Pulse : MonoBehaviour
{
   // pulse in dimension the object
    void Start()
    {
        DOTween.Init();
        transform.DOScale(2.0f, 0.5f).SetLoops(-1, LoopType.Yoyo);
    }


    void Update()
    {
        
    }
}
