using System.Collections;
using UnityEngine;

public class SetRandomColor : MonoBehaviour {

    IEnumerator Start () {
        yield return null;
        var r = GetComponentInChildren<Renderer>();
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        r.GetPropertyBlock(block);
        block.SetColor("_Color", Random.ColorHSV());
        r.SetPropertyBlock(block);
	}
}
