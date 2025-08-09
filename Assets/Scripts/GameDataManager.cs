using System.Collections.Generic;
using UnityEngine;

public class GameDataManager : Singleton<GameDataManager>
{
    public LoginData loginData = null;

    public PokemonShop[] pokemonShopList = null;
    public MyPokemon[] myPokemonList = null;
    public HashSet<int> myPokemonIds = null;

    public double walletBalance = -1;

    public string nextScene = "";

    public int testNum = 5;
    protected override void Awake()
    {
        base.Awake();  // ΩÃ±€≈Ê √ ±‚»≠

        Debug.Log("GameDataManager init");
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ResetData()
    {
        loginData = null;

        pokemonShopList = null;
        myPokemonList = null;
        myPokemonIds = null;

        walletBalance = -1;
    }
}
