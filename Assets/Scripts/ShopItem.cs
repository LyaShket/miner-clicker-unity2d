﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] Image image;
    [SerializeField] Text title;
    [SerializeField] Text description;
    [SerializeField] Text price;

    [Header("Logic")]
    public int type = 0;
    public int priceCount;

    [Header("Design")]
    [SerializeField] Color availableColor;
    [SerializeField] Color notAvailableColor;

    private Image imageComp;

	private void Start()
	{
        imageComp = GetComponent<Image>();
        RerenderUI();
	}

	private void Update()
	{
        if (SaveScript.money >= priceCount)
            imageComp.color = availableColor;
        else
            imageComp.color = notAvailableColor;
    }

    private void RerenderUI()
    {
        price.text = priceCount.ToString();
        switch (type) {
            case 0:
                description.text = $"Min Mining Power - {Player.minPower * 2}\nMax Mining Power - {Player.minPower * 4}";
                break;
            case 1:
                description.text = $"Mining Delay - {Player.miningDelay / 1.2f}s";
                break;
        }
    }

    private void Buy()
    {
        switch (type) {
            case 0:
                Player.minPower *= 2;
                Player.maxPower = Player.minPower * 2;
                priceCount = (int)(priceCount * 2.6f);
                break;
            case 1:
                Player.miningDelay /= 1.2f;
                priceCount = (int)(priceCount * 2.8f);
                break;
        }
        RerenderUI();
    }

    public void OnBuyButton()
	{
        if (GameManager.ChangeMoney(-priceCount))
		{
            Buy();
        }
    }
}