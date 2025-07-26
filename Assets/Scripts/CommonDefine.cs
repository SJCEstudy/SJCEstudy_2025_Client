public static class CommonDefine
{
    public const string WEB_BASE_URL = "http://127.0.0.1:3000/";

    public const string REGISTER_URL = "users/register";
    public const string LOGIN_URL = "users/login";
    

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


