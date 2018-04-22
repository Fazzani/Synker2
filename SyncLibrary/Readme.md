[![Build status](https://ci.appveyor.com/api/projects/status/nnk28cpkyyh1tot4?svg=true)](https://ci.appveyor.com/project/Fazzani/synker2-fmqp0)

## Goals
<pre>Créer un service système permettant la synchrnisation entre des providers différents. 
Il faut que ce soit modulable, configurable et autonome.
Il faut que ce soit configurable via un fichier de config (json de préférence).
Il faut qu'on puisse injecter une config via la CLI (sans que le fichier de config soit présent sur le disque dur). ça sera utile dans le cas d'un appel par la web API
</pre>
<pre><p>les providers qu'on peut avoir à titre d'exemple : un serveur Elastic, un serveur TvHeadend, une base de données 
un fichier ou une url. le providers doivent être injectés par DLL, pour que ce soit extensible</p></pre>
<pre>La communication avec le monde extérieur ça se passe via des API ou des WebHooks</pre>
---------------------------
## Notes
 - dotnet publish -r linux-arm -o Y:\SyncPlaylist
 - [Commande nuget package](https://github.com/gsscoder/commandline) used
#### TODO
- [ ] Appveyor deploy
- [ ] Diff to update properties only (epg, group, position, logo, urls for mutli) on (Elastic => TVH)
- [ ] Use FormatableString for to string the different file's formats
- [ ] Move mapping cache_filter to a simple file config
- [ ] Load dynamicaly all handlers
- [ ] Parallelize update medias
- [x] WebSockets when something started
- [x] Format all medias (Epg, Names, Group, etc)
- [x] Sync from local or remote files to Elastic
- [x] Add lang for epg and treat it on handler
- [x] Parametrer le script (-f force update, )
- [x] Fix verbose log
- [x] Put all Logs to elastic logstash
- [x] Put EPG referentiel to Elastic
- [x] Mapping TvgMedia to Elastic
      
## Docker daemon api

* [Docker Api Reference][docker_api_ref]
* Display Docker config view

```sh
env | grep DOCKER
```

* Generating cert key.pfx for certification connection type:

```sh
# Display docker certif path
echo $DOCKER_CERT_PATH
# Generate cert pfx
openssl pkcs12 -export -inkey key.pem -in cert.pem -out key.pfx -certfile ca.pem
```

#### BUGS

- [ ] Fix ID EPG conflicts
- [ ] Fix log encoding (log4net UDP, logstash)
- [ ] Logger bash scripts with logger (man logger) to syslog (/var/log/xxx) and stash them

<pre><code>
 output.elasticsearch:
  # Array of hosts to connect to.
  hosts: ["151.80.235.155:9201"]

  # Optional protocol and basic auth credentials.
  #protocol: "https"
  username: "logstash"
  password: "logstash"
 </code></pre>

#### Install certif

<pre><code>
New-SelfSignedCertificate -Type Custom -KeySpec Signature `
>> -Subject "CN=application_dev" -KeyExportPolicy Exportable `
>> -HashAlgorithm sha256 -KeyLength 2048 `
>> -CertStoreLocation "Cert:\CurrentUser\My" -KeyUsageProperty All -KeyUsage DataEncipherment -KeyFriendlyName dev -FriendlyName dev_app
 </code></pre>

https://www.elastic.co/guide/en/elasticsearch/client/net-api/current/writing-analyzers.html

https://qbox.io/blog/applying-elasticsearch-analyzers

https://hassantariqblog.wordpress.com/2016/09/22/elastic-search-search-document-using-nest-in-net/

### Elastic request for all xmltv_id
<pre><code>
GET filebeat*/_search
{
  "size": 1000,
  "from": 0,
  "_source": [
    "xmltv_id",
    "site_id"
  ],
  "query": {
    "bool": {
      "filter": [{
          "exists": {
            "field": "xmltv_id"
          }}],
      "must": [
        {
          "range": {
            "@timestamp": {
              "gte": "now-7d",
              "lt": "now"
            }
          }
        }
      ]
    }
  },
  "aggs": {
    "unique_ids": {
      "terms": {
        "field": "xmltv_id.keyword"
      }
    }
  }
}
</code></pre>

[docker_api_ref]:https://docs.docker.com/engine/api/v1.28