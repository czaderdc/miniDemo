using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using ConsoleApp7;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MailKit.Security;
using MimeKit;
using MimeKit.Utils;
namespace ConsoleApp5
{

    class User
    {
        public string EmailAddress { get; set; }
        public string Password { get; set; }
        public string ClientType { get; set; }
    }

    class Program
    {
        static List<WiadomoscTemp> WiadomoscTempi = new List<WiadomoscTemp>();
        static string nadawca = "";
        static string nadawcaMail = "";
        static DirectoryInfo sciezka = Directory.CreateDirectory("C:\\folderPlikiMaile");

        static void Main(string[] args)
        {
            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromMinutes(30);

            var timer = new System.Threading.Timer((e) =>
            {

                GlownaMetoda();
            }, null, startTimeSpan, periodTimeSpan);









            Console.ReadLine();



        }

        private static void GlownaMetoda()
        {
            // WiadomoscTempi = null;
            //  WiadomoscTempi = new List<WiadomoscTemp>();
            //System.GC.Collect();
            //GC.WaitForPendingFinalizers();


            var user = new User();
            user.ClientType = (ClientType.IMAP);
            //user.ClientType = (ClientType.POP3);

            Console.WriteLine("Typ klienta: " + user.ClientType);
            using (var context = new ConsoleApp7.MyDBContext())
            {

                if (user.ClientType == ClientType.IMAP)
                {
                    Console.WriteLine("Lacze sie przez IMAP :D:D:D");
                    using (var client = new ImapClient())
                    {
                        Console.WriteLine("Rozmiar listy: " + WiadomoscTempi.Count);
                        Console.WriteLine("Loguje sie.....");
                        try
                        {
                            client.Connect("pop3.poczta.onet.pl", 995, SecureSocketOptions.SslOnConnect);
                        }
                        catch (Exception ex)
                        {

                        }

                        try
                        {
                            client.Authenticate("konrad521@vp.pl", "Chedodsa!");
                        }
                        catch (Exception ex)
                        {

                        }
                        var personalNamespace = client.PersonalNamespaces[0];
                        var emailFolders = GetBoxFolerList(client, personalNamespace);

                        foreach (var folder in emailFolders)
                        {//readwrite, bo musze oznaczyc jako otwarte przetworzone juz pliki

                            var readingFolder = client.GetFolder(folder.Name).Open(FolderAccess.ReadWrite);
                            Console.WriteLine("Nazwa folderu: " + folder.Name);
                            int liczbaNieprzeczytanych = folder.Unread;
                            int liczbaWiadomoscTempi = folder.Count;
                            Console.WriteLine("Liczba plikow: " + client.GetFolder(folder.Name).Count);
                            //interesuja mnie tylko nieprzeczytane
                            IList<MailKit.UniqueId> idNieprzeczytanychWiadomoscTempi = client.GetFolder(folder.Name).Search(SearchQuery.All);
                            if (idNieprzeczytanychWiadomoscTempi.Count != 0)
                            {
                                //WiadomoscTemp to 
                                foreach (var WiadomoscTemp in idNieprzeczytanychWiadomoscTempi)
                                {

                                    MimeMessage message = folder.GetMessage(WiadomoscTemp);
                                    if (SprawdzCzyJestWBazie(context, message.MessageId))
                                        continue;
                                    // Console.WriteLine(message.TextBody);
                                    PobierzNadawceWiadomoscTempi(message.From.Mailboxes);
                                    //PobierzDaneWiadomoscTempi(nadawca, nadawcaMail, folder.GetMessage(WiadomoscTemp).Subject,
                                    //folder.GetMessage(WiadomoscTemp).TextBody, folder.GetMessage(WiadomoscTemp).HtmlBody);
                                    Wiadomosc wiadomosc = new Wiadomosc();
                                    wiadomosc.MailNadawcy = nadawcaMail;
                                    wiadomosc.Nadawca = nadawca;
                                    wiadomosc.TrescWiadomoscTempiHTML = folder.GetMessage(WiadomoscTemp).HtmlBody;
                                    wiadomosc.Temat = folder.GetMessage(WiadomoscTemp).Subject;
                                    wiadomosc.MessageID = message.MessageId;
                                    context.Wiadomosci.Add(wiadomosc);
                                    context.SaveChanges();
                                    Console.WriteLine("Rozmiar listy: " + WiadomoscTempi.Count);
                                    //po przetworzeniu oznaczyc jako przeczytany
                                    folder.SetFlags(WiadomoscTemp, MessageFlags.Seen, true);

                                    // if(folder.GetMessage(WiadomoscTemp).Attachments.Count() > 0)
                                    // PobierzZalaczniki(client, idNieprzeczytanychWiadomoscTempi, client.GetFolder(folder.Name));



                                }

                            }
                        }
                    }
                }


                else if (user.ClientType == ClientType.POP3)
                {
                    Console.WriteLine("Lacze sie przez pop3!~");
                }
            }


            Console.WriteLine("Wylogowany....");

        }


        private static void PobierzDaneWiadomoscTempi(string nazwaNadawcy, string mailNadawcy, string tematWiadomoscTempi, string trescWiadomoscTempi, string trescHTML)
        {
            WiadomoscTemp nowa = new WiadomoscTemp(nazwaNadawcy, mailNadawcy, tematWiadomoscTempi, trescWiadomoscTempi, trescHTML);
            WiadomoscTempi.Add(nowa);
        }

        private static List<IMailFolder> GetBoxFolerList(ImapClient client, FolderNamespace folderNamespace)
        {
            return client.GetFolders(folderNamespace).ToList();
        }

        private static List<UniqueId> GetIdsOfUnreadMessages(ImapClient client, IMailFolder folderName)
        {
            return client.GetFolder(folderName.Name).Search(SearchQuery.NotSeen).ToList();
        }

        private static void PobierzNadawceWiadomoscTempi(IEnumerable<MailboxAddress> adres)
        {

            foreach (var mailbox in adres)
            {
                nadawca = mailbox.Name;
                nadawcaMail = mailbox.Address;
                //  Console.WriteLine(mailbox.Name);
                // Console.WriteLine(mailbox.Address);
            }

        }


        private static bool SprawdzCzyJestWBazie(MyDBContext context, string MessageID)
        {
            var query = context.Wiadomosci.Where(w => w.MessageID == MessageID).Count();
            if (query > 0)
            {
                Console.WriteLine("Ta wiadomosc jest juz zapisana w bazie!");
                return true;
            }

            return false;

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
                    //Console.WriteLine("Rozmiar zalacznika w bytach: " + size.ToString());
                    //   Console.WriteLine("Nazwa pliku: " + attachment.FileName);
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


