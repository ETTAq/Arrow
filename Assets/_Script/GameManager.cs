using UnityEngine;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{
    [Header("式式式式式 幗が 式式式式式")]
    public Button getPointUpgrade;
    public Button autoGetPointUpgrade;
    public Button shotSpeedUpgrade;
    public Button autoShotUpgrade;
    public Button addBowUpgrade;

    private float getPoint = 1;

    private float cost_getPointUpgrade = 50;
    private float cost_autoGetPointUpgrade = 200;
    private float cost_shotSpeedUpgrade = 150;
    private float cost_autoShotUpgrade = 500;
    private float cost_addBowUpgrade = 300;

    private float factor_getPointUpgrade = 5;
    private float factor_autoGetPointUpgrade = 20;
    private float factor_shotSpeedUpgrade = 10;
    private bool isAutoShotEnabled = false;
    private float factor_addBowUpgrade = 1.5f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
