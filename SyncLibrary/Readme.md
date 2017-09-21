# Notes
 - dotnet publish -r linux-arm -o Y:\SyncPlaylist
 - OCTOPUS deploy
#### TODO
- [ ] (API) Generate new m3u file for TVH
- [ ] Diff to update properties only (epg, group, position, logo, urls for mutli) on (Elastic => TVH)
- [ ] Use FormatableString for to string the different file's formats
- [ ] Move mapping cache_filter to a simple file config
- [ ] WebSockets when something started
- [ ] Load dynamicaly all handlers
- [x] Format all medias (Epg, Names, Group, etc)
- [x] Sync from local or remote files to Elastic
- [x] Add lang for epg and treat it on handler
- [x] Parametrer le script (-f force update, )
- [x] Fix verbose log
- [x] Put all Logs to elastic logstash
- [x] Put EPG referentiel to Elastic
- [x] Mapping TvgMedia to Elastic

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