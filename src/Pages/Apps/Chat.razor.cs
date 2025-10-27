using Microsoft.AspNetCore.Components;

namespace Pocco.Client.Web.Pages.Apps;

partial class Chat : ComponentBase
{
    private string _displayOrgs = "none";
    private readonly List<Organization> _organizations = [];
    private Organization _currentOrganization = null!;

    protected override async Task OnInitializedAsync()
    {
        _organizations.Add(new Organization
        {
            Name = "Pocco",
            Channels = new List<BaseChannel>
            {
                new TextChannel { Name = "general", Topic = "General discussion" },
                new VoiceChannel { Name = "voice-chat", Bitrate = 64000, UserLimit = 10 },
                new CategoryChannel
                {
                    Name = "Categories",
                    Channels = new List<BaseChannel>
                    {
                        new TextChannel { Name = "announcements", Topic = "Announcements" },
                        new VoiceChannel { Name = "support", Bitrate = 32000, UserLimit = 5 }
                    }
                }
            },
            Users = new List<User>
            {
                new User { Name = "Alice", Status = new UserStatus { Status = UserStatusType.Online } },
                new User { Name = "Bob", Status = new UserStatus { Status = UserStatusType.Idle } },
                new User { Name = "Charlie", Status = new UserStatus { Status = UserStatusType.DoNotDisturb } },
                new User { Name = "Dave", Status = new UserStatus { Status = UserStatusType.Offline } }
            }
        });
        _organizations.Add(new Organization
        {
            Name = "ggg-alpha",
            Channels = new List<BaseChannel>
            {
                new CategoryChannel
                {
                    Name = "Categories",
                    Channels = new List<BaseChannel>
                    {
                        new TextChannel { Name = "general", Topic = "General discussion" },
                        new TextChannel { Name = "secret", Topic = "Secret discussion" },
                        new TextChannel { Name = "dev", Topic = "Dev discussion" },
                        new VoiceChannel { Name = "voice-chat", Bitrate = 64000, UserLimit = 10 },
                        new TextChannel { Name = "announcements", Topic = "Announcements" },
                        new VoiceChannel { Name = "support", Bitrate = 32000, UserLimit = 5 }
                    }
                }
            },
            Users = new List<User>
            {
                new User { Name = "Alice", Status = new UserStatus { Status = UserStatusType.Online } },
                new User { Name = "Bob", Status = new UserStatus { Status = UserStatusType.Idle } },
            }
        });
        _organizations.Add(new Organization
        {
            Name = "tech.c.f",
            Channels = new List<BaseChannel>
            {
                new TextChannel { Name = "general", Topic = "General discussion" },
                new TextChannel { Name = "secret", Topic = "Secret discussion" },
                new TextChannel { Name = "dev", Topic = "Dev discussion" }
            },
            Users = new List<User>
            {
                new User { Name = "Alice", Status = new UserStatus { Status = UserStatusType.Online } },
                new User { Name = "Bob", Status = new UserStatus { Status = UserStatusType.Idle } },
                new User { Name = "Dave", Status = new UserStatus { Status = UserStatusType.Offline } }
            }
        });

        _currentOrganization = _organizations.FirstOrDefault() ?? new Organization();
        LoadOrganization(0);

        await base.OnInitializedAsync();
    }

    private void ToggleOrgs()
    {
        _displayOrgs = _displayOrgs == "none" ? "block" : "none";
    }

    private void LoadOrganization(int index)
    {
        if (index < 0 || index >= _organizations.Count)
        {
            return; // Invalid index
        }

        _currentOrganization = _organizations[index];
        _currentOrganization.CurrentChannel = _currentOrganization.Channels.FirstOrDefault()?.Name ?? string.Empty;
        _currentOrganization.CurrentUser = _currentOrganization.Users.FirstOrDefault()?.Name ?? string.Empty;
    }

    private void SelectOrganization(int index)
    {
        if (index < 0 || index >= _organizations.Count)
        {
            return; // Invalid index
        }

        LoadOrganization(index);
        SelectChannel(_currentOrganization);
    }

    private void SelectChannel(Organization org)
    {
        // Clear previous messages
        org.Messages.Clear();

        var rnd = new Random().Next(0, 25);

        // Generate a random message
        for (int i = 0; i < rnd; i++)
        {
            var user = org.Users[new Random().Next(org.Users.Count)];
            var message = new Message
            {
                Content = $"Message {i + 1} from {user.Name}",
                Timestamp = DateTime.Now,
                Author = user
            };
            org.Messages.Add(message);
        }
    }

    private class Organization
    {
        public int Id { get; set; } = new Random().Next(1, 1000);
        public string Name { get; set; } = string.Empty;
        public List<BaseChannel> Channels { get; set; } = new();
        public List<User> Users { get; set; } = new();
        public List<Message> Messages { get; set; } = new();
        public string CurrentChannel { get; set; } = string.Empty;
        public string CurrentUser { get; set; } = string.Empty;
    }

    #region Sample channel
    private class BaseChannel
    {
        public string Name { get; set; } = string.Empty;
        public ChannelType Type { get; set; }
    }

    private class TextChannel : BaseChannel
    {
        public string Topic { get; set; } = string.Empty;
    }
    private class VoiceChannel : BaseChannel
    {
        public int Bitrate { get; set; }
        public int UserLimit { get; set; }
    }
    private class CategoryChannel : BaseChannel
    {
        public List<BaseChannel> Channels { get; set; } = new();
    }

    private enum ChannelType
    {
        Category,
        Text,
        Voice,
    }
    #endregion

    #region Sample user
    private class User
    {
        public string Name { get; set; } = string.Empty;
        public UserStatus Status { get; set; } = new UserStatus
        {
            Status = UserStatusType.Online,
            CustomStatus = string.Empty,
            Emoji = string.Empty
        };
    }

    private class UserStatus
    {
        public UserStatusType Status { get; set; }
        public string CustomStatus { get; set; } = string.Empty;
        public string Emoji { get; set; } = string.Empty;
    }
    private enum UserStatusType
    {
        Online,
        Offline,
        DoNotDisturb,
        Idle,
    }
    #endregion

    #region Sample message
    private class Message
    {
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public User Author { get; set; } = new User();
    }
    #endregion
}
