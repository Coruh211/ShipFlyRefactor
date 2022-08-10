
using UnityEngine;

namespace Menus
{
    /// <summary>
    /// Базовый интерфейс реализации меню
    /// </summary>
    public interface IUIMenu
    {
        /// <summary>
        /// Строковый уникальный идентификатор. Имя не должно меняться в рантайме
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Активно ли сейчас меню
        /// </summary>
        bool IsActive { get; }
        
        /// <summary>
        /// Интерфейсный трансформ
        /// </summary>
        RectTransform transform { get; }
    }
}
