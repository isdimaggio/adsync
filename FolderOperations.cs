using Renci.SshNet;

namespace ADSync
{
    internal class FolderOperations
    {
        SshClient client;
        string homeFoldersPath;

        public FolderOperations(string serverName, string userName, string password, string homeFoldersPath)
        {
            var connectionInfo = new ConnectionInfo(
                serverName, userName, new PasswordAuthenticationMethod(userName, password));
            client = new SshClient(connectionInfo);
            client.Connect();
            client.KeepAliveInterval = new TimeSpan(0, 3, 0);
            this.homeFoldersPath = homeFoldersPath;
        }

        public bool DeleteUserHomeFolder(string logonName)
        {
            try
            {
                if(!client.IsConnected) client.Connect();
                client.CreateCommand("rm -rf " + homeFoldersPath + logonName).Execute();
                Console.WriteLine("Home folder deleted for user " + logonName);
                return true;
            }
            catch (Exception ex) 
            {
                Console.WriteLine("Error while deleting home folder for user " + logonName + " ... " + ex.ToString());
                return false;
            }
        }
    }
}
