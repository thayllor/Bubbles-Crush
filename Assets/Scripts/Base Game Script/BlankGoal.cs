using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlankGoal : MonoBehaviour
{

    public Sprite breakableSprite;
    public Sprite lockSprite;
    public Sprite concreteSprite;
    public Sprite slimeSprite;
    public Sprite redSprite;
    public Sprite darkGreenSprite;
    public Sprite lightGreenSprite;
    public Sprite blueSprite;
    public Sprite whiteSprite;
    public Sprite orangeSprite;

    public Goalkind goalKind;

    private Sprite goalSprite;
    private string matchValue;

    public Sprite GetSprite()
    {
        Setup();
        return goalSprite;
    }
    public string GetTag()
    {
        Setup();
        return matchValue;
    }
    private void Setup()
    {
        if (goalSprite == null)
        {
            switch (goalKind)
            {
                case Goalkind.Breakable:
                    goalSprite = breakableSprite;
                    matchValue = "Breakable";
                    break;
                case Goalkind.Lock:
                    goalSprite = lockSprite;
                    matchValue = "Lock";
                    break;
                case Goalkind.Concrete:
                    goalSprite = concreteSprite;
                    matchValue = "Concrete";
                    break;
                case Goalkind.Slime:
                    goalSprite = slimeSprite;
                    matchValue = "Chocolate";
                    break;
                case Goalkind.Red:
                    goalSprite = redSprite;
                    matchValue = "Red Dot";
                    break;
                case Goalkind.DarkGreen:
                    goalSprite = darkGreenSprite;
                    matchValue = "Dark Green Dot";
                    break;
                case Goalkind.LightGreen:
                    goalSprite = lightGreenSprite;
                    matchValue = "Yellow Dot";
                    break;
                case Goalkind.Blue:
                    goalSprite = blueSprite;
                    matchValue = "Dark Blue Dot";
                    break;
                case Goalkind.White:
                    goalSprite = whiteSprite;
                    matchValue = "White Dot";
                    break;
                case Goalkind.Orange:
                    goalSprite = orangeSprite;
                    matchValue = "Orange Dot";
                    break;
                default:
                    break;
            }
        }
    }
}

