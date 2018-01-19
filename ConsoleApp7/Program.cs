using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MailKit.Security;
using MimeKit;
using MimeKit.Utils;
namespace ConsoleApp5
{
    class Program
    {
        static List<Wiadomosc> Wiadomosci = new List<Wiadomosc>();
        static string nadawca = "";
        static string nadawcaMail = "";
        static DirectoryInfo sciezka = Directory.CreateDirectory("C:\\folderPlikiMaile");
        static void Main(string[] args)
        {
            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromMinutes(250);

            var timer = new System.Threading.Timer((e) =>
            {
                GlownaMetoda();
            }, null, startTimeSpan, periodTimeSpan);









            Console.ReadLine();

        

        }

        private static void GlownaMetoda()
        {
            using (var client = new ImapClient())
            {
                Console.WriteLine("Loguje sie.....");
                client.Connect("server", 993, SecureSocketOptions.SslOnConnect);


                client.Authenticate("emai", "haslo");
                var personalNamespace = client.PersonalNamespaces[0];
                var emailFolders = client.GetFolders(personalNamespace);

                foreach (var folder in emailFolders)
                {//readwrite, bo musze oznaczyc jako otwarte przetworzone juz pliki
                  
                   var readingFolder = client.GetFolder(folder.Name).Open(FolderAccess.ReadWrite);
                    Console.WriteLine("Nazwa folderu: " + folder.Name);
                    int liczbaNieprzeczytanych = folder.Unread;
                    int liczbaWiadomosci = folder.Count;
                    Console.WriteLine("Liczba plikow: " + client.GetFolder(folder.Name).Count);
                    //interesuja mnie tylko nieprzeczytane
                    IList<MailKit.UniqueId> idNieprzeczytanychWiadomosci = client.GetFolder(folder.Name).Search(SearchQuery.NotSeen);
                    if (idNieprzeczytanychWiadomosci.Count != 0)
                    {
                        //wiadomosc to uid wiadomosci....
                        foreach (var wiadomosc in idNieprzeczytanychWiadomosci)
                        {
                            
                            MimeMessage message = folder.GetMessage(wiadomosc);
                            Console.WriteLine(message.TextBody);
                            PobierzNadawceWiadomosci(message.From.Mailboxes);
                            PobierzDaneWiadomosci(nadawca, nadawcaMail, folder.GetMessage(wiadomosc).Subject,
                                folder.GetMessage(wiadomosc).TextBody, folder.GetMessage(wiadomosc).HtmlBody);
                            //po przetworzeniu oznaczyc jako przeczytany
                            folder.SetFlags(wiadomosc, MessageFlags.Seen, true);

                            if(folder.GetMessage(wiadomosc).Attachments.Count() > 0)
                            PobierzZalaczniki(client, idNieprzeczytanychWiadomosci, client.GetFolder(folder.Name));



                        }
                        
                    }
                }
             
                Console.WriteLine("Wylogowany....");
            }
        }

        private static void PobierzDaneWiadomosci(string nazwaNadawcy, string mailNadawcy, string tematWiadomosci, string trescWiadomosci, string trescHTML)
        {
            Wiadomosc nowa = new Wiadomosc(nazwaNadawcy, mailNadawcy, tematWiadomosci, trescWiadomosci, trescHTML);
            Wiadomosci.Add(nowa);
        }

        private static void PobierzNadawceWiadomosci(IEnumerable<MailboxAddress> adres)
        {

            foreach (var mailbox in adres)
            {
                nadawca = mailbox.Name;
                nadawcaMail = mailbox.Address;
                Console.WriteLine(mailbox.Name);
                Console.WriteLine(mailbox.Address);
            }

        }



    

        private static void PobierzZalaczniki(ImapClient client, IList<MailKit.UniqueId> ids, IMailFolder folder)
        {

            var items = folder.Fetch(ids, MessageSummaryItems.BodyStructure | MessageSummaryItems.UniqueId);

            foreach (var item in items)
            {

              
                foreach (var attachment in item.Attachments)
                {
                    // octects rozmiar pliku w bajtach
                    var size = attachment.Octets;
                    Console.WriteLine("Rozmiar zalacznika w bytach: " + size.ToString());
                    Console.WriteLine("Nazwa pliku: " + attachment.FileName);
                    // pobieranie zalacznika
                    var entity = folder.GetBodyPart(item.UniqueId, attachment);

                   
                    using (var stream = File.Create(Path.Combine(sciezka.FullName, Directory.CreateDirectory(sciezka.FullName + "\\" + nadawca).FullName.Trim(' '), attachment.FileName)))
                    {
                        if (entity is MessagePart)
                        {
                            var part = (MessagePart)entity;

                            part.Message.WriteTo(stream);
                        }
                        else
                        {
                            var part = (MimePart)entity;

                            part.Content.DecodeTo(stream);
                        }
                    }
                }
            }

        }
    }

    
}


