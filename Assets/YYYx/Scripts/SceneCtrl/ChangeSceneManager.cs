using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using ZivenSDK;

public class ChangeSceneManager : MonoBehaviour
{
    
    public string nextChapterName = "FirstScene";
    bool prev;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            EnterNext();

        var hand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        if (hand.TryGetFeatureValue(CommonUsages.triggerButton, out bool p))
        {
            if (p && !prev) EnterNext();
            prev = p;
        }
    }

    void EnterNext()
    {
        if (string.IsNullOrEmpty(nextChapterName)) return;
        LBEManager.Instance.EnterChapter(nextChapterName);
    }

}
