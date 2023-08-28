Schema - Actors

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

1. Confirm Actor (contract, message, deposit, etc...)
1. Message Actor (send secure message)
1. Credit card actor to providers ??
1. Finance smart contract
    1. Host Process
    1. Contract EXE


Scenario...
Rent a house for a year

- Total amount / number of months
- Down payment
- Security deposit
- Bank account connection

Flow...
- User fills out form - (start SmartC, @state=started)
    - Upload driver license
- Owner approves form and provides bank info for payment
- User fills out contract and signs
    - Connects with bank account for payment
    - TrxPush deposit and security to owner
    - Sign contract
- User ack's keys and access - @state=active
- (future: message + pictures)
- Owner and User ACK start
- SmartC auto takes money from user's account to owner based on schedule
- SmartC auto completes contract
- SmartC send out renew reminder to owner
- SmartC provides financial and ledger info for reporting


Ship Require
DatalakeResourceIdMap - how to manage schema to storage map, currently hardcoded
