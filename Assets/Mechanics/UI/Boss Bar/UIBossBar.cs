using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UIBossBar : MonoBehaviour
{
    public Image mainHealth;
    public Image changedHealth;
    public TextMeshProUGUI bossName;

    Animator animator;

    public Color mainColor = Color.red;
    public Color invincibleColor = Color.yellow;

    float lastHealth;

    private void Start()
    {
        animator = GetComponent<Animator>();
        lastHealth = 1;
    }
    public void StartBossBarTracking(string boss)
    {
        gameObject.SetActive(true);
        bossName.SetText(boss);
        UpdateHealth(1);
    }

    public void StartInvincible()
    {
        changedHealth.fillAmount = 1;
        mainHealth.fillAmount = 1;
        mainHealth.color = invincibleColor;
    }
    public void EndInvincible()
    {
        mainHealth.color = mainColor;

        mainHealth.fillAmount = lastHealth;
        changedHealth.fillAmount = lastHealth;
    }

    public void UpdateHealth(float newRelitiveHealth)
    {
        changedHealth.fillAmount = lastHealth;
        lastHealth = newRelitiveHealth;
        mainHealth.fillAmount = newRelitiveHealth;
    }

    public IEnumerator StopBossBar()
    {
        animator.SetTrigger("Stop");
        yield return new WaitForSeconds(1);
        gameObject.SetActive(false);
    }

}
