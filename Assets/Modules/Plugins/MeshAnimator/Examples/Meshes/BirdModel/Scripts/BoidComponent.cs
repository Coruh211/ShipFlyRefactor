using FSG.MeshAnimator;
using System.Collections;
using UnityEngine;

public class BoidComponent : MonoBehaviour
{
    public float speed = 20;
    private Quaternion velocity;
    private IEnumerator Start()
    {
        speed += Random.value - 0.5f * speed * 0.25f;
        InvokeRepeating("UpdateTarget", Random.value * 5, Random.value * 5);
        UpdateTarget();
        yield return null;
        GetComponent<MeshAnimatorBase>().speed = 1f + Random.Range(-0.25f, 0.5f); 
    }
    private void UpdateTarget()
    {
        if (transform.position.y < 10f)
            velocity = Quaternion.LookRotation(Vector3.up * 0.25f + new Vector3(Random.value - 0.5f, 0, Random.value - 0.5f));
        else if (transform.position.y > 30f)
            velocity = Quaternion.LookRotation(Vector3.down * 0.25f + new Vector3(Random.value - 0.5f, 0, Random.value - 0.5f));
        else
            velocity = Quaternion.LookRotation(Random.onUnitSphere);
    }
    private void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
        transform.rotation = Quaternion.Lerp(transform.rotation, velocity, Time.deltaTime * 2f);
    }
}
