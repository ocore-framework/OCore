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
    public class UserData
    {
        [Id(0)]
        public Guid UserId { get; set; }
    }


    [DataEntity("User")]
    public interface IUser : IDataEntity<UserData>
    {
        Task<string> TestGetProfile();
    }
}
