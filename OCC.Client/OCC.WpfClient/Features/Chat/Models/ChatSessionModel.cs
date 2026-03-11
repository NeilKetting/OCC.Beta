using CommunityToolkit.Mvvm.ComponentModel;
using OCC.Shared.DTOs;
using System;
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
        private bool _isFavourite;

        public string DecryptedAesKey { get; set; } = string.Empty;

        public ObservableCollection<ChatMessageModel> Messages { get; } = new ObservableCollection<ChatMessageModel>();

        public ChatSessionModel(ChatSessionDto dto)
        {
            Dto = dto;
            _name = dto.Name ?? "Chat";
            _isGroupChat = dto.IsGroupChat;
            _unreadCount = dto.UnreadCount;
            _isFavourite = dto.IsFavourite;
            
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
