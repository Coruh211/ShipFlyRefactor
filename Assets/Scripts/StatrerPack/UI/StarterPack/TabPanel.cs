using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TabPanel : MonoBehaviour
{
    public Action<int> OnPageSelected;
    
    [SerializeField] private Transform tabButtonsParent;
    [SerializeField] private Transform tabPagesParent;

    private GameObject[] pages;

    public GameObject[] Pages => pages;

    private void Start()
    {
        pages = tabPagesParent.GetChildes().Select(x => x.gameObject).ToArray();

        int j = 0;
        for (int i = 0; i < tabButtonsParent.childCount; i++)
            if (tabButtonsParent.GetChild(i).TryGetComponent(out Button tabButton))
            {
                int m = j++;
                tabButton.onClick.AddListener(() => SelectPage(m));
            }
    }

    public void SelectPage(int index)
    {
        foreach (GameObject page in pages)
        {
            if (page.activeSelf)
                page.SetActive(false);
        }
        
        pages[index].SetActive(true);
        OnPageSelected?.Invoke(index);
    }
}
