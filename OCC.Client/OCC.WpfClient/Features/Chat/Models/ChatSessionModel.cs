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
        }
    }
}
