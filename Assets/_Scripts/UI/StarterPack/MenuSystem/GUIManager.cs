using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Menus
{
    /// <summary>
    /// Менеджер по управлению менюшками. Является синглтоном, не поддерживает DontDestroy
    /// </summary>
    public class GUIManager : Singleton<GUIManager>
    {
        protected override void AwakeSingletone()
        {
            cashedUIList = new Dictionary<string, IUIMenu>(prebakeCashedListUI.Count);
            cashedUIListTyped = new Dictionary<Type, IUIMenu>(prebakeCashedListUI.Count);
            selectQuene = new IUIMenu[prebakeCashedListUI.Count];

            foreach (var prb in prebakeCashedListUI)
            {
                if (prb == null || !(prb is IUIMenu ui)) 
                    continue;
                
                cashedUIList.Add(ui.Name, ui);
                
                if (!cashedUIListTyped.ContainsKey(ui.GetType()))
                    cashedUIListTyped.Add(ui.GetType(), ui);
            }
        }

        private IUIMenu selectedUI;
        [SerializeField, Tooltip("Будет ли выбранное активное меню перекрывать другие, если они так же активны")] private bool overlayedSelectedUI = true;
        [SerializeField] private List<Object> prebakeCashedListUI = new List<Object>();

        private Dictionary<string, IUIMenu> cashedUIList;
        private Dictionary<Type, IUIMenu> cashedUIListTyped;

        private IUIMenu[] selectQuene;
        private int quenePivot;

        private Action<IUIMenu> onOpenMenuHandler;
        private Action<IUIMenu> onCloseMenuHandler;
        private Action<IUIMenu> onSelectMenuHandler;
        
        private void Enqueue(IUIMenu menu)
        {
            selectQuene[quenePivot++] = menu;
        }
        
        public IUIMenu Dequeue()
        {
            IUIMenu output = selectQuene[quenePivot];
            selectQuene[quenePivot--] = null;
            return output;
        }

        //property
        /// <summary>
        /// Вызывается при открытии неактивного меню
        /// </summary>
        public static event Action<IUIMenu> OnOpenMenuHandler
        {
            add => Instance.onOpenMenuHandler += value;
            remove => Instance.onOpenMenuHandler -= value;
        }
        
        /// <summary>
        /// Вызывается при закрытии активного меню
        /// </summary>
        public static event Action<IUIMenu> OnCloseMenuHandler
        {
            add => Instance.onCloseMenuHandler += value;
            remove => Instance.onCloseMenuHandler -= value;
        }
        
        /// <summary>
        /// Вызывается при смене таргета на новое меню
        /// </summary>
        public static event Action<IUIMenu> OnSelectMenuHandler
        {
            add => Instance.onSelectMenuHandler += value;
            remove => Instance.onSelectMenuHandler -= value;
        }
        
        /// <summary>
        /// Флаг на перекрытие активной и выбранной менюшки других менюшек
        /// </summary>
        public static bool OverlayedSelectedUI { 
            get => Instance.overlayedSelectedUI; 
            set 
            {
                Instance.overlayedSelectedUI = value;
                if(value)
                {
                    if((Object)Instance.selectedUI)
                        Instance.selectedUI.transform.SetAsLastSibling();
                }
            }
        }
        
        /// <summary>
        /// Получить экземпляр меню по его имени
        /// </summary>
        /// <param name="byName">Имя меню</param>
        /// <returns></returns>
        public static IUIMenu GetUI(string byName)
        {
            return Instance.cashedUIList.TryGetValue(byName, out IUIMenu val) ? val : null;
        }
        
        /// <summary>
        /// Получить экземпляр меню по его имени и преобразовав к конкретному типу
        /// </summary>
        /// <typeparam name="T">Тип меню</typeparam>
        /// <param name="byName">Имя меню</param>
        /// <returns></returns>
        public static T GetUI<T>(string byName) where T : Object, IUIMenu
        {
            return Instance.cashedUIList.TryGetValue(byName, out IUIMenu val) ? val is T t ? t : null : null;
        }

        /// <summary>
        /// Возвращает первое меню определенного типа
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetUI<T>() where T : Object, IUIMenu
        {
            return Instance.cashedUIListTyped.TryGetValue(typeof(T), out IUIMenu val) ? val as T : null;
        }

        /// <summary>
        /// Возвращает текущий выбранный UI
        /// </summary>
        /// <returns></returns>
        public static IUIMenu Selected() => Instance.selectedUI;

        public static T Selected<T>() where T : Object, IUIMenu => Instance.selectedUI as T;

        /// <summary>
        /// Устанавливает меню по имени в качестве выбранного. Так же отображает его поверх других активных меню
        /// </summary>
        /// <param name="byName">Имя меню</param>
        public static void Select(string byName) => Select(GetUI(byName));

        /// <summary>
        /// Устанавливает меню в качестве выбранного. Так же отображает его поверх других активных меню
        /// </summary>
        /// <param name="menu"></param>
        public static void Select(IUIMenu menu)
        {
            if ((Object)menu)
                if (menu == Instance.selectedUI || !menu.IsActive) return;

            if (Instance.selectedUI is IEventUIMenu ieuim)
                ieuim.OnFocused(false);

            if ((Object)menu)
            {
                Instance.Enqueue(Instance.selectedUI);
                Instance.selectedUI = menu;

                if (Instance.overlayedSelectedUI)
                    menu.transform.SetAsLastSibling();
            }
            else
                Instance.selectedUI = null;

            if (menu is IEventUIMenu aieuim)
                aieuim.OnFocused(true);

            Instance.onSelectMenuHandler?.Invoke(menu);
        }

        /// <summary>
        /// Открывает меню по имени
        /// </summary>
        /// <param name="byName">Название меню</param>
        /// <param name="select">Поместить меню сразу в фокус?</param>
        public static void Open(string byName, bool select = false) => Open(GetUI(byName), select);

        /// <summary>
        /// Открывает меню, если оно не открыто. Если нужно отобразить открытое меню поверх других, используйте <see cref="Select(IUIMenu)"/>
        /// </summary>
        /// <param name="menu"></param>
        /// <param name="select"></param>
        public static void Open(IUIMenu menu, bool select = false)
        {
            if (menu == null || menu.IsActive) 
                return;

            if (menu is ICustomManageMenuProvider prv)
                prv.ProvideOpen();
            else
                menu.transform.gameObject.SetActive(true);

            if (menu is IEventUIMenu ieuim)
                ieuim.OnOpen();

            Instance.onOpenMenuHandler?.Invoke(menu);

            if (select)
                Select(menu);
        }


        /// <summary>
        /// Открывает меню по типу
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="select">Выбрать меню в фокус?</param>
        public static void Open<T>(bool select = false) where T : Object, IUIMenu => Open(GetUI<T>(), select);

        /// <summary>
        /// Закрывает меню, если оно октрыто
        /// </summary>
        /// <param name="byName">Имя меню</param>
        /// <returns></returns>
        public static bool Close(string byName) => Close(GetUI(byName));

        /// <summary>
        /// Закрывает меню по типу
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool Close<T>() where T : Object, IUIMenu => Close(GetUI<T>());

        /// <summary>
        /// Закрывает меню, если оно открыто
        /// </summary>
        /// <param name="menu">Меню</param>
        /// <returns>Было ли успешно закрыто меню</returns>
        public static bool Close(IUIMenu menu)
        {
            if (menu == null || !menu.IsActive) 
                return false;

            if (menu is ICustomManageMenuProvider prv)
                prv.ProvideClose();
            else
                menu.transform.gameObject.SetActive(false);

            bool isFocused = Selected() == menu;

            if (menu is IEventUIMenu ieuim)
            {
                ieuim.OnClosed();

                if (isFocused)
                    Select(Instance.Dequeue());
            }

            Instance.onCloseMenuHandler?.Invoke(menu);
            return true;
        }
        
        /// <summary>
        /// Регистрирует новое меню в системе
        /// </summary>
        /// <param name="newMenu"></param>
        public void RegisterSelf<T>(T newMenu) where T : Object, IUIMenu
        {
            if (!Instance.prebakeCashedListUI.Contains(newMenu))
                Instance.prebakeCashedListUI.Add(newMenu);
            else 
                return;

            if (!Application.isPlaying) 
                return;
            
            cashedUIList.Add(newMenu.Name, newMenu);
            cashedUIListTyped.Add(newMenu.GetType(), newMenu);
            Array.Resize(ref selectQuene, selectQuene.Length * 2);

        }
        
#if UNITY_EDITOR
        /// <summary>
        /// EDITOR ONLY
        /// </summary>
        public void FindAllMenusOnSceneAndCash()
        {
            List<Object> allMenus = new List<Object>(128);
            Canvas[] allCanvases = FindObjectsOfType<Canvas>();
            foreach(Canvas canvas in allCanvases)
            {
                IUIMenu[] menus = canvas.GetComponentsInChildren<IUIMenu>(true);
                foreach(IUIMenu mn in menus)
                {
                    if (mn is Object cm)
                    {
                        if (!allMenus.Contains(cm))
                            allMenus.Add(cm);
                    }
                }
            }

            prebakeCashedListUI = allMenus;
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            Debug.Log($"Find and register {allMenus.Count} menus");
        }
#endif
    }
}
