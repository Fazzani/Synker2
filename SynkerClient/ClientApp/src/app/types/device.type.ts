export interface Device {
  name: string;
  pushEndpoint: string;
  pushP256DH: string;
  pushAuth: string;
  expirationTime: Date;
}
export interface DeviceModel extends Device {
  id: number;
  createdDate: Date;
  updatedDate: Date;
}
