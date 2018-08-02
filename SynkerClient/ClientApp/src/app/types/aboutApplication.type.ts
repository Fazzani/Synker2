// Generated by https://quicktype.io

export class AboutApplication {
  Author:          string;
  ApplicationName: string;
  LastUpdate:      string;
  Comments:        null;
  Version:         string;
  License:         string;
  OSVersion:       OSVersion;
  EnvVersion: Version;

  static Default: AboutApplication = <AboutApplication>{
    ApplicationName: "Synker",
    Author: "Synker",
    LastUpdate: new Date().toLocaleTimeString(),
    License: "MIT",
    Version: "1.0.0-beta"
  };
}

export interface Version {
  Major:         number;
  Minor:         number;
  Build:         number;
  Revision:      number;
  MajorRevision: number;
  MinorRevision: number;
}

export interface OSVersion {
  Platform:      number;
  ServicePack:   string;
  Version:       Version;
  VersionString: string;
}

