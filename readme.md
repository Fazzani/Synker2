| | | |
|-|-|-|
| **Travis Master** | [![](https://travis-ci.org/Fazzani/Synker2.svg?branch=master)](https://travis-ci.org/Fazzani/Synker2) |
| **Travis Develop** | [![](https://travis-ci.org/Fazzani/Synker2.svg?branch=Develop)](https://travis-ci.org/Fazzani/Synker2) |
| **Release** | [![GitHub release](https://img.shields.io/github/release/Fazzani/Synker2.svg?style=for-the-badge)](https://github.com/Fazzani/Synker2/releases/latest) |
| **CodeBeat** | [![codebeat badge](https://codebeat.co/badges/d1235125-1d3c-4471-b5a4-a5a75f525b58)](https://codebeat.co/projects/github-com-fazzani-synker2-master) |
| **Azure DevOPS** | [![Build Status](https://henifazzani.visualstudio.com/Synker/_apis/build/status/Synker-ASP.NET%20Core-CI?branchName=master)](https://henifazzani.visualstudio.com/Synker/_build/latest?definitionId=11&branchName=master) |


[link SEO](https://github.com/angular/universal/tree/master/modules/aspnetcore-engine)

### linux env compatibility

- [ ] [Sockets](http://www.c-sharpcorner.com/article/building-a-tcp-server-in-net-core-on-ubuntu/) linux endpoint
- [ ] StdOut/stdErr for logging
- [ ] Install application files into system folders as /usr/data for app data, /etc/bin/ for exe and /var/log for logs
  
### Package nuget PlaylistLibrary

- dotnet pack hfa.PlaylistBaseLibrary.csproj /p:PackageVersion=1.0.0-beta -c Release
- dotnet nuget push "bin\Release\hfa.playlistbaselibrary.1.0.0-beta.nupkg" -k ux16b846ggi3wy8arhuyxaqx -s https://ci.appveyor.com/nuget/fazzani-22jw22ht3p0v/api/v2/package

Targets
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
