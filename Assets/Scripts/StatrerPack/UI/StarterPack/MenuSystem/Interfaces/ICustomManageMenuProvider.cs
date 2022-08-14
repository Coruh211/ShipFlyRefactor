namespace Menus
{
    /// <summary>
    /// Advanced: Интерфейс провайдера по открытию и закрытию меню
    /// </summary>
    public interface ICustomManageMenuProvider : IUIMenu
    {
        /// <summary>
        /// Представляет собой собственную реализацию открытия меню
        /// </summary>
        void ProvideOpen();
        
        /// <summary>
        /// Представляет собой собственную реализацию закрытия меню
        /// </summary>
        void ProvideClose();
    }
}