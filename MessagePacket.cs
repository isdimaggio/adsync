namespace ADSync
{
    internal class MessagePacket
    {
        public string Action { get; set; }

        public string LogonName { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public bool WifiAccess { get; set; }

        public string Group { get; set; }

        public string Password { get; set; }

        public bool SelfManaged { get; set; }

    }
}
