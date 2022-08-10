using FSG.MeshAnimator;
using UnityEngine;

public class AI_Unit : MonoBehaviour
{
    private static int _maxMovingUnits = 5000;
    private static int _movingUnits = 0;

    public enum ActionType
    {
        idle,
        move,
        die,
        attack
    }
    [System.Serializable]
    public struct AIAction
    {
        public ActionType type;
        public MeshAnimationBase[] animations;
    }

    public MeshAnimatorBase meshAnimator;
    public AIAction[] actions;
    public float maxMoveDistance = 20;
    public float moveSpeed = 1;

    private bool _chooseNewAction = true;
    private AIAction _currentAction;
    private float _actionEndTime;
    private Vector3 _currentPosition;
    private Vector3 _movePosition;

    private void OnEnable()
    {
        AI_Manager.Add(this);
        _chooseNewAction = true;
        transform.forward = new Vector3(Random.value - 0.5f, 0, Random.value - 0.5f);
    }
    private void OnDisable()
    {
        AI_Manager.Remove(this);
    }
    public void Tick(float time, float deltaTime)
    {
        PerformAction(time, deltaTime);
    }
    private void PerformAction(float time, float deltaTime)
    {
        if (meshAnimator == null)
            return;
        if (_chooseNewAction)
        {
            _chooseNewAction = false;
            _currentAction = actions[Random.Range(0, actions.Length)];
            if (_currentAction.type == ActionType.move)
            {
                if (_movingUnits > _maxMovingUnits)
                    _currentAction = actions[0];
                else
                    _movingUnits++;
            }
            string actionAnimation = null;
            if (_currentAction.animations.Length > 0)
            {
                actionAnimation = _currentAction.animations[Random.Range(0, _currentAction.animations.Length)].animationName;
                meshAnimator.Play(actionAnimation);
            }

            switch (_currentAction.type)
            {
                case ActionType.idle:
                    {
                        _actionEndTime = time + 5;
                        break;
                    }
                case ActionType.move:
                    {
                        _currentPosition = transform.position;
                        _movePosition = _currentPosition + new Vector3(Random.Range(-1f, 1f) * maxMoveDistance, 0, Random.Range(-1f, 1f) * maxMoveDistance);
                        transform.LookAt(_movePosition);
                        break;
                    }
                case ActionType.attack:
                    {
                        _actionEndTime = time + meshAnimator.GetClip(actionAnimation).Length;
                        break;
                    }
                case ActionType.die:
                    {
                        _actionEndTime = time + meshAnimator.GetClip(actionAnimation).Length;
                        System.Action<string> dieAction = null;
                        dieAction = (anim) =>
                        {
                            meshAnimator.OnAnimationFinished -= dieAction;
                            gameObject.SetActive(false);
                        };
                        meshAnimator.OnAnimationFinished += dieAction;
                        break;
                    }
            }
        }
        if (_currentAction.type == ActionType.move)
        {
            _currentPosition = Vector3.MoveTowards(_currentPosition, _movePosition, deltaTime * moveSpeed);
            if (_currentPosition == _movePosition)
            {
                _actionEndTime = 0;
                _movingUnits--;
            }
            else
            {
                _actionEndTime = time;
            }
            try
            {
                transform.position = _currentPosition;
            }
            catch { }
        }
        if (Time.time > _actionEndTime)
            _chooseNewAction = true;
    }
}
