namespace HIA.Core.Models.Token
{
    public class RoleConstant
    {
        public const string MASTER = "28bc2c50-4bda-49ba-9d88-8628fc6b586a";
        public const string ADMIN = "e399d4b2-8ae6-450b-9ffe-e37b5da0cdda";
        public const string USER = "1c5f3ba0-82d5-4a82-9e66-ee2d6391dbba";
        public const string ADMIN_and_USER = ADMIN + "," + USER;
        public const string MASTER_and_ADMIN = MASTER + "," + ADMIN;
        public const string ALL = MASTER + "," + ADMIN + "," + USER;

        public const string BOYTE = "ae141088-8958-49a9-b119-f741b3a95e14";
    }
}