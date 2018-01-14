using System;
using System.Collections.Generic;
using System.Text;

namespace hfa.PlaylistBaseLibrary.Entities.XtreamCode
{
    public class XtreamPanel
    {
        public User_Info User_info { get; set; }
        public Server_Info Server_info { get; set; }
        public Categories Categories { get; set; }
        public Available_Channels Available_channels { get; set; }
    }

    public class User_Info
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public int Auth { get; set; }
        public string Status { get; set; }
        public string Exp_date { get; set; }

        public DateTime? ExpirationDate { get { return Exp_date?.UnixTimeStampToDateTime(); } }

        public string Is_trial { get; set; }
        public bool IsTrial { get { return !string.IsNullOrEmpty(Is_trial) && Active_cons.Equals("1"); } }
        public string Active_cons { get; set; }

        public bool Active { get { return !string.IsNullOrEmpty(Active_cons) && Active_cons.Equals("1"); } }

        public string Created_at { get; set; }
        public DateTime? CreatedDate { get { return Created_at?.UnixTimeStampToDateTime(); } }

        public string Max_connections { get; set; }
        public List<string> Allowed_output_formats { get; set; }
    }

    public class Server_Info
    {
        public string Url { get; set; }
        public string Port { get; set; }
        public string Rtmp_port { get; set; }
        public string Timezone { get; set; }
        public DateTime Time_now { get; set; }
    }

    public class Categories
    {
        public List<Live> Live { get; set; }
    }

    public class Live
    {
        public string Category_id { get; set; }
        public int CategoryId { get { return Convert.ToInt32(Category_id); } }
        public string Category_name { get; set; }
        public int Parent_id { get; set; }
    }

    public class Available_Channels
    {
        public List<Channels> Channels { get; set; }
    }

    public class Channels
    {
        public int Num { get; set; }
        public string Name { get; set; }
        public string Stream_type { get; set; }
        public string Type_name { get; set; }
        public string Stream_id { get; set; }
        public string Stream_icon { get; set; }
        public string Epg_channel_id { get; set; }
        public string Added { get; set; }
        public string Category_name { get; set; }
        public string Category_id { get; set; }
        public string Series_no { get; set; }
        public string Live { get; set; }
        public string Container_extension { get; set; }
        public string Custom_sid { get; set; }
        public int Tv_archive { get; set; }
        public string Direct_source { get; set; }
        public int Tv_archive_duration { get; set; }
    }

    public class PlayerApi
    {
        public User_Info User_info { get; set; }
        public Server_Info Server_info { get; set; }
    }

    public class Epg_Listings
    {
        public string Id { get; set; }
        public string Epg_id { get; set; }
        public string Title { get; set; }
        public string Lang { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public string Description { get; set; }
        public string Channel_id { get; set; }
        public string Start_timestamp { get; set; }
        public string Stop_timestamp { get; set; }
    }

}
