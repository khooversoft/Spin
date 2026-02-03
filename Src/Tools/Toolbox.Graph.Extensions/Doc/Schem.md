PaGraph relationships...

| Schema         | Query                                     |
| -----------    | ----------------------------------------- |
| SecurityGroup  | SecurityGroup -> Principal                |
| Channel        | Channel -> SecurityGroup -> Principal     |
| Account        | Account -> Principal                      |
| TicketGroup    | TicketGroup -> SecurityGroup -> Principal |


#### Schema: SecurityGroup -> Users

Security group is a group of principals with rights

1) Use for verifying security access
2) Can be used provider ownership/member list

```
select (key=user:user1@domain.com) -> [securityGroup];
```

#### Schema: Account -> Users

Account's owner

#### Schema: TicketGroup -> [SecurityGroup] -> Principal

User's in ticket group.

```
select (key=ticketGroup:group1) -> [securityGroup] -> [principal];
select (key=user:user1) <- [SeceurityGroup] <- [ticketGroup];]
```

### Security Group

1) SecurityGroupId
2) List of Principals with rights (None, Read, Contributor, Owner)


## Id(s)

Id schema for creating releated Node keys.

#### PrincipalId

"user:{principalId}"
NodeTag = "principalIdentity"

