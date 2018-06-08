import { Component, OnInit, OnDestroy } from "@angular/core";
import { MatSnackBar } from "@angular/material";
import { PagedResult } from "../../types/common.type";
import { MessageService } from "../../services/message/message.service";
import { Message, MessageStatus, MessageTypeEnum } from "../../types/message.type";

@Component({
  selector: "messages",
  templateUrl: "./messages.component.html"
})
export class MessagesComponent implements OnInit, OnDestroy {
  messages: PagedResult<Message>;
  messageTypes = MessageTypeEnum;

  constructor(public snackBar: MatSnackBar, private messageService: MessageService) {}

  ngOnInit(): void {
    this.messageService.listByStatus([MessageStatus.None, MessageStatus.NotReaded], 0, 10).subscribe(res => {
      this.messages = res;
    });
  }

  markAsRead(message: Message): void {
    message.status = MessageStatus.Readed;
    this.messageService.update(message).subscribe(numerOfUpdates => {
      this.messages.results = this.messages.results.splice(this.messages.results.indexOf(message));
    });
  }

  markAllAsRead(): void {
    this.messages.results.forEach(m => this.markAsRead(m));
  }

  ngOnDestroy() {}
}
