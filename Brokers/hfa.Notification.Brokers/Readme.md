﻿[![Build status](https://ci.appveyor.com/api/projects/status/jskpfdwhep4s3b4r?svg=true)](https://ci.appveyor.com/project/Fazzani/synker2-0j10q)

[![](https://images.microbadger.com/badges/version/synker/broker.svg)](https://microbadger.com/images/synker/broker "Get your own version badge on microbadger.com")
[![](https://images.microbadger.com/badges/image/synker/broker.svg)](https://microbadger.com/images/synker/broker "Get your own image badge on microbadger.com")
## Context

- Notification app service based on RabbitMQ

    1. RabbitMQ as broker mounted on cluster
    2. HAProxy as Reverse proxy and LB

## TODO

- Templating mails
- Routing by header

### DOCKER
- **Redis** for caching
- Creating **Linux service** into the Dockerfile AND run it
- Creating **user/group** for service
- Creating an **ENTRYPOINT/CMD** for Dockerfile service
- Docker-compose for the **DEV env** and another for **Prod**
- **Rex-ray** storage driver test
- Clusturing the **application Stack** and scale application test
- Resoudre le pb des **appsettings** by env

### RabbitMQ

- Test Persistence 
- Header Message
- Ack AND Nack scenarios

## Various
- [Create Deamon app and service](http://pmcgrath.net/running-a-simple-dotnet-core-linux-daemon)

- [DockerFile to build multiple project dotnet core](http://www.ben-morris.com/using-docker-to-build-and-deploy-net-core-console-applications/)