using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Transition : MonoBehaviour
{
    public float speed;
    private string currentTransition = "";
    public void Exit()
    {
          this.currentTransition = "Exit";
    }

    void Update() {
        if(this.currentTransition == "Exit") {
            if(gameObject.transform.localScale.y >= 0.0f) {
                gameObject.transform.localScale -= Vector3.up * speed * Time.deltaTime;
            }
            else if (gameObject.active == true) {
                gameObject.SetActive(false);
            } 
        }
    } 
}