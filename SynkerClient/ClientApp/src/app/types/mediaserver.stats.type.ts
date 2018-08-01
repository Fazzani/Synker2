export class MediaServerStats {
  public Os: Os;
  public Cpu: Cpu;
  public Mem: Mem;
  public Net: Net;
  public Nodejs: Nodejs;
  public Clients: Clients;
}

export class Os {
  public Arch: string;
  public Platform: string;
  public Release: string;
}
export class Cpu {
  public Num: number;
  public Load: number;
  public Model: string;
  public Speed: number;
}
export class Mem {
  public Totle: number;
  public Free: number;
}
export class Net {
  public Inbytes: number;
  public Outbytes: number;
}
export class Nodejs {
  public Uptime: number;
  public Version: string;
  public Mem: Mem1;
}
export class Mem1 {
  public Rss: number;
  public HeapTotal: number;
  public HeapUsed: number;
  public External: number;
}
export class Clients {
  public Accepted: number;
  public Active: number;
  public Idle: number;
  public Rtmp: number;
  public Http: number;
  public Ws: number;
}
