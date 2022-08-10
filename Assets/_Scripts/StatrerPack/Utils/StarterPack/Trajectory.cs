using System.Collections.Generic;
using UnityEngine;

public class Trajectory : MonoBehaviour
{
    public GameObject TrajectoryObjectPrefab;
    public float Height;
    public float Width;
    public float Speed = 1;
    public bool Visible = true;
    public GameObject MovingObject;
    
    private int _currentIndex = -1;
    private List<Transform> points = new List<Transform>();
    
    private void Start()
    {
        for (int i = 0; i < 11; i++)
        {
            Transform newObject = Instantiate(TrajectoryObjectPrefab).transform;
            newObject.SetParent(transform);
            newObject.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            points.Add(newObject);
        }
    }
    
    private void Update()
    {
        for (int i = 0; i < points.Count; i++)
        {
            float x = -Width + 2 * Width * i / (points.Count - 1);
            int n = points.Count / 2;
            int m = i <= points.Count / 2 ? i : points.Count - i - 1;
            float y = Height * (1 - (float) (n - m) * (n - m) / (n * n));
            points[i].position = new Vector3(x, y, 0);
        }

        TranslateObject();

        foreach (Transform point in points)
            point.GetComponent<MeshRenderer>().enabled = Visible;
    }

    private void TranslateObject()
    {
        if (!MovingObject)
        {
            _currentIndex = -1;
            return;
        }

        if (_currentIndex == -1)
        {
            _currentIndex = 0;
            MovingObject.transform.position = points[0].position;
        }

        if (_currentIndex >= points.Count)
        {
            MovingObject = null;
            _currentIndex = -1;
            return;
        }

        float height = (MovingObject.transform.position.y - transform.position.y) / Height;
        float speed = Speed / (1 + height * 2);
        Vector3 delta = (points[_currentIndex].position - MovingObject.transform.position).normalized * (Time.deltaTime * speed);
        
        if (delta.magnitude > (points[_currentIndex].position - MovingObject.transform.position).magnitude 
            || (points[_currentIndex].position - MovingObject.transform.position).magnitude < 0.01f)
            _currentIndex++;

        MovingObject.transform.position += delta;
    }
}