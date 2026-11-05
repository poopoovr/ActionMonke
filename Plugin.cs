using BepInEx;
using UnityEngine;

namespace ActionMonke
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        private MonkeAvatar monkeAvatarInstance;

        private bool hooked = false;

        void Start()
        {
            monkeAvatarInstance = new GameObject("MonkeAvatar").AddComponent<MonkeAvatar>();
        }

        private bool wasInRoom = false;

        void Update()
        {
            if (!hooked && GorillaNetworking.CosmeticsController.instance != null)
            {
                GorillaNetworking.CosmeticsController.instance.OnCosmeticsUpdated += OnCosmeticsChanged;
                hooked = true;
            }

            bool inRoom = Photon.Pun.PhotonNetwork.InRoom;
            if (inRoom && !wasInRoom)
            {
                if (monkeAvatarInstance != null) monkeAvatarInstance.UpdateMonke();
            }
            wasInRoom = inRoom;
        }

        void OnCosmeticsChanged()
        {
            if (Photon.Pun.PhotonNetwork.InRoom && monkeAvatarInstance != null) 
                monkeAvatarInstance.UpdateMonke();
        }
    }
}
