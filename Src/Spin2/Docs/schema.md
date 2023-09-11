# Schema Strategy

The Spin system uses a resource ID pattern for all addresses in the system.

Schema = identifies Spin resource such as subscription, user, contract, {custom}, etc...

Resource ID format = {schema}:domain/path[/path...]

Acceptable patterns for resource ID are...
1. Principal ID
    1. ex. user1@domain.com
    1. fmt: \{user}@\{domain}
1. Account ID.
    1. Schema is optional or will accept any.
    1. fmt: domain/accountid[/path...] or schema:domain/accountid[/path...]

Examples...
- User = user1@domain.com or user:user1@domain.com.  Schema "user" is optional
- User:user1@company5.net
- kid:user1@company3.com/sign
- principal-key:user1@company5.net/sign
- principal-private-key:user1@company5.net/sign
- contract:company3.com/path
- softbank:company3.com/accountId
- contract:company3.com/softbank/contractId
- attachment:company3.com/softbank/accountId  (note: same as contract domain/path...)
- smartc:domain.com/packageId

System...
- subscription:{name}
- tenant:{domain}
- system:scheduler
- agent:{name}

Wellknown schemas...
- "contract:" - The API for the block chain storage.  These block chains by any SmartC service provider.
- "lease:" - Provides a presistance locking service for coordinating shared resources or specific application features like placing funds on hold for a transfer.
- "principal-key:" - Manages the public key for a user.  This key is used to validate a JWT signature.
- "principal-private-key:" - Manages the private key for a user.  This key is used to create a signed JWT.
- "signature:" - Provides the signing and validation service
- "subscription:" - Manages subscriptions
- "tenant:" - Manages tenants
- "user:" - Identifies a user in the directory
- "attachment:" - Service for attaching data to block chain contracts.  These are wellknown block types.



