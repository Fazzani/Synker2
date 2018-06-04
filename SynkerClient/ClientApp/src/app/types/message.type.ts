﻿import { QueryListBaseModel } from "./common.type";

/**
 * Message Type
 * @description Message type.
 */
export class Message {
    
    id: string;
    content: string;
    type: string;
    timeStamp: Date;
    status: MessageStatus;
    messageType: MessageTypeEnum;
    userName: string;

    public get isUnreaded(): boolean {
        return this.status < 2;
    }
}
/**
 * MessageStatus (0: None, 1: NotReaded, 2: Readed).
 * @description MessageStatus (0: None, 1: NotReaded, 2: Readed).
 */
export enum MessageStatus {
    None = 0,
    NotReaded = 1,
    Readed = 2
}
export class MessageQueryModel extends QueryListBaseModel{
    public MessageStatus: MessageStatus[];
}

export enum MessageTypeEnum {
    None = 0,
    Ping,
    START_SYNC_MEDIAS,
    END_SYNC_MEDIAS,
    START_CREATE_CONFIG,
    END_CREATE_CONFIG,
    START_SYNC_EPG_CONFIG,
    END_SYNC_EPG_CONFIG,
    EXCEPTION,
    START_PUSH_XMLTV,
    END_PUSH_XMLTV,
    DIFF_PLAYLIST
}