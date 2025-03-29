using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Character : MonoBehaviour
{
    [SerializeField] private CharacterSO characterSO;
    [SerializeField] private Image portrait;
    [SerializeField] private Enum_Mood startingMood;
    public string Name => characterSO.characterName;

    private void Awake()
    {
        portrait = gameObject.GetComponent<Image>();
        SetMood(startingMood);
    }

    public void SetMood(Enum_Mood mood)
    {
        portrait.sprite = characterSO.GetSprite(mood);
    }
}
