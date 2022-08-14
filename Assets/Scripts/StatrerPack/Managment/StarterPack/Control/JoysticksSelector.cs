using System;
using UnityEngine;
using UnityEngine.UI;

public class JoysticksSelector : Singleton<JoysticksSelector>
{
    [SerializeField] private ControlPreset[] controlPresets;
    [SerializeField] private Transform joystickParent;

    private Joystick currentJoystick = null;

    public Joystick CurrentJoystick => currentJoystick;

    public void Awake()
    {
        DataInstaller data = GameManager.Instance.dataInstaller;

        ControlPreset currentPreset = new ControlPreset();

        foreach (var preset in controlPresets)
        {
            if (preset.type != data.ControlType)
            {
                Destroy(preset.prefab);
            }
            else
            {
                currentPreset = preset;
                currentJoystick = preset.prefab.GetComponent<Joystick>();
            }
        }

        if (currentPreset.prefab == null)
        {
            return;
        }

        ShowJoystick(currentPreset.prefab.transform, data.ShowJoystick);
    }

    private void ShowJoystick(Transform joystick, bool enabled)
    {
        foreach (var image in joystick.GetComponentsInChildren<Image>())
        {
            image.enabled = enabled;
        }
    }

    [Serializable]
    private struct ControlPreset
    {
        public GameObject prefab;
        public ControlType type;
    }
}
