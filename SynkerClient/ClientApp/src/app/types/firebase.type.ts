export default class FirebaseNotification {
  Date: Date;
  Body: string;
  Title: string;
  Source: string;
  Level: "info" | "warning" | "error";
}

export class FirebasePlaylistHealthState {
  IsOnline:boolean;
  Id:number;
  Name:string;
  MediaCount:number;
}
