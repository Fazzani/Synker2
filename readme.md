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

Functional targets
===================
- [x] Pull playlist        
- [x] Push playlist
- [x] Diff 2 playlist => new entries and deleted entries
- [ ] Group by tag channels
- [ ] Match with EPG
- [ ] Auto-correct channel name
- [ ] Auto match pictures
- [ ] Sort channel
- [ ] Generate EPG config file (WebGrab++ format) channel with time offset
- [ ] Formatters, parsers (m3u, json, tvlist), support (file, url, webservice)
- [ ] Tvheadend provider

Tvheadend provider
==================
- [x] Select simple object
- [ ] Select anonymous object
- [ ] Select simple property
- [x] OrderBy asc
- [ ] OrderBy desc 
- [x] Take, Skip
- [ ] Where expression
- [ ] SubQuery
- [ ] GroupBy
- [ ] Agregation (max, min, distinct, avg, etc..)
- [ ] Mapping (muxes, services, channels, epg, etc...)

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

