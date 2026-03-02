# Copilot Instructions

## Project Guidelines
- In `PrincipalCollection.Add(...)`, assume `PrincipalId`/`Key` identity is authoritative and index updates ensure consistency; update logic should not redundantly validate that the `PrincipalId` is the same when updating an existing principal.