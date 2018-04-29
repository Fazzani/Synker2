export class Host {
    public id: number;
    public name: string;
    public address: string;
    public authentication: Authentication;
    public port: string;
    public adressUri: string;
    public comments: string;
    public enabled: boolean;
}

export class Authentication {
    public certPath: string;
    public username: string;
    public password: string;
}