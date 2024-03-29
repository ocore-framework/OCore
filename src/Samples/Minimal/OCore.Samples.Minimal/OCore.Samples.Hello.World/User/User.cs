﻿using OCore.Entities.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCore.Samples.Hello.World.User
{
    public class User : DataEntity<UserData>, IUser
    {
        public async Task<string> TestGetProfile()
        {
            var userProfile = Get<IUserProfile>();
            return await userProfile.Ping("Hello");            
        }
    }
}
