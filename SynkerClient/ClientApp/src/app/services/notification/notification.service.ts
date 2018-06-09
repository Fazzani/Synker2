import { BehaviorSubject } from "rxjs";
import { map, retry, share } from "rxjs/operators";
import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
// import { RxWebsocketSubject } from "../../types/wsReconnectionSubject.type";
import { Message } from "../../types/message.type";
import { BaseService } from "../base/base.service";
import { environment } from "../../../environments/environment";
import { HubConnection, HubConnectionBuilder } from "@aspnet/signalr";
import { CommonService } from "../common/common.service";

@Injectable()
export class NotificationService extends BaseService {
  public messages: BehaviorSubject<Message> = new BehaviorSubject<Message>(null);
  private hubConnection: HubConnection;

  constructor(protected http: HttpClient, private commonService: CommonService) {
    super(http, "");
    this.hubConnection = new HubConnectionBuilder().withUrl(`${environment.base_hub_url}notification`).build();

    this.hubConnection
      .start()
      .then(() => this.commonService.info("Hub connection", "Connecté au hub"))
      .catch(error => this.commonService.displayError("Hub connection", "Problème de connexion au hub: " + error));

    this.hubConnection.on("ReceiveMessage", (userName: string, content: string) => {
      const text = `New received message from ${userName}: ${content}`;
      console.log(text);
      this.messages.next(<Message>{
        userName: userName,
        content: content
      });
    });
  }
}
