Visit our [**Discord**](https://discord.gg/FjaYVjFe)

# Shaman server
C# solution for creating a full functional room-based game server system. 

### Description
There are a lot of kinds of games that require so-called room logic - when players join isolated game spaces and have fun shooting each other or just chating. **Shaman** is a framework for creating these kinds of games.
Every **Shaman** deployment consists of two main parts
 - The first one is **Shaman** itself. It is the core of server and the exact thing you acquire from this repo. It is usually launched using docker image or may be launched as simple application.
 - The second one is **Bundle** - the part you need to code with yourself to create your game. It is a set of libraries with game-specific logic. After Shaman creates room for your players, all magic whatever you come up with comes into play. It can be an FPS shooter game or something turn based or whatever. Shaman Server can download Bundle from the Internet or use it from a local folder - you may setup it during launch time. Developing a game with Shaman - is actually developing your Bundle using some Nuget packages and predefined interfaces. Shaman core may be black box for you.

### Frameworks and tools used for creating and deploying this solution:
 - .net6.0 as main platform for all applications
 - Docker as infrastructure service
 - [LiteNet](https://github.com/RevenantX/LiteNetLib) or WebSocket network layer (see Networking section)

### Networking
The **Shaman** solution was designed to use external network libraries on transport level. Current version uses LiteNet or WebSockets. To use another one you need to create an implementation of ITransportLayer interface using any network layer you want. 

### Deployment
The simplest way to deploy **Shaman** is to use docker to pull and run **Shaman** itself and put your bundle to an available place. Docker Hub contains up-to-date **Shaman** image. But you can build your own one using sample scripts from the Deploy folder. You can find scripts for docker images building and for deployment using Ansible - it is extremely helpful when you need to update your servers on several hardware hosts.

### Create your first server
Let's create a simple server - it actually does nothing, but it really cool visualizes a workflow. You will need installed Docker for this demo.
1. Create new project in your favourite IDE (Visual Studio or Rider)
2. Add to project Nuget package ""
3. Add these classes to your project
3. Paste the following code to your Program.cs file
4. Run your project
5. Your **Shaman** server is online now and is ready to accept Clients!
6. Download this sample Unity package, import it to Unity, run it and you will see logs, saying that you are connected to your server

### Start with Docker
Let's start a server based on Docker image. We will use our Test Bundle which will be downloaded from here.
- Launch this command in command prompt and your server will be ready to accept players
```docker 
docker run -p 23452:23452/udp --name=game-standalone -e LauncherSettings__BundleUri=https://github.com/shamangamesolution/Samples/releases/download/v1.12-beta1/test-bundle-v1.12-beta1.zip docker.pkg.github.com/shamangamesolution/shaman.server/shaman.server.game.standalone:1.12.1

Parameters
"-p" - port mapping, UDP port 23452 of container is mapped to port 23452 of your host
"--name" - container name
"-e LauncherSettings__BundleUri" - URL of archive with test bundle to launch server with
"docker.pkg.github.com..." - image name from Docker Hub repository
```
- Check your server with Unity the same way as in previous section.



