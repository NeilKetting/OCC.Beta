using CommunityToolkit.Mvvm.ComponentModel;
using OCC.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace OCC.WpfClient.Features.Chat.Models
{
    public partial class ChatSessionModel : ObservableObject
    {
        public ChatSessionDto Dto { get; }

        public Guid Id => Dto.Id;
        
        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private bool _isGroupChat;

        [ObservableProperty]
        private string _lastMessagePreview;

        [ObservableProperty]
        private DateTime _lastMessageTime;

        [ObservableProperty]
        private int _unreadCount;

        [ObservableProperty]
        private bool _isCurrentUserAdmin;

        [ObservableProperty]
        private bool _isFavourite;

        [ObservableProperty]
        private string _initials;

        [ObservableProperty]
        private string _badgeColor;

        [ObservableProperty]
        private string? _profilePictureUrl;

        public Guid CreatedById => Dto.CreatedById;
        public List<ChatUserDto> Users => Dto.Users;

        public bool IsAdmin(Guid userId) => Dto.CreatedById == userId;

        public string DecryptedAesKey { get; set; } = string.Empty;

        public ObservableCollection<ChatMessageModel> Messages { get; } = new ObservableCollection<ChatMessageModel>();

        private readonly Guid _currentUserId;

        public ChatSessionModel(ChatSessionDto dto, Guid currentUserId)
        {
            Dto = dto;
            _currentUserId = currentUserId;
            _isGroupChat = dto.IsGroupChat;
            _unreadCount = dto.UnreadCount;
            _isFavourite = dto.IsFavourite;

            if (!_isGroupChat && dto.Users.Count >= 2)
            {
                var otherUser = dto.Users.FirstOrDefault(u => u.UserId != _currentUserId);
                _name = otherUser != null ? $"{otherUser.FirstName} {otherUser.LastName}" : (dto.Name ?? "Chat");
            }
            else
            {
                _name = dto.Name ?? "Chat";
            }
            
            if (dto.LastMessage != null)
            {
                _lastMessagePreview = dto.LastMessage.HasAttachment ? "📎 Attachment" : dto.LastMessage.Content;
                _lastMessageTime = dto.LastMessage.SentDate;
            }
            else
            {
                _lastMessagePreview = "";
                _lastMessageTime = dto.CreatedAtUtc;
            }

            // Set Initials and Color
            UpdateMetadata();
        }

        private void UpdateMetadata()
        {
            if (!_isGroupChat && Dto.Users.Count >= 2)
            {
                var otherUser = Dto.Users.FirstOrDefault(u => u.UserId != _currentUserId);
                if (otherUser != null)
                {
                    Initials = GetInitials(otherUser.FirstName, otherUser.LastName);
                    BadgeColor = GetColorFromGuid(otherUser.UserId);
                    // ProfilePictureUrl = otherUser.ProfilePictureUrl; // Not in Dto yet, but ready
                }
            }
            else
            {
                Initials = GetInitials(Name, "");
                BadgeColor = GetColorFromGuid(Id);
            }
        }

        private string GetInitials(string first, string last)
        {
            if (string.IsNullOrWhiteSpace(first)) return "?";
            if (string.IsNullOrWhiteSpace(last)) return first[0].ToString().ToUpper();
            return (first[0].ToString() + last[0].ToString()).ToUpper();
        }

        private string GetColorFromGuid(Guid id)
        {
            // Curated harmonious color palette (HSL tailored)
            string[] colors = { 
                "#0ea5e9", // Sky 500
                "#8b5cf6", // Violet 500
                "#d946ef", // Fuchsia 500
                "#f43f5e", // Rose 500
                "#f97316", // Orange 500
                "#eab308", // Yellow 500
                "#22c55e", // Green 500
                "#06b6d4"  // Cyan 500
            };
            
            var hash = id.ToByteArray().Sum(b => (int)b);
            return colors[hash % colors.Length];
        }
    }
}
