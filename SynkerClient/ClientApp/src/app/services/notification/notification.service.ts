import { Observable } from "rxjs";
import { map, share } from "rxjs/operators";
import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { BaseService } from "../base/base.service";
import { environment } from "../../../environments/environment";
import { HubConnection, HubConnectionBuilder } from "@aspnet/signalr";
import { CommonService } from "../common/common.service";
//import { AuthService } from "../auth/auth.service";
import { AngularFireDatabase, AngularFireList } from "@angular/fire/database";
import FirebaseNotification from "../../types/firebase.type";

@Injectable({
  providedIn: "root"
})
export class NotificationService extends BaseService {
  private hubConnection: HubConnection | undefined;
  readonly hubName: string = "hubs/notification";

  constructor(private db: AngularFireDatabase,
    protected http: HttpClient,
    private commonService: CommonService
    //private authService: AuthService
  ) {
    super(http);
    console.log(`NotificationHub Url : ${environment.base_hub_url}${this.hubName}`);
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(`${environment.base_hub_url}${this.hubName}`, {
        accessTokenFactory: () => null//this.authService.getToken()
      })
      .build();

    //TODO: retry when failed // see implementation in this file history
    this.hubConnection
      .start()
      .then(() => this.commonService.info("Hub connection", "Connecté au hub"))
      .catch(error => this.commonService.displayError("Hub connection", "Problème de connexion au hub: " + error));
  }

  public list(userId: number, limit: number = 10): AngularFireList<FirebaseNotification> {
    return this.db.list<FirebaseNotification>(`/notifications/${userId}`, ref => ref.limitToFirst(limit).orderByChild('Order'));
  }

  public count(userId:number): Observable<number> {
    return this.db
      .list<FirebaseNotification>(`/notifications/${userId}`)
      .valueChanges()
      .pipe(map(x => x.length), share());
  }

  public remove(userId: number, key: string) {
    this.db.object<FirebaseNotification>(`/notifications/${userId}/${key}`).remove();
  }

  public removeAll(userId: number) {
    this.db.object<FirebaseNotification>(`/notifications/${userId}`).remove();
  }
}
