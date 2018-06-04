export class XtreamPanel {
    public user_info: User_Info;
    public server_info: Server_Info;
    public categories: Categories;
    public available_channels: Available_Channels;
}

export class Server_Info {
    public url: string;
    public port: string;
    public rtmp_port: string;
    public timezone: string;
    public time_now: Date;
}

export class User_Info {
    public username: string;
    public password: string;
    public auth: number;
    public status: string;
    public expirationDate: string;
    public isTrial: boolean;
    public active: boolean;
    public createdDate: string;
    public max_connections: string;
    public allowed_output_formats: string[];
}

export class Categories {
    public live: Live[];
}

export class Live {
    public categoryId: number;
    public category_name: string;
    public parent_id: number;
}

export class Available_Channels {
    public channels: Channels[];
}
export class Channels {
    public num: number;
    public name: string;
    public stream_type: string;
    public type_name: string;
    public streamId: number;
    public stream_icon: string;
    public epg_channel_id: string;
    public added: string;
    public category_name: string;
    public categoryId: number;
    public series_no: string;
    public live: string;
    public container_extension: string;
    public custom_sid: string;
    public tv_archive: number;
    public direct_source: string;
    public tv_archive_duration: number;
}

export class PlayerApi {
    public user_info: User_Info;
    public server_info: Server_Info;
}
export class Epg_Listings {
    public id: string;
    public epg_id: string;
    public title: string;
    public lang: string;
    public start: string;
    public end: string;
    public description: string;
    public channel_id: string;
    public start_timestamp: string;
    public stop_timestamp: string;
}