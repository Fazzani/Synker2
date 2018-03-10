| | ||
| --- | --- | -- |
| **Appveyor** | [![Build status](https://ci.appveyor.com/api/projects/status/hwigxlp50tlut2ws?svg=true)](https://ci.appveyor.com/project/Fazzani/synker2-8v1uw) |[![Build history](https://buildstats.info/appveyor/chart/Fazzani/synker2-8v1uw)](https://ci.appveyor.com/project/dustinmoris/ci-buildstats/history)|
| **Travis Master** | [![](https://travis-ci.org/Fazzani/Synker2.svg?branch=master)](https://travis-ci.org/Fazzani/Synker2) |
| **Travis Develop** | [![](https://travis-ci.org/Fazzani/Synker2.svg?branch=Develop)](https://travis-ci.org/Fazzani/Synker2) |
| **Release** | [![GitHub release](https://img.shields.io/github/release/Fazzani/Synker2.svg?style=for-the-badge)](https://github.com/Fazzani/Synker2/releases/latest) |

[link SEO](https://github.com/angular/universal/tree/master/modules/aspnetcore-engine)

### linux env compatibility

- [ ] [Sockets](http://www.c-sharpcorner.com/article/building-a-tcp-server-in-net-core-on-ubuntu/) linux endpoint
- [ ] StdOut/stdErr for logging
- [ ] Install application files into system folders as /usr/data for app data, /etc/bin/ for exe and /var/log for logs
  
### Package nuget PlaylistLibrary

- dotnet pack hfa.PlaylistBaseLibrary.csproj /p:PackageVersion=1.0.0-beta -c Release
- dotnet nuget push "bin\Release\hfa.playlistbaselibrary.1.0.0-beta.nupkg" -k ux16b846ggi3wy8arhuyxaqx -s https://ci.appveyor.com/nuget/fazzani-22jw22ht3p0v/api/v2/package

Objectifs
=========
- [ ] PDB file test debug a dll
- [ ] CodeDom
- [ ] l'Objective 2.6: Manage the object life cycle à relire attentivement
- [ ] Encryption sénario entre 2 app (Asym/Sym)
- [ ] link to see https://msdn.microsoft.com/en-us/library/system.object.gethashcode(v=vs.110).aspx
- [ ] Permissions/CAS
- [x] TraceSource
- [x] StringReader/StringWriter
- [x] Threading and parallel
- [x] ByteConverter : You should never create an instance of a ByteConverter. Instead, call the GetConverter method of TypeDescriptor.
- [x] System.ComponentModel.DataAnnotations.IValidatableObject
- [x] performancecountercategory
- [ ] SecureString, ProtectedData, ProtectedMemory

#### DEPLOY PROD Config
###### APACHE2 CONFIG :/etc/apache2/sites-enabled# sudo nano 000-default.conf
<pre>
<code>
<VirtualHost *:80>
        ProxyPreserveHost On
        #ProxyRequests Off
        #RewriteEngine On
        ProxyPass / http://0.0.0.0:56800/
        ProxyPassReverse / http://0.0.0.0:56800/
        ServerAdmin webmaster@localhost
        ServerName api.synker.ovh
        DocumentRoot /home/synker/WebApi
        ErrorLog ${APACHE_LOG_DIR}/errorSynkerApi.log
        CustomLog ${APACHE_LOG_DIR}/access.log combined
</VirtualHost>
<VirtualHost *:80>
        ProxyPreserveHost On
        #ProxyRequests Off
        #RewriteEngine On
        ProxyPass / http://0.0.0.0:56801/
        ProxyPassReverse / http://0.0.0.0:56801/
        ServerName synker.ovh
        ServerAlias www.synker.ovh
        ServerAdmin webmaster@localhost
        DocumentRoot /home/synker/WebClient
        ErrorLog ${APACHE_LOG_DIR}/errorSynkerClient.log
        CustomLog ${APACHE_LOG_DIR}/access.log combined
</VirtualHost>
</code>
</pre>
