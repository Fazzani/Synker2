import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs/Rx';
import { WebSocketService } from '../websocket/websocket.service';
import { Message } from '../../types/message.type';

import * as variables from "../../variables";

@Injectable()
export class NotificationService {
    public messages: Subject<Message>;

    constructor(wsService: WebSocketService) {
        
        this.messages = <Subject<Message>>wsService
            .connect(variables.BASE_WS_URL)
            .map((response: MessageEvent): Message => {
                
                let data = JSON.parse(response.data);
                return <Message>{
                    author: <string>data.author,
                    content: <string>data.message
                }
            });
    }
}