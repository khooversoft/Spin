# Work Schedule

The basic flow for a SmartC to get executed is...

1. Work is scheduled with Schedule's actor (from SmartC in the future or some other event)
1. Agent running asks Agent's actor for work
1. Agent's actor asks Schedule Actor for work
1. Agent's actor setup SmartC with details including WorkId
1. Agent load SmartC and passes command
1. SmartC contact is executed and talks to it's SmartC's Actor to get details and provide updates
1. SmartC tells it's actor the results of its operation.


Notes:
1. Agent actor controls the work for it's running agent, including setup details in SmartC's actor.
1. SmartC only communicates with its Actor
