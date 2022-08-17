using OCore.Entities.Data;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Samples.Hello.World.User
{

    [Serializable]
    [GenerateSerializer]
    public class UserProfileData
    {

    }

    [DataEntity("UserProfile")]
    public interface IUserProfile : IDataEntity<UserProfileData>
    {
        Task<string> Ping(string ping);
    }
}
