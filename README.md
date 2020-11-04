# Shaman server
C# solution for creating full functional room-based game server system. 

### Description
There are o lot of kinds of games, that require so-called room logic - when players join to isolated game spaces and have fun shooting each other or just chat and play words. Shaman is framework for creating these kinds of games.
Every Shaman deployment (independent of launch type you will choose) consists of two main parts
 - The first one is **Shaman** itself in one of following configurtions (we will call it Launcher): StandAlone Game, Pair, Separated Game and MM and Balanced Game and MM (they all described in following sections). It is the exactly thing you will download from this repo. It is usually launched using docker image.
 - The second one is **Bundle** - the part you need to code with yourself to create your game. It is a set of libraries with game-specific logic. After Shaman creates room for your players, all magic whatever you come up with comes into play. It can be FPS shooter game or something turn based or whatever. Shaman Launcher can download Bundle from internet or use it from local folder - you may setup it during launch time. Developing game with Shaman - is actually developing your Bundle using some Nuget packages and predefined interfaces. Shaman core may be black box for you.
 
Each Launcher operates with two types of servers
 - **Matchmaker** - this adds aggregates players, who want to join battle. Matchmaker groups players, creates room on one of game servers. Player receives «coordinates» of new room and becomes completely ready to join battle. 
 - **Game** - it is game server which hosts rooms with game logic. Game server receives CreateRoom request from Matchmaker, prepares new room and waits for players to come.
And Balanced Laucnher adds following server types to field
 - **Router** - this server is responsible for routing client between several matchmakers - for example in case of some geo regions.

### Launchers
Lets talk about Launcjers, mentioned above. The way to actually launch it is described little bit below in **Start** section.
 - **StandAlone Game** - this Launcher is used for simplest cases, than you do not need any matchmaking or geographic region based balancing. You just need to put your players into some room, which is open or create one if there is no one available. It does its work inside only one docker container on you server host.
 - **Separated Game and Matchmaker** - this Launcher you probably want to use then you need to group your players using some criterias. Players firstly go to matchmaker and after that they come into rooms grouped by some player properties or custom matchmaking logic which came from current Bundle. This launcher in runtime consists of two different containers - one for Matchmaker and one for Game Server. Each matchmaker can send players to several game servers, so your production environment may consist of one matchmaker and some game servers running on different hardware.
 - **Pair** - this launcher is similar to **Separated Game and Matchmaker** launcher, but it works in one container - you can use it in cases than you can share you hardware between Matchmaker and Game server. Also it is good to use locally during development process - you have to manage just one container.
 - **Balanced Game and Matchmaker** - in this case to **Separated Game and Matchmaker** deployment we add Router which can balance players between several matchmakers. It is next level of scaling than you need for example some geo regions and players come to matchmaker located in their region. Also this kind of deployment use such entity as Meta server - it is most complex launcher which can fit any scaling requirement.
Also while developing your Bundle you may use so-called Debug Server - it is fastest way to get into the game and to check your last code update.


### Frameworks and tools used for creating and deploying this solution:
 - .net Core 2.2 as main platform for all applications
 - Docker as infrastructure service, ansible as orchestration platform (you may find some samples of ansible scripts that you can use for deploying - see **Deployment** section)
 - MySQL as DB engine, used by Router to store its routing tables
 - [LiteNet](https://github.com/RevenantX/LiteNetLib) network layer (see Networking section)

### Plan of further development:
 - Fixing bugs and memory leaks (constantly)
 - Updating sample deployment scripts

### Networking

### Deployment

### Special thanks to:
*  [ivacbka](https://github.com/ivacbka) - for inspiration and involving me in this
