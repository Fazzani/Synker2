/**
 * Message Type
 * @description Message type.
 */
export interface Message {
    id: string;
    content: string;
    type: string;
    timeStamp: Date;
    status: MessageStatus
    author: string,
    objectContent:any
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
export class MessageQueryModel {
    public MessageStatus: MessageStatus[];
    public PageSize: number;
    public PageIndex: number;
}
