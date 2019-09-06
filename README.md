Shaman server - is C# solution for creating full functional game server system.

### Description
Solution consists of following modules:
 - **Router** - this module is responsible for routing client, using its application version to forward it correct environment (Shaman.Router project in solution).
 - **Backend** - backend is responsible for serving all requests related to player profiles - different buy, upgrade operations, player’s attributes changes, authentication logic etc (Shaman.Backend project in solution)
 - **Matchmaker** - this module aggregates players, who want to join battle. Matchmaker groups players, adds bots if needed, and after that creates room on one of connected game servers. Player receives  «coordinates» of new room and becomes completely ready to join battle. (Shaman.Matchmaker project in solution)
 - **Game** - it is game server which hosts rooms with game logic. Game server receives CreateRoom request from Matchmaker, prepares new room and waits for players to come. After minimal amount needed for battle start was entered the room - battle starts. (Shaman.Game project in solution)
 - **Client** - bunch of client classes (Unity client is also present) incapsulating most of routing and authentication functionality, giving simple interface to integrate into your game. (Shaman.Client project in solution)

### Solution based on this platform can be represented like this:
<img src="https://monosnap.com/image/P9RDa66HBSWMZpFb8YqWymbaTKJdHx"/>

### Frameworks and tools used for creating and deploying this solution:
 - .net Core 2.2 as main platform for all applications
 - Docker as infrastructure service, ansible as orchestration platform (deployment scripts coming soon, see Deploy section)
 - MySQL as DB engine, used by default for storing all data (see Storing section)
 - Redis as cache engine to store players’ profiles after they were received from database
 - [Hazel](https://github.com/willardf/Hazel-Networking) network layer (see Networking section)

### To launch your own game using this solution, you will have to do following things:
 - Create all Requests, Responses and Events objects needed for your game;
 - Create tables in database to store additional data;
 - Create actions inside controllers of Backend project to process requests;
 - Create Room logic creating IGameModeControllerFactory and IGameModeController implementations;
 - Create send Requests and Events logic inside your client;
 - Create your personal deployment scripts using your favorite infrastructure services.

### Projects, which are already "on live" based on this solution:
 - [Metal Sky](https://play.google.com/store/apps/details?id=com.redclusterstudio.metalsky&hl=ru)

### Plan of further development:
 - Fixing bugs and memory leaks (constantly)
 - Creating sample deployment scripts using docker, ansible and AWS as hoster
 - Stabilizing architecture - finding and fixing «bad design» parts
 - Adding some new network adapters to test performance and choose best 
 - Creating small sample project with «one-click» install to present Shaman platform at most fastest way
 - Adding block of functionality with value - purchase validators, game logic features, infrastructure services etc
More detailed release plan will be published here later

### Points of customization of solution without updating core code
 - Creating new serializers implementing ISerializer interface
 - Creating new Network adapters implementing IReliableSock interface
 - Deployment scripts

### Data Storing

### Networking

### Deployment

### Special thanks to:
*  [ivacbka](https://github.com/ivacbka) - for inspiration and involving me in this
