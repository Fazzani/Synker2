import { Resolve } from '@angular/router';
import { ActivatedRouteSnapshot } from '@angular/router';
import { Injectable } from '@angular/core';
import { MessageService } from '../../services/message/message.service';
import { MessageStatus } from '../../types/message.type';

@Injectable()
export class MessagesResolver implements Resolve<any> {
  constructor(private messageService: MessageService) {}

  resolve(route: ActivatedRouteSnapshot) {
    return this.messageService.listByStatus([MessageStatus.None, MessageStatus.NotReaded], 0, 10);
  }
}
