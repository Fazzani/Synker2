import { BehaviorSubject, Observable } from "rxjs";
import { map, retry, share } from "rxjs/operators";
import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Message } from "../../types/message.type";
import { BaseService } from "../base/base.service";
import { environment } from "../../../environments/environment";
import { HubConnection, HubConnectionBuilder } from "@aspnet/signalr";
import { CommonService } from "../common/common.service";
import { AuthService } from "../auth/auth.service";
import { AngularFireDatabase, AngularFireList } from '@angular/fire/database';
import FirebaseNotification from "../../types/firebase.type";

@Injectable({
  providedIn: 'root',
})
export class NotificationService extends BaseService {
  public messages: BehaviorSubject<Message> = new BehaviorSubject<Message>(null);
  private hubConnection: HubConnection | undefined;
  readonly hubName: string = 'hubs/notification';

  constructor(private db: AngularFireDatabase, protected http: HttpClient, private commonService: CommonService, private authService: AuthService) {
    super(http);
    console.log(`NotificationHub Url : ${environment.base_hub_url}${this.hubName}`);
    this.hubConnection = new HubConnectionBuilder().withUrl(`${environment.base_hub_url}${this.hubName}`, { accessTokenFactory: () => this.authService.getToken() }).build();

    //TODO: retry when failed // see implementation in this file history
    this.hubConnection
      .start()
      .then(() => this.commonService.info("Hub connection", "Connecté au hub"))
      .catch(error => this.commonService.displayError("Hub connection", "Problème de connexion au hub: " + error));

    this.hubConnection.on("SendMessage", (userName: string, content: string) => {
      const text = `New received message from ${userName}: ${content}`;
      console.log(text);
      this.messages.next(<Message>{
        userName: userName,
        content: content
      });
    });
  }

  public list(userId: number, limit: number = 10): AngularFireList<FirebaseNotification> {
    return this.db.list<FirebaseNotification>(`/notifications/${userId}`, ref => ref.limitToFirst(limit).orderByKey());
  }
  public count(): Observable<number> {
    return this.db.list<FirebaseNotification>('/notifications').valueChanges().pipe(map(x => x.length));
  }
}
