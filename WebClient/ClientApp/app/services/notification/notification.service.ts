import { Injectable } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { Observable, Subject } from 'rxjs/Rx';
import { RxWebsocketSubject } from '../../types/wsReconnectionSubject.type';
import { Message } from '../../types/message.type';
import * as variables from "../../variables";
import { BaseService } from "../base/base.service";

@Injectable()
export class NotificationService extends BaseService {
    public messages: Observable<Message>;

    constructor(protected http: HttpClient) {
        super(http, '');

        let subject = new RxWebsocketSubject(variables.BASE_WS_URL);

        this.messages = subject.map((response: MessageEvent | string): Message => {
            console.log("new message event ", response);
            if (this.IsJsonString((<MessageEvent>response).data)) {
                let data = JSON.parse((<MessageEvent>response).data);
                return <Message>{
                    userName: <string>data.username,
                    content: <string>data.message
                }
            }
            else {
                return <Message>{ content: <string>response };
            }
        }).share();

        subject.retry().subscribe(
            function (e) {

                console.log(`Message from server: "${e}"`);
            },
            function (e) {
                console.log('Unclean close', e);
            },
            function () {
                console.log('Closed');
            }
        );

        subject.connectionStatus.subscribe((isConnected) => {
            //textareaLog.disabled = sendMsgBtn.disabled = !isConnected;
            let msg = isConnected ? 'Server connected' : 'Server disconnected';
            console.log(msg);
            //addLogMessage(msg);
        });

    }
}