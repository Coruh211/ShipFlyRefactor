using PathologicalGames;
using UnityEngine;

public class CoinMover : MonoBehaviour
{
    [SerializeField] private float speed = 1f;
    
    private Vector3 endPosition;
    private RectTransform rectTransform;
    private int countReward;

    public void MoveAt(Vector3 endPosition, int countReward)
    {
        this.countReward = countReward;
        this.endPosition = endPosition.ChangeZ();
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if ((endPosition - rectTransform.position).magnitude > speed * Time.deltaTime)
            rectTransform.position += (endPosition - rectTransform.position).normalized * (speed * Time.deltaTime);
        else
        {
            CoinsManager.Instance.CoinsCount.Value += countReward;
            PoolManager.Pools["CoinsPool"].Despawn(transform);
        }
    }
}