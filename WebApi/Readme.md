﻿![alt text](https://github.com/Fazzani/Synker2/blob/master/WebClient/wwwroot/favicon-32x32.png?raw=true)
[![Build status](https://ci.appveyor.com/api/projects/status/9drbo0ty6whivq12?svg=true)](https://ci.appveyor.com/project/Fazzani/synker2)
### TODO
- [x] Generate new m3u file for TVH
- [x] Export Provider to Provider dynamiquement (ex: tvlist to m3u)
- [x] Auth [link1](https://blogs.msdn.microsoft.com/webdev/2017/04/06/jwt-validation-and-authorization-in-asp-net-core/ ) [link2](http://luizcarlosfaria.net/blog/jwt-no-asp-net-core-standalone/)
  - [ ] Authorize based policy and roles
  - [ ] token encrypt by certif
  - [ ] Custom Security Providers [link](https://stormpath.com/blog/store-protect-sensitive-data-dotnet-core)
  - [ ] Add filter for set the Principal from token
- [ ] Attach playlists to users (add user to playlists)
- [ ] Perf :
  - [ ] Compress response
  - [ ] Cache file response (Etag)
- [ ] EF [link](https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql/blob/master/README.md)
- [x] Gestion des exceptions
- [ ] Fix ToList (QueryProvider not working on where) in Push Method of M3uProvider when downloading a file
- [x] WebSockets [link1](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/websockets)
  - [ ] Fix Logger (RequestService problem)
  - [ ] Sockets filtred by user
  - [x] SSL sockets
  - [ ] Persist notificaton and send them after connection
- [ ] Xmltv services
  - [ ] Upload files
  - [ ] Config customs xmltv feeds from channels and by user
  - [ ] Purge uploaded files
  - [ ] Get Epg for XmltvCompnent by channel and dateTime range
- [ ] Swagger auth by token support