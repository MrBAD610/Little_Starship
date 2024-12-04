using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColonistDiagramUIHandler : MonoBehaviour
{
    [Header("Body Region Images")]
    [SerializeField] private Image headImage;
    [SerializeField] private Image neckImage;
    [SerializeField] private Image chestImage;
    [SerializeField] private Image rightArmImage;
    [SerializeField] private Image leftArmImage;
    [SerializeField] private Image midsectionImage;
    [SerializeField] private Image pelvisImage;
    [SerializeField] private Image rightLegImage;
    [SerializeField] private Image leftLegImage;

    private Sprite headSprite;
    private Sprite neckSprite;
    private Sprite chestSprite;
    private Sprite rightArmSprite;
    private Sprite leftArmSprite;
    private Sprite midsectionSprite;
    private Sprite pelvisSprite;
    private Sprite rightLegSprite;
    private Sprite leftLegSprite;


    // Start is called before the first frame update
    void Start()
    {
        InitializeBodyRegionSprites(); // Initialize the sprites for each body region
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void InitializeBodyRegionSprites()
    {
        headSprite = headImage.sprite;
        neckSprite = neckImage.sprite;
        chestSprite = chestImage.sprite;
        rightArmSprite = rightArmImage.sprite;
        leftArmSprite = leftArmImage.sprite;
        midsectionSprite = midsectionImage.sprite;
        pelvisSprite = pelvisImage.sprite;
        rightLegSprite = rightLegImage.sprite;
        leftLegSprite = leftLegImage.sprite;
    }

    private void SetDisplay() // Set the display of the body regions based on the colonist's injuries
    {
        
    }
}
