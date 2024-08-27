namespace BrashSquirrel.Shared.Constants;

// if this gets cumbersome consider breaking into separate classes for each resource
public static class ResourceKeys
{
    // open ai
    public static string AZURE_OPENAI_CHAT_DEPLOYMENT_NAME = "chat";
    public static string AZURE_OPENAI_CHAT_MODEL_NAME = "gpt-4o-mini";
    public static string AZURE_OPENAI_CHAT_MODEL_VERSION = "2024-07-18";
    public static string AZURE_OPENAI_CHAT_MODEL_SKU = "GlobalStandard";
    public static int AZURE_OPENAI_CHAT_MODEL_CAPACITY = 1000;

    public static string AZURE_OPENAI_CONN_STR = "openai";


    // app insights
    public static string APP_INSIGHTS = "appinsights";
    public static string APPLICATIONINSIGHTS_CONNECTION_STRING = "APPLICATIONINSIGHTS_CONNECTION_STRING";


    // api
    public static string API_PROJECT_NAME = "apiservice";

    // web
    public static string WEB_PROJECT_NAME = "webfrontend";

    // cosmos
    public static string COSMOS_NAME = "cosmos";
    public static string COSMOS_DB_NAME = "chatHistory";
    public static string COSMOS_CONTAINER_NAME = "chatSessions";
}
