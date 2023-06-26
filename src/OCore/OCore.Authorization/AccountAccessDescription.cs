using Orleans;
using System;
using OCore.Authorization.Request;

namespace OCore.Authorization
{

    [Serializable]
    [GenerateSerializer]
    public class AccountAccessDescription
    {
        [Id(0)]
        public string Resource { get; set; }
        [Id(1)]
        public Permissions Permissions { get; set; }
    }
}
