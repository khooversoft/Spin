# SmartC - Smart Contract Executable

SmartC are executables responsible for implementing the requirements of a specific contract.  Examples are consumer loan,
rental agreement, line of credit, etc...

### Dependency for Startup
- Agent(s) must be registered
- SmartC must be registered


### Startup process
The following is the steps required to host and start a SmartC.

1. **Agent** start with cluster URL specified
1. **Agent** talks to verify registration and operational status (not regisitered, running/paused)
1. **Agent** monitor to activity queue in scheduler
    1. note: future is backed by service bus
1. **Scheduler** schedules work based on registered SmartC configuration and/or SmartC Events
    1. note: SmartC events will be backed by service bus
1. **Agent** is assigned SmartC contract with command line parameters
    1. Agent verifies status of SmartC and its registration
1. **Agent** starts child process with command line parameters
1. **Agent** monitors process and output and sends data to agent's actor
    1. SmartC starts
    1. Gets the SmartC configuration from SmartC's actor
    1. Execute command(s)
    1. Closes out SmartC Event with scheduler



### Components...

The following are the components that are required to host and execute a SmartC.

| Component                 | Notes                                                               |
| ------------------------- | ------------------------------------------------------------------- |
| Agent Scheduler Actor     | SmartC host, start/stop/monitor/management SmartC executables       |
| Agent Actor               | Actor for each agent running, messages, control, etc...             |
| Directory Actor           | General directory for the system, key with key value pairs          |
| SpinAgent.exe             | Spin service agent, run n exe(s) (monitor/management)               |
| {SmartC}.exe              | SmartC (Smart Contract) executable, workflow,validation,reporting   |

Notes...
- Agent scheduler manages running agents through their actor.
- Agent Scheduler service's actor key is "smartc-scheduler"



### SpinAgent.exe commands

| Command                   | Notes                                                               |
| ------------------------- | ------------------------------------------------------------------- |
| start                     | Start agent                                                         |
| --clusterUrl              | Spin Cluster agent scheduler's URL address.                         |
| --agentId                 | Agent's ID, agents must be pre-configured, through their actor      |

Notes...
- Agent calls cluster over REST, polling for status
- Agent captures output and send to Agent's actor for commands and data.  Logging is done in app-insights.
- Schema = "agent:domain/contractId"



### SmartC Commands...

| Command                   | Notes                                                               |
| ------------------------- | ------------------------------------------------------------------- |
| Run                       | Run SmartC executable                                               |
|                           |                                                                     |
| --auth {jwt}              | jwt ticket, used for bearer token for all API calls to the cluster  |
| --contractId {contractId} | Contract ID                                                         |
| --userId                  | User's ID                                                           |
| --clusterUrl              | Spin Cluster agent scheduler's URL address.                         |

Notes...
- Each "SmartC" has an Actor for configuration, status and data/activity logging.
- Schema = "smartc-exe:domain/contractId"



### SmartC contract
Contract must have resource list, one which is the location of the executable for the SmartC, plus version, and hash.


### Configuration
- Agent's configuration - handled by actor
- SmartC configuration - handled by actor
- Scheduler configuration - handled by actor









