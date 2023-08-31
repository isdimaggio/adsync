using System.DirectoryServices.AccountManagement;
using System.Text.Json;
using ADSync;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt;

for(int i = 0; i < 4; i++) { Console.WriteLine(); } // spazio per i log
Console.WriteLine("Started up at " + DateTime.Now.ToString());

Configuration configuration;

try
{
    string jsonFilePath = @"C:\tools\adsync\config.json";
    string jsonContent = File.ReadAllText(jsonFilePath);
    // deserializza json configurazione
    configuration = JsonSerializer.Deserialize<Configuration>(jsonContent);
}
catch (Exception ex)
{
    Console.WriteLine("Cannot read configuration: " + ex.ToString());
    return;
}

PrincipalContext userPrincipalContext;
PrincipalContext groupPrincipalContext;
try
{
    // contesto utenti (cartella del self managed)
    userPrincipalContext = new PrincipalContext(
        ContextType.Domain, configuration.DomainName, configuration.UserOuLDAPPath + "," + configuration.DomainLDAPPath);

    // contesto gruppi (il dominio intero)
    groupPrincipalContext = new PrincipalContext(
        ContextType.Domain, configuration.DomainName, configuration.DomainLDAPPath);
}
catch (Exception e)
{
    Console.WriteLine("Domain authentication falied. Exception: " + e.ToString());
    return;
}

UserOperations adClient = new UserOperations(
    userPrincipalContext, groupPrincipalContext, configuration.NTDomainName, configuration.WiFiGroupName);

FolderOperations sftpClient;

try
{
    sftpClient = new FolderOperations(
        configuration.SftpServer, configuration.SftpUsername, configuration.SftpPassword, configuration.HomeFoldersPath);
}
catch (Exception ex)
{
    Console.WriteLine("SFTP authentication falied. Exception: " + ex.ToString());
    return;
}

// accedi al broker mqtt
MqttClient mqttClient = new MqttClient(
    configuration.MqttUrl, configuration.MqttPort, false, null, null, MqttSslProtocols.None);

// subba i topic e definisci le funzioni di callback
string[] topics = { configuration.MqttTopicName };
byte[] qosLevels = { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE };

mqttClient.MqttMsgSubscribed += (sender, e) =>
{
    Console.WriteLine($"Mqtt connection successful: {e.MessageId}");
};

// funzione di callback per gestire i messaggi in arrivo
// la libreria dovrebbe (????) spawnare un nuovo thread per ogni messaggio che muore a fine elaborazione
mqttClient.MqttMsgPublishReceived += (sender, e) =>
{
    string message = System.Text.Encoding.UTF8.GetString(e.Message);
    List<MessagePacket> packets = new List<MessagePacket>(); // deserializza la lista di operazioni (pacchetti)
    try
    {
        packets = JsonSerializer.Deserialize<List<MessagePacket>>(message);
    }
    catch (Exception ex)
    {
        Console.WriteLine("Tried parsing invalid json: " + ex.ToString());
    }
    foreach (MessagePacket packet in packets)
    {
        // in base all'azione richiesta chiama le funzioni
        switch(packet.Action)
        {
            case "CreateUpdate":
                adClient.CreateOrUpdateUser(packet.LogonName, packet.FirstName, packet.LastName, packet.WifiAccess, packet.Group, packet.Password); 
                break;
            case "UpdatePassword":
                adClient.UpdatePassword(packet.LogonName, packet.Password, packet.SelfManaged);
                break;
            case "Delete":
                // per il delete serve doppia chiamata siccome le homes sono gestite solo dal NAS
                adClient.DeleteUser(packet.LogonName);
                sftpClient.DeleteUserHomeFolder(packet.LogonName);
                break;
        }
    }
};

mqttClient.Connect(
    Guid.NewGuid().ToString(), configuration.MqttUsername, configuration.MqttPassword); // stabilisci il loop di connessione
mqttClient.Subscribe(topics, qosLevels);
