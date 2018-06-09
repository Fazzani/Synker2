import { Observable } from "rxjs";
import { map, retry, share } from "rxjs/operators";
import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
// import { RxWebsocketSubject } from "../../types/wsReconnectionSubject.type";
import { Message } from "../../types/message.type";
import { BaseService } from "../base/base.service";
import { environment } from "../../../environments/environment";
import { HubConnection, HubConnectionBuilder } from "@aspnet/signalr";
import { HUB_NOTIF } from "../../variables";
import { CommonService } from "../common/common.service";

@Injectable()
export class NotificationService extends BaseService {
  public messages: Observable<Message> = <Observable<Message>>{};
  private hubConnection: HubConnection;

  constructor(protected http: HttpClient, private commonService: CommonService) {
    super(http, "");
    this.hubConnection = new HubConnectionBuilder().withUrl(HUB_NOTIF).build();
    this.hubConnection
      .start()
      .then(() => this.commonService.info("Hub connection", "Connecté au hub"))
      .catch(error => this.commonService.displayError("Hub connection", "Problème de connexion au hub: " + error));

    // let subject = new RxWebsocketSubject(environment.base_api_url);

    // this.messages = subject.pipe(
    //   map(
    //     (response: MessageEvent | string): Message => {
    //       console.log("new message event ", response);
    //       if (this.IsJsonString((<MessageEvent>response).data)) {
    //         let data = JSON.parse((<MessageEvent>response).data);
    //         return <Message>{
    //           userName: <string>data.username,
    //           content: <string>data.message
    //         };
    //       } else {
    //         return <Message>{ content: <string>response };
    //       }
    //     }
    //   ),
    //   share(),);

    // subject.pipe(retry()).subscribe(
    //   function(e) {
    //     console.log(`Message from server: "${e}"`);
    //   },
    //   function(e) {
    //     console.log("Unclean close", e);
    //   },
    //   function() {
    //     console.log("Closed");
    //   }
    // );

    // subject.connectionStatus.subscribe(isConnected => {
    //   //textareaLog.disabled = sendMsgBtn.disabled = !isConnected;
    //   let msg = isConnected ? "Server connected" : "Server disconnected";
    //   console.log(msg);
    //   //addLogMessage(msg);
    // });
  }
}
