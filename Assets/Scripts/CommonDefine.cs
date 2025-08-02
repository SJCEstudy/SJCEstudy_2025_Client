using System.Collections.Generic;

public static class CommonDefine
{
    public const string WEB_BASE_URL = "http://127.0.0.1:3000/";

    public const string REGISTER_URL = "users/register";
    public const string LOGIN_URL = "users/login";
    public const string GET_MY_POKEMON_URL = "users/pokemons";

    public const string LOADING_SCENE = "LoadingScene";
    public const string GAME_SCENE = "GameScene";
    public const string LOGIN_SCENE = "SampleScene";
}

#region POST_DATA
public class LoginPostData
{
    public string id;
    public string password;
}

#endregion

public class LoginData
{
    public string sessionId;
    public string id;
}

[System.Serializable]
public class MyPokemon
{
    public int poketmonId;
    public string name;
    public int hp;
    public List<PokemonSkill> skills;
}

[System.Serializable]
public class PokemonSkill
{
    public int pokemon_id;
    public int skill_id;
    public string name;
    public string type;
    public string target;
    public int damage;
    public int pp;
}

public class ServerPacket
{
    public string packetType;
    public string packetValue;
}

