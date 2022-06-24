﻿using OCore.Authorization.Abstractions;
using Orleans;
using System;
using System.Collections.Generic;
using System.Text;

namespace OCore.Authorization
{
    /// <summary>
    ///   All resources have an associated list of role-permissions pairs which this class is used to represent.
    /// </summary>

    [Serializable]
    [GenerateSerializer]
    public class AccessDescription : IEquatable<AccessDescription>
    {
        public string Role { get; set; }
        public Permissions Permissions { get; set; }

        public AccessDescription()
        {
        }

        public AccessDescription(string role, Permissions permissions)
        {
            Role = role;
            Permissions = permissions;
        }

        public bool Equals(AccessDescription other)
        {
            return Role.Equals(other.Role) && Permissions == other.Permissions;
        }
    }
}
