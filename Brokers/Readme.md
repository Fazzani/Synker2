[![Build status](https://ci.appveyor.com/api/projects/status/jskpfdwhep4s3b4r?svg=true)](https://ci.appveyor.com/project/Fazzani/synker2-0j10q)

**Travis ci**

- Master [![](https://travis-ci.org/Fazzani/Synker2.svg?branch=master)](https://travis-ci.org/Fazzani/Synker2)
- Develop [![](https://travis-ci.org/Fazzani/Synker2.svg?branch=Develop)](https://travis-ci.org/Fazzani/Synker2)
## Context

- Notification app service based on RabbitMQ

    1. RabbitMQ as broker mounted on cluster
    2. HAProxy as Reverse proxy and LB


### Command to launch broker container 

``` SHELL
docker build  -t synker/broker .
```

``` SHELL
docker run -v "$(pwd)/appsettings.Prod.json":/home/synker/appsettings.json \
 -e "ASPNETCORE_ENVIRONMENT=Prod" \
 -v $(pwd):/home/synker/Logs \
 -itd --rm --name broker synker/broker:latest
```

``` SHELL
docker exec -it broker ls -l /app
```

**Remove all containers**

``` SHELL
 docker rm $(docker ps -a -q)
```

## TODO

- Templating mails
- Routing by header

### DOCKER
- [ ] Travis CI/CD by application 
  - [ ] WebClient
  - [ ] WebApi
  - [ ] Batch
  - [ ] Broker
- [ ] **appsettings.json bind to volume by environment**
- [ ] CI/CD appveyor config
- [ ] Creating **Linux service** into the Dockerfile AND run it
- [ ] Creating an **ENTRYPOINT/CMD** for Dockerfile service (display version for example)
- [ ] Docker-compose for the **DEV env** and another for **Prod**
- [ ] **Rex-ray** storage driver test
- [ ] Clusturing the **application Stack** and scale application test
- [ ] Log stdout / stderr
- [ ] **Redis** for caching
- [x] Creating **user/group** for service
- [x] Resoudre le pb des **appsettings** by env

### RabbitMQ

- Test Persistence 
- Header Message
- Ack AND Nack scenarios

## Various

- [Create Deamon app and service](http://pmcgrath.net/running-a-simple-dotnet-core-linux-daemon)

- [DockerFile to build multiple project dotnet core](http://www.ben-morris.com/using-docker-to-build-and-deploy-net-core-console-applications/)
- [HashiCorp Vault](https://www.vaultproject.io/intro/getting-started/install.html)
- Azure Vault
- [Store configuration data using Docker Configs](https://docs.docker.com/engine/swarm/configs/)
- [DockerFile Dotnet Core Console Application](http://www.ben-morris.com/using-docker-to-build-and-deploy-net-core-console-applications/)
- [socket with dotnet Core application](http://www.c-sharpcorner.com/article/building-a-tcp-server-in-net-core-on-ubuntu/)
