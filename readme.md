﻿[![Build status](https://ci.appveyor.com/api/projects/status/369mb12g7tgnut2i?svg=true)](https://ci.appveyor.com/project/Fazzani/synker2)

[link SEO](https://github.com/angular/universal/tree/master/modules/aspnetcore-engine)
### CI & DEPLOY

- [ ] Deploy by travis/appveyor [link appveyor](https://raw.githubusercontent.com/NLog/NLog/master/appveyor.yml)
- [ ] CI
- [ ] Config SSL encrypt [link1](https://certbot.eff.org/#debianstretch-other)
- [ ] Config smtp [link1](https://wiki.debian-fr.xyz/Configuration_d%27un_serveur_mail_avec_Postfix)
 
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



Objectifs
=========
- [ ] Linq query
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

Technical targets
=================
- [x] Create collection channels with : IComparable, IEnumerable, Indexer, Equals, hashCode.
- [x] Object validation
- [ ] Serialize and deserialize channels list in file with permissions, buffering and crypto
- [ ] Manipulate channels with linq (group, sort, join, etc...)
- [x] Trace all the application.
- [ ] Secure playlists exchange between two entities (asym/sym crypto, hash and certif)
- [x] Perfermance metrics : PerformanceCounter, Stopwatch
- [ ] Sign the library
- [x] Diff 2 files m3u
- [ ] auto loader for providers and formatters (plugins by reflection)
- [ ] Encryp with certif, convert to Base64 and verification
- [ ] Linq to m3u provider
- [ ] Microservices architecture

NOTES
=====

1) Providers

Save and load data

**Types** 

- FileProvider (uri, local, crypto)
- BaseProvider			
- WebServiceProvider
				
2) Formaters
Serialiser, déserialiser : 

   - M3u
   - tvlist
   - Binary (objets)
   - Json
   - Xml
		 
Sync entre 2 providers in manager

3) EPG (Load, Match, etc)
