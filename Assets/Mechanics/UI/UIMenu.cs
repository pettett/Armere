using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIMenu : MonoBehaviour
{
    //All the buttons that can be used to move up
    [System.Serializable]
    public class UpwardNavigationButton
    {
        public Button button;
        public UIMenu menu;
    }
    //All the elements that will be disabled when the upwards menu is activated
    public Selectable[] menuElements;
    public UpwardNavigationButton[] upwardNavigationButtons;
    public Button backButton;
    public GameObject holder;
    public bool menuOpen = false;
    public bool menuActive = false;
    int currentUpwardsMenu;
    protected virtual void Start()
    {
        if (backButton != null)
            backButton.onClick.AddListener(CloseMenu);
        for (int i = 0; i < upwardNavigationButtons.Length; i++)
        {
            //Setup every upward navigation
            int index = i;
            upwardNavigationButtons[i].button.onClick.AddListener(() => NavigateUp(index));
        }
    }
    public virtual void OpenMenu()
    {
        ToggleMenu(true);
        currentUpwardsMenu = -1;
    }
    public virtual void CloseMenu()
    {
        ToggleMenu(false);
        if (currentUpwardsMenu != -1)
        {
            //Close up menu
            upwardNavigationButtons[currentUpwardsMenu].menu.CloseMenu();
        }
    }
    public virtual void ToggleMenu(bool active)
    {
        menuOpen = active;
        menuActive = active;
        holder.SetActive(active);

    }
    public virtual void ToggleMenuActive(bool active)
    {
        menuActive = active;
        for (int i = 0; i < menuElements.Length; i++)
        {
            menuElements[i].interactable = active;
        }
    }

    public void NavigateUp(int toIndex)
    {
        currentUpwardsMenu = toIndex;
        if (menuActive)
            StartCoroutine(
                NavigateMenu(
                    upwardNavigationButtons[toIndex].menu,
                    //Reset the active up menu when complete
                    new System.Action(() => currentUpwardsMenu = -1)));
    }

    public IEnumerator NavigateMenu(UIMenu menu, System.Action onComplete = null)
    {
        ToggleMenuActive(false);
        //disable this menu while the upper menu is in use
        yield return menu.WaitForMenu();
        ToggleMenuActive(true);
        onComplete?.Invoke();
    }
    public virtual IEnumerator WaitForMenu()
    {
        OpenMenu();
        //wait until the menu is open
        yield return new WaitUntil(() => !menuOpen);
        print("Menu closed");
    }
}
