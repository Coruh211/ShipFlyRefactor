using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MatEditor;

public class MatEditorExtenshion_LevelPatternSelector : MonoBehaviour
{
    [SerializeField] int[] supportedPatternsIndexes;
    [SerializeField] int forcePatternIndex = -1;

    private void Start()
    {
        int selIndex = -1;
        if(forcePatternIndex>=0)
        {
            selIndex = forcePatternIndex;
        }
        else
        {
            selIndex = supportedPatternsIndexes[Random.Range(0, supportedPatternsIndexes.Length)];
        }

        if(MaterialSelector.SupportPatternIndex(selIndex))
        {
            MaterialSelector.ChangeGlobalIndex(selIndex);
        }
        else
        {
            Debug.LogError("Invalid pattern index");
        }    
    }
}
