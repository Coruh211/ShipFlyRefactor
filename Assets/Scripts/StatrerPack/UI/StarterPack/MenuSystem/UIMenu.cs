using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Menus
{
    /// <summary>
    /// Абстрактный компонент, наследующий поддержку меню. Если нужно точное поведение, то вы можете реализовать интерфейс <see cref="IUIMenu"/>
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public abstract class UIMenu : MonoBehaviour, IEventUIMenu
    {
        [SerializeField, Tooltip("Объект изначально находится на сцене?")] private bool IsConst = true;

        private void Awake()
        {
            if (!IsConst)
                GUIManager.Instance.RegisterSelf(this);
            UIAwake();
        }
        /// <summary>
        /// Используйте этот Awake вместо стандартного
        /// </summary>
        protected virtual void UIAwake() { }

        #region interface
        public string Name => name;
        RectTransform IUIMenu.transform => transform as RectTransform;

        bool IUIMenu.IsActive => gameObject.activeSelf;
        #endregion

        #region callbacks
        void IEventUIMenu.OnOpen() => OnOpen();
        void IEventUIMenu.OnClosed() => OnClosed();
        void IEventUIMenu.OnFocused(bool focused) => OnFocused(focused);
        /// <summary>
        /// Вызывается при открытии меню
        /// </summary>
        protected virtual void OnOpen() { }
        /// <summary>
        /// Вызывается при закрытии меню
        /// </summary>
        protected virtual void OnClosed() { }
        /// <summary>
        /// Вызывается при смене фокуса меню. Если на меню фокусируются, то focused = true. Если меню теряет фокус, то focused = false
        /// </summary>
        /// <param name="focused"></param>
        protected virtual void OnFocused(bool focused) { }
        #endregion

#if UNITY_EDITOR
        private void Reset()
        {
            GUIManager.Instance.RegisterSelf(this);
        }
#endif
    }
}
