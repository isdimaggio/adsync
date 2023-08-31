using System.DirectoryServices.AccountManagement;

namespace ADSync
{
    internal class UserOperations
    {

        PrincipalContext userPrincipalContext;
        PrincipalContext groupPrincipalContext;
        string domainName;
        string wifiGroupName;

        public UserOperations(
            PrincipalContext userPrincipalContext, 
            PrincipalContext groupPrincipalContext,
            string domainName,
            string wifiGroupName
            )
        {
            this.userPrincipalContext = userPrincipalContext;
            this.groupPrincipalContext = groupPrincipalContext;
            this.domainName = "@" + domainName;
            this.wifiGroupName = wifiGroupName;
        }

        public bool CreateOrUpdateUser(
            string logonName,
            string firstName,
            string lastName,
            bool wifiAccess,
            string mainGroup,
            string password = ""
            )
        {
            UserPrincipal usr = UserPrincipal.FindByIdentity(userPrincipalContext, logonName);
            if (usr != null) //  esiste gia
            {
                // (re)imposta first e last name
                if (lastName == null | lastName.Length < 1)
                {
                    Console.WriteLine("Invalid last name while updating user " + logonName);
                    return false;
                }
                if (firstName == null | firstName.Length < 1)
                {
                    Console.WriteLine("Invalid first name while updating user " + logonName);
                    return false;
                }

                usr.GivenName = firstName;
                usr.Surname = lastName;
                usr.DisplayName = firstName + " " + lastName;

                usr.Save();

                // aggiorna lo stato di wifiaccess

                try
                {
                    if (wifiAccess)
                    {
                        GroupPrincipal wifiGroup = GroupPrincipal.FindByIdentity(groupPrincipalContext, wifiGroupName);
                        wifiGroup.Members.Add(groupPrincipalContext, IdentityType.SamAccountName, logonName);
                        wifiGroup.Save();

                    }
                    else
                    {
                        GroupPrincipal wifiGroup = GroupPrincipal.FindByIdentity(groupPrincipalContext, wifiGroupName);
                        wifiGroup.Members.Remove(groupPrincipalContext, IdentityType.SamAccountName, logonName);
                        wifiGroup.Save();
                    }
                } catch { } // ignora qualsiasi eccezione (uno dei due blocchi da per forza errore siccome aggiorna alla cieca)

                Console.WriteLine("Successfully updated user " + logonName);
                return true;
            }

            // utente non esiste, effettua impostazione iniziale

            UserPrincipal userPrincipal = new UserPrincipal(userPrincipalContext);

            if (lastName == null | lastName.Length < 1) 
            {
                Console.WriteLine("Invalid last name while creating user " + logonName);
                return false;
            }
            if (firstName == null | firstName.Length < 1) 
            {
                Console.WriteLine("Invalid first name while creating user " + logonName);
                return false; 
            }
            if (logonName == null | logonName.Length < 1) 
            {
                Console.WriteLine("Invalid logon name while creating user " + logonName);
                return false; 
            }

            userPrincipal.GivenName = firstName;
            userPrincipal.EmailAddress = logonName + domainName;
            userPrincipal.SamAccountName = logonName;
            userPrincipal.Surname = lastName;
            userPrincipal.DisplayName = firstName + " " + lastName;
            userPrincipal.UserPrincipalName = logonName + domainName;

            // gestione della password

            if (password != "" && password != null)
            {
                if(password.Length < 8)
                {
                    Console.WriteLine("Password too short while creating user " + logonName);
                    return false;
                }
                // gestita autonomamente, impostala
                userPrincipal.SetPassword(password);
            }
            else
            {
                // gestita da servizi esterni (il campo password viene lasciato vuoto)
                userPrincipal.SetPassword(Guid.NewGuid().ToString()); // totalmente random (da impostare con call successiva)
                userPrincipal.UserCannotChangePassword = true;
            }

            userPrincipal.PasswordNeverExpires = true;
            userPrincipal.Enabled = true;

            try
            {
                userPrincipal.Save(); // salva dati utente
            }
            catch (Exception ex) 
            {
                Console.WriteLine("Exception raised while creating user " + logonName + " ... " + ex.ToString());
                return false;
            }

            // aggiunta a gruppo di appartenenza
            try
            {
                GroupPrincipal group = GroupPrincipal.FindByIdentity(groupPrincipalContext, mainGroup);
                group.Members.Add(groupPrincipalContext, IdentityType.SamAccountName, logonName);
                group.Save();

                if (wifiAccess) // se ha l'accesso wifi provvedi
                {
                    GroupPrincipal wifiGroup = GroupPrincipal.FindByIdentity(groupPrincipalContext, "AccessoWiFi");
                    wifiGroup.Members.Add(groupPrincipalContext, IdentityType.SamAccountName, logonName);
                    wifiGroup.Save();

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception raised while setting main group for user " + logonName + " ... " + ex.ToString());
                return false;
            }

            Console.WriteLine("Successfully created user " + logonName);
            return true;
        }

        public bool UpdatePassword(
            string logonName, string password, bool selfManaged = true)
        {
            try
            {
                UserPrincipal usr = UserPrincipal.FindByIdentity(userPrincipalContext, logonName);
                if (usr == null)
                {
                    Console.WriteLine("Cannot update password for non existing user " + logonName);
                    return false;
                }

                if (password == "" | password == null | password.Length < 8)
                {
                    Console.WriteLine("Password too short while updating user " + logonName);
                    return false;
                }

                usr.SetPassword(password);
                usr.PasswordNeverExpires = true;
                usr.Enabled = true;

                if (!selfManaged) usr.UserCannotChangePassword = true;

                usr.Save();
                Console.WriteLine("Password successfully updated user " + logonName);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception raised while updating password of user " + logonName + " ... " + ex.ToString());
                return false;
            }
        }

        public bool DeleteUser(string logonName)
        {
            try
            {
                UserPrincipal usr = UserPrincipal.FindByIdentity(userPrincipalContext, logonName);
                if (usr == null)
                {
                    Console.WriteLine("Cannot delete non existing user " + logonName);
                    return false;
                }

                usr.Delete();
                Console.WriteLine("Successfully deleted user " + logonName);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception raised while deleting user " + logonName + " ... " + ex.ToString());
                return false;
            }
        }
    }
}
