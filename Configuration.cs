using System;
using System.Collections.Generic;
using System.Linq;
namespace ADSync
{
    internal class Configuration
    {
        public string DomainName { get; set; }

        public string NTDomainName { get; set; }

        public string DomainLDAPPath { get; set; }

        public string UserOuLDAPPath { get; set; }

        public string WiFiGroupName { get; set; }

        public string SftpServer { get; set; }

        public string SftpUsername { get; set; }

        public string SftpPassword { get; set; }

        public string HomeFoldersPath { get; set; }

        public string MqttUrl { get; set; }

        public int MqttPort { get; set; }

        public string MqttUsername { get; set;}

        public string MqttPassword { get; set;}

        public string MqttTopicName { get; set; }

    }
}
