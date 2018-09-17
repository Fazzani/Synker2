export default class FirebaseNotification {
  date: Date;
  body: string;
  title: string;
  source: string;
  level: "info" | "warning" | "error";
}
