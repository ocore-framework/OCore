# Authorization

## Concepts

- Account: A user account. A user account is identified by an id, a stringed Guid
- ApiKey: An API key is a secret token that a client can use to authenticate with the server
- Role: A role is a collection of permissions
- Tenant: A tenant is a collection of users, roles and permissions
- Elevation: Elevation is the process of temporarily granting a user additional permissions
  - An authorization endpoint is used to elevate a user. This will give the user free access to all
    resources in the system when called futher down the chain
  - Specific endpoint can decide to not accept elevated calls
- Permission: A permission is a string that represents a specific