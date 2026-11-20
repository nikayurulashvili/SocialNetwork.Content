using System;
using System.Collections.Generic;
using System.Text;

namespace SocialNetwork.Content.Contracts.DTOs.Requests;

public class LikeRequest
{
    public Guid PostId { get; set; }
    public Guid UserId { get; set; }
    public bool IsLiked { get; set; }

    public int LikesCount { get; set; }
}

    
