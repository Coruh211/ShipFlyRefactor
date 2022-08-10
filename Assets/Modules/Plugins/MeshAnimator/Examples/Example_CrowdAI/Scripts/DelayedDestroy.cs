using System.Collections;
using UnityEngine;

public class DelayedDestroy : MonoBehaviour {

    public float delay;
    IEnumerator Start()
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}
