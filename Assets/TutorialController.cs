using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TutorialController : MonoBehaviour
{
    private int currentPage = 0;
    [SerializeField] private GameObject[] pages;
    public void goToPage(int change)
    {
        pages[this.currentPage].SetActive(false);
        pages[(this.currentPage + change) % this.pages.Count()].SetActive(true);
        this.currentPage = (this.currentPage + change) % this.pages.Count();
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            goToPage(this.pages.Count()-1);
            // Debug.Log("goToPage");
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            goToPage(1);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            gameObject.SetActive(false);
        }
    } 
}