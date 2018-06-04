import { Subject, Observer, Observable } from 'rxjs';
import { WebSocketSubject, WebSocketSubjectConfig } from 'rxjs/observable/dom/WebSocketSubject';

/// we inherit from the ordinary Subject
export class RxWebsocketSubject<T> extends Subject<T> {
    private reconnectionObservable: Observable<number>;
    private wsSubjectConfig: WebSocketSubjectConfig<any>;
    private socket: WebSocketSubject<any>;
    private connectionObserver: Observer<boolean>;
    public connectionStatus: Observable<{}>;

    /// by default, when a message is received from the server, we are trying to decode it as JSON
    /// we can override it in the constructor
    defaultResultSelector = (e: MessageEvent) => {
        return JSON.parse(e.data);
    }

    /// when sending a message, we encode it to JSON
    /// we can override it in the constructor
    defaultSerializer = (data: any): string => {
        return JSON.stringify(data);
    }

    constructor(
        private url: string,
        private reconnectInterval: number = 5000,  /// pause between connections
        private reconnectAttempts: number = 10,  /// number of connection attempts

        private resultSelector?: (e: MessageEvent) => any,
        private serializer?: (data: any) => string,
    ) {
        super();

        /// connection status
        this.connectionStatus = new Observable((observer) => {
            this.connectionObserver = observer;
            })
            .share()
            .distinctUntilChanged();

        if (!resultSelector) {
            this.resultSelector = this.defaultResultSelector;
        }
        if (!this.serializer) {
            this.serializer = this.defaultSerializer;
        }

        /// config for WebSocketSubject
        /// except the url, here is closeObserver and openObserver to update connection status
        this.wsSubjectConfig = {
            url: url,
            closeObserver: {
                next: (e: CloseEvent) => {
                    this.socket = null;
                    this.connectionObserver.next(false);
                }
            },
            openObserver: {
                next: (e: Event) => {
                    this.connectionObserver.next(true);
                }
            }
        };
        /// we connect
        this.connect();
        /// we follow the connection status and run the reconnect while losing the connection
        this.connectionStatus.subscribe((isConnected) => {
            if (!this.reconnectionObservable && typeof (isConnected) == "boolean" && !isConnected) {
                this.reconnect();
            }
        });
    }

    connect(): void {
        this.socket = new WebSocketSubject(this.wsSubjectConfig);
        this.socket.subscribe(
            (m) => {
                this.next(m); /// when receiving a message, we just send it to our Subject
            },
            (error: Event) => {
                if (!this.socket) {
                    /// in case of an error with a loss of connection, we restore it
                    this.reconnect();
                }
            });
    }

    /// WebSocket Reconnect handling
    reconnect(): void {
        this.reconnectionObservable = Observable.interval(this.reconnectInterval)
            .takeWhile((v, index) => {
                return index < this.reconnectAttempts && !this.socket
            });
        this.reconnectionObservable.subscribe(
            () => {
                this.connect();
            },
            null,
            () => {
                /// if the reconnection attempts are failed, then we call complete of our Subject and status
                this.reconnectionObservable = null;
                if (!this.socket) {
                    this.complete();
                    this.connectionObserver.complete();
                }
            });
    }

    /// sending the message
    send(data: any): void {
        this.socket.next(this.serializer(data));
    }
}
