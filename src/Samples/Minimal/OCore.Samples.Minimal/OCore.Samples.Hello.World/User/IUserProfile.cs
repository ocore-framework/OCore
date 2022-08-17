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
    public class UserProfile
    {

    }

    public interface IUserProfile
    {
    }
}
