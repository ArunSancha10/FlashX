namespace outofoffice.Graph
{
    /// <summary>
    /// Contains constants used by Microsoft Graph-related code.
    /// </summary>
    public static class GraphConstants
    {
        /// <summary>
        /// Defines the permission scopes used by the app.
        /// </summary>
        public static readonly string[] Scopes =
        {
            "User.Read",
            "MailboxSettings.ReadWrite",
            "Sites.ReadWrite.All",
            "Team.ReadBasic.All",
            "Channel.ReadBasic.All",
            "Sites.Manage.All",
            "offline_access"
                
        };

        // Default page size for collections
        public const int PageSize = 25;

        // User
        public const string UserRead = "User.Read";
        public const string UserReadBasicAll = "User.ReadBasic.All";
        public const string UserReadAll = "User.Read.All";
        public const string UserReadWrite = "User.ReadWrite";
        public const string UserReadWriteAll = "User.ReadWrite.All";

        // Group
        public const string GroupReadWriteAll = "Group.ReadWrite.All";

        // Teams
        public const string ChannelCreate = "Channel.Create";
        public const string ChannelMessageSend = "ChannelMessage.Send";
        public const string ChannelSettingsReadWriteAll = "ChannelSettings.ReadWrite.All";
        public const string TeamsAppInstallationReadWriteForTeam = "TeamsAppInstallation.ReadWriteForTeam";
        public const string TeamCreate = "Team.Create";
        public const string TeamSettingsReadWriteAll = "TeamSettings.ReadWrite.All";

        // Mailbox settings
        public const string MailboxSettingsRead = "MailboxSettings.Read";
        public const string MailboxSettingsReadWrite = "MailboxSettings.ReadWrite";

        // Mail
        public const string MailRead = "Mail.Read";
        public const string MailReadWrite = "Mail.ReadWrite";
        public const string MailSend = "Mail.Send";

        // Calendar
        public const string CalendarReadWrite = "Calendars.ReadWrite";

        // Files
        public const string FilesReadWrite = "Files.ReadWrite";
        public const string FilesReadWriteAll = "Files.ReadWrite.All";

        // Sites
        public const string SitesReadWriteAll = "Sites.ReadWrite.All";
        public const string SitesManageAll = "Sites.Manage.All";

        // Errors
        public const string ItemNotFound = "ErrorItemNotFound";
        public const string RequestDenied = "Authorization_RequestDenied";
        public const string RequestResourceNotFound = "Request_ResourceNotFound";
        public const string ResourceNotFound = "ResourceNotFound";
    }

}
