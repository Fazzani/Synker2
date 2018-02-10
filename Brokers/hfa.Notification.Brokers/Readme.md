[![Build status](https://ci.appveyor.com/api/projects/status/jskpfdwhep4s3b4r?svg=true)](https://ci.appveyor.com/project/Fazzani/synker2-0j10q)

[![](https://images.microbadger.com/badges/version/synker/broker.svg)](https://microbadger.com/images/synker/broker "Get your own version badge on microbadger.com")
[![](https://images.microbadger.com/badges/image/synker/broker.svg)](https://microbadger.com/images/synker/broker "Get your own image badge on microbadger.com")
## Context

- Notification app service based on RabbitMQ

    1. RabbitMQ as broker mounted on cluster
    2. HAProxy as Reverse proxy and LB


### Command to launch broker container 

```
docker build  -t synker/broker .
```

``` SHELL
docker run -v "$(pwd)/appsettings.Prod.json":/app/appsettings.json \
 -e "ASPNETCORE_ENVIRONMENT=Prod" \
 -v $(pwd):/app/Logs \
 -itd --rm --name broker synker/broker:latest
```

``` SHELL
docker exec -it broker ls -l /app
```

**Remove all containers**

```
 docker rm $(docker ps -a -q)
```

## TODO

- Templating mails
- Routing by header

### DOCKER
- [ ] **Redis** for caching
- [ ] Creating **Linux service** into the Dockerfile AND run it
- [ ] Creating **user/group** for service
- [ ] Creating an **ENTRYPOINT/CMD** for Dockerfile service
- [ ] Docker-compose for the **DEV env** and another for **Prod**
- [ ] **Rex-ray** storage driver test
- [ ] Clusturing the **application Stack** and scale application test
- [ ] Resoudre le pb des **appsettings** by env
- [ ] Log stdout / stderr

### RabbitMQ

- Test Persistence 
- Header Message
- Ack AND Nack scenarios

## Various

- [Create Deamon app and service](http://pmcgrath.net/running-a-simple-dotnet-core-linux-daemon)

- [DockerFile to build multiple project dotnet core](http://www.ben-morris.com/using-docker-to-build-and-deploy-net-core-console-applications/)
