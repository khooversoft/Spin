# Schema - Actors

1. Subscription
1. Tenant
1. User
1. Public key
1. Private keys
1. SoftBank
1. Contract
1. Signature
1. Configuration - need to review for dead code
 

Next...
1. Finance smart contract
    - Host Process
    - Contract EXE
1. Security - hook up to Azure Active Directory, MFA enabled
1. Security - used User JWT ticket for "Principal ID"
1. UI subscriptions, tenant, users, soft bank
1. Create SmartC
1. Confirm UI (contract, message, deposit, etc...)
1. Message UI (send secure message)
1. Credit card actor to providers ??  (SoftBank=Credit card provider)


# Scenario...
Rent a house for a year
- Total amount / number of months
- Down payment
- Security deposit
- Bank account connection

Flow...
- User fills out form - (start SmartC, @state=started)
    1. Upload driver license
- Owner approves form and provides bank info for payment
- User fills out contract and signs
    1. Connects with bank account for payment
    1. TrxPush deposit and security to owner
    1. Sign contract
- User ack's keys and access - @state=active
- (future: message + pictures)
- Owner and User ACK start
- SmartC auto takes money from user's account to owner based on schedule
- SmartC auto completes contract
- SmartC send out renew reminder to owner
- SmartC provides financial and ledger info for reporting


Ship Require Features...
- DatalakeResourceIdMap - how to manage schema to storage map, currently hardcoded
