using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EntranceAreaTrigger : MonoBehaviour
{
    public float staySeconds = 2f;

    public string targetScene = "GameScene";

    float stayTimer;
    bool switched;


   

    void OnTriggerStay(Collider other)
    {
        if(switched) return;
        stayTimer += Time.deltaTime;

        if(stayTimer >= staySeconds)
        {
            switched = true;
            SceneManager.LoadScene(targetScene);
        }
    }

    void OiggerExit(Collider other)
    {
       stayTimer = 0f;
        
    }


}
