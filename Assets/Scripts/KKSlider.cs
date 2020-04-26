using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KKSlider : MonoBehaviour
{
    public Animator RightHand;
    public Animator LeftHand;
    public Animator Eyes;
    public Animator Mouth;

    public void StartGuitar()
    {
        RightHand.ResetTrigger("Idle");
        LeftHand.ResetTrigger("Idle");
        RightHand.SetTrigger("Play");
        LeftHand.SetTrigger("Play");
    }

    public void StopGuitar()
    {
        RightHand.ResetTrigger("Play");
        LeftHand.ResetTrigger("Play");
        RightHand.SetTrigger("Idle");
        LeftHand.SetTrigger("Idle");
    }

    public void Awooo()
    {
        Eyes.SetTrigger("Awoo");
        Mouth.SetTrigger("Awoo");
    }

    public void SlowSing()
    {
        Mouth.SetTrigger("Slow");
    }

    public void StartSing()
    {
        Mouth.ResetTrigger("Idle");
        Mouth.SetTrigger("Play");
    }

    public void StopSing()
    {
        Mouth.ResetTrigger("Play");
        Mouth.SetTrigger("Idle");
    }
}
