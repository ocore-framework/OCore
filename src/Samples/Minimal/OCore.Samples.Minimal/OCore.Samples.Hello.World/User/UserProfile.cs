using OCore.Entities.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Samples.Hello.World.User
{
    public class UserProfile : DataEntity<UserProfileData>, IUserProfile
    {
        public Task<string> Ping(string ping)
        {
            return Task.FromResult($"Pong! {ping}");
        }
    }
}
