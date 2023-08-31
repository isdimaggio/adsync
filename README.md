# adsync
Sincronizza le utenze di LampSchool con un Domain Controller Active Directory tramite MQTT (broker non incluso).   
Prevede l'utilizzo di un fileserver QNAP QTS.   
Per utilizzarlo cambiare nel file `Program.cs` il percorso della configurazione.

Compilare successivamente il file JSON di configurazione seguendo questo schema:
```json
{
  "DomainName": "EXAMPLE",
  "NTDomainName": "example.com",
  "DomainLDAPPath": "DC=example,DC=com",
  "UserOuLDAPPath": "OU=Sottocartella,OU=Cartella",
  "WiFiGroupName": "NomeGruppo",
  "SftpServer": "nome_o_ip",
  "SftpUsername": "username",
  "SftpPassword": "password",
  "HomeFoldersPath": "/share/CACHEDEV1_DATA/homes/DOMAIN=EXAMPLE/",
  "MqttUrl": "indirizzoip",
  "MqttPort": 1883,
  "MqttUsername": "username",
  "MqttPassword": "password",
  "MqttTopicName": "topic"
}
```
