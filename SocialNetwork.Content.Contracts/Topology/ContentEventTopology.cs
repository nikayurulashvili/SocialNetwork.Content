namespace SocialNetwork.Content.Contracts.Topology;

/// <summary>
/// Public RabbitMQ contract for services that publish to or consume from Content.
/// All Content integration events use a durable topic exchange.
/// </summary>
public static class ContentEventTopology
{
    public const string Exchange = "socialnetwork.events";
    public const string SourceService = "Content";

    public static class Users
    {
        // Events published by the Auth service and consumed by Content.
        public const string Created = "user.created";
        public const string Updated = "user.updated";
        public const string Deleted = "user.deleted";
    }

    public static class Posts
    {
        public const string Created = "post.created";
        public const string Updated = "post.updated";
        public const string Deleted = "post.deleted";
        public const string Liked = "post.liked";
        public const string Unliked = "post.unliked";
    }

    public static class Comments
    {
        public const string Created = "comment.created";
        public const string Updated = "comment.updated";
        public const string Deleted = "comment.deleted";
    }
}
