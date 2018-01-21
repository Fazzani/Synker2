using System.Collections.Generic;

namespace hfa.Synker.Service.Services.Notification
{
    public interface ISendpulse
    {
        Dictionary<string, object> ActivateSender(string email, string code);
        Dictionary<string, object> AddEmails(int bookId, string emails);
        Dictionary<string, object> AddSender(string senderName, string senderEmail);
        Dictionary<string, object> AddToBlackList(string emails);
        Dictionary<string, object> CampaignCost(int bookId);
        Dictionary<string, object> CampaignStatByCountries(int id);
        Dictionary<string, object> CampaignStatByReferrals(int id);
        Dictionary<string, object> CancelCampaign(int id);
        Dictionary<string, object> CreateAddressBook(string bookName);
        Dictionary<string, object> CreateCampaign(string senderName, string senderEmail, string subject, string body, int bookId, string name, string send_date = "", string attachments = "");
        Dictionary<string, object> CreatePushTask(Dictionary<string, object> taskinfo, Dictionary<string, object> additionalParams);
        Dictionary<string, object> EditAddressBook(int id, string newname);
        Dictionary<string, object> EmailStatByCampaigns(string email);
        Dictionary<string, object> GetBalance(string currency);
        Dictionary<string, object> GetBlackList();
        Dictionary<string, object> GetBookInfo(int id);
        Dictionary<string, object> GetCampaignInfo(int id);
        Dictionary<string, object> GetEmailGlobalInfo(string email);
        Dictionary<string, object> GetEmailInfo(int bookId, string email);
        Dictionary<string, object> GetEmailsFromBook(int id);
        Dictionary<string, object> GetSenderActivationMail(string email);
        Dictionary<string, object> ListAddressBooks(int limit, int offset);
        Dictionary<string, object> ListCampaigns(int limit, int offset);
        Dictionary<string, object> ListSenders();
        Dictionary<string, object> PushCampaignInfo(int id);
        Dictionary<string, object> PushCountWebsites();
        Dictionary<string, object> PushCountWebsiteSubscriptions(int id);
        Dictionary<string, object> PushListCampaigns(int limit, int offset);
        Dictionary<string, object> PushListWebsites(int limit, int offset);
        Dictionary<string, object> PushListWebsiteSubscriptions(int id, int limit, int offset);
        Dictionary<string, object> PushListWebsiteVariables(int id);
        Dictionary<string, object> PushSetSubscriptionState(int id, int state);
        Dictionary<string, object> RemoveAddressBook(int id);
        Dictionary<string, object> RemoveEmailFromAllBooks(string email);
        Dictionary<string, object> RemoveEmails(int bookId, string emails);
        Dictionary<string, object> RemoveFromBlackList(string emails);
        Dictionary<string, object> RemoveSender(string email);
        Dictionary<string, object> SmtpAddDomain(string email);
        Dictionary<string, object> SmtpGetEmailInfoById(string id);
        Dictionary<string, object> SmtpListAllowedDomains();
        Dictionary<string, object> SmtpListEmails(int limit, int offset, string fromDate, string toDate, string sender, string recipient);
        Dictionary<string, object> SmtpListIP();
        Dictionary<string, object> SmtpRemoveFromUnsubscribe(string emails);
        Dictionary<string, object> SmtpSendMail(Dictionary<string, object> emaildata);
        Dictionary<string, object> SmtpUnsubscribeEmails(string emails);
        Dictionary<string, object> SmtpVerifyDomain(string email);
    }
}