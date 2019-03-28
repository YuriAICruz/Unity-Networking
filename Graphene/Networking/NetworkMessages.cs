namespace Graphene.Networking
{
    public enum NetworkMessages
    {
        AddPlayerOnClient = 9001,
        SendPlayerToServer = 9002,
        GetPlayerFromClient = 9003,
        RemovePlayerOnClient = 9004,
        UpdatePlayerOnServer = 9005,
        UpdatePlayerOnClient = 9006,
        OpenScene = 9007,
        StartGame = 9008,
        SpawnPointState = 9010,
        
        //TODO: Custom Messages
        UpdatePositionOnServer = 8000,
        UpdatePositionOnClient = 8001,
        OpenSceneCustom = 8002,
        OpenVrSceneCustom = 8003,
        OpenSceneSingleCustom = 8004,
    }
}