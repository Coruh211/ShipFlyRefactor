namespace Menus
{
    /// <summary>
    /// Продвинутый интерфейс, поддерживающий каллбеки
    /// </summary>
    public interface IEventUIMenu : IUIMenu
    {
        /// <summary>
        /// Каллбек, вызывающийся на объекте при открытии меню
        /// </summary>
        void OnOpen();
        
        /// <summary>
        /// Каллбек, вызывающийся на объекте при закрытии меню
        /// </summary>
        void OnClosed();
        
        /// <summary>
        /// Каллбек, сообщающий о смене фокуса менюшки
        /// </summary>
        /// <param name="focused">Находится ли менюшка в фокусе?</param>
        void OnFocused(bool focused);
    }
}