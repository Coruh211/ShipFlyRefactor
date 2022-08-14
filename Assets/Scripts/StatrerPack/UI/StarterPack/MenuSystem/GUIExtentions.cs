using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Object = UnityEngine.Object;

namespace Menus.Extentions
{
    public static class GUIExtentions
    {
        public static bool TryGetUI(this GUIManager Instance, string byName, out IUIMenu result)
        {
            result = GUIManager.GetUI(byName);
            return result != null;
        }

        public static bool TryGetUI<T>(this GUIManager Instacne, string byName, out T result) where T : Object, IUIMenu
        {
            result = GUIManager.GetUI(byName) as T;
            return result != null;
        }

        public static bool TryGetByType<T>(this GUIManager Instance, out T menu) where T : Object, IUIMenu
        {
            menu = GUIManager.GetUI<T>();
            return menu != null;
        }
    }

    public static class UIExtentions
    {
        /// <summary>
        /// Открывает меню, если оно еще не открыто. Альтернатива <see cref="GUIManager.Open(IUIMenu)"/>
        /// </summary>
        /// <param name="menu">Само меню</param>
        /// <param name="select">Выбрать меню в фокус,</param>
        public static void Open(this IUIMenu menu, bool select)
        {
            GUIManager.Open(menu, select);
        }
        
        /// <summary>
        /// Открывает меню, если оно еще не открыто. Альтернатива <see cref="GUIManager.Open(IUIMenu)"/>
        /// </summary>
        /// <param name="menu"></param>
        public static void Open(this IUIMenu menu)
        {
            GUIManager.Open(menu);
        }
        
        /// <summary>
        /// Закрывает меню, если оно еще открыто. Альтернатива <see cref="GUIManager.Close(IUIMenu)"/>
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        public static bool Close(this IUIMenu menu)
        {
            return GUIManager.Close(menu);
        }
        
        /// <summary>
        /// Оборачивает меню в определенный тип
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="menu"></param>
        /// <returns></returns>
        public static T As<T>(this IUIMenu menu) where T : Object, IUIMenu
        {
            return menu as T;
        }
        
        /// <summary>
        /// Сделать меню в фокусе
        /// </summary>
        /// <param name="menu"></param>
        public static void Focus(this IUIMenu menu)
        {
            GUIManager.Select(menu);
        }
    }
}
