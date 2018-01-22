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
            try
            {
                var timer = new System.Threading.Timer(async (e) =>
                {

                    await GlownaMetoda();
                }, null, startTimeSpan, periodTimeSpan);
            }
            catch(Exception ex)
            {

            }









            Console.ReadLine();



        }

        private static async Task GlownaMetoda()
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
                            await client.ConnectAsync("imap.poczta.onet.pl", 993, SecureSocketOptions.SslOnConnect);
                        }
                        catch (Exception ex)
                        {

                        }

                        try
                        {
                            await client.AuthenticateAsync("konrad521@vp.pl", "Chedozycto3!!");
                        }
                        catch (Exception ex)
                        {

                        }
                        var personalNamespace = client.PersonalNamespaces[0];
                        var emailFolders = await GetBoxFolerList(client, personalNamespace);

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
                                    var zad = await Task.Run(() => folder.GetMessageAsync(WiadomoscTemp));
                                    MimeMessage message = zad;
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
                                    try
                                    {
                                      
                                        int n =  await context.SaveChangesAsync();
                                        Console.WriteLine("Rozmiar listy: " + WiadomoscTempi.Count);
                                    }
                                    catch(Exception ex)
                                    {

                                    }
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


        private static async Task PobierzDaneWiadomoscTempi(string nazwaNadawcy, string mailNadawcy, string tematWiadomoscTempi, string trescWiadomoscTempi, string trescHTML)
        {
            WiadomoscTemp nowa = new WiadomoscTemp(nazwaNadawcy, mailNadawcy, tematWiadomoscTempi, trescWiadomoscTempi, trescHTML);
            await Task.Run(() => WiadomoscTempi.Add(nowa));
        }

        private static async Task<List<IMailFolder>> GetBoxFolerList(ImapClient client, FolderNamespace folderNamespace)
        {
            var list = await client.GetFoldersAsync(folderNamespace);
            return list.ToList();
        }

        private static async Task<List<UniqueId>> GetIdsOfUnreadMessages(ImapClient client, IMailFolder folderName)
        {
            var ids = await (client.GetFolder(folderName.Name).SearchAsync(SearchQuery.NotSeen));
            return ids.ToList();

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


        private static async Task PobierzZalaczniki(ImapClient client, IList<MailKit.UniqueId> ids, IMailFolder folder)
        {

            var items = await folder.FetchAsync(ids, MessageSummaryItems.BodyStructure | MessageSummaryItems.UniqueId);

            foreach (var item in items)
            {


                foreach (var attachment in item.Attachments)
                {
                    // octects rozmiar pliku w bajtach
                    var size = attachment.Octets;
                    //Console.WriteLine("Rozmiar zalacznika w bytach: " + size.ToString());
                    //   Console.WriteLine("Nazwa pliku: " + attachment.FileName);
                    // pobieranie zalacznika
                    var entity = await folder.GetBodyPartAsync(item.UniqueId, attachment);
                    using (var stream = File.Create(Path.Combine(sciezka.FullName, Directory.CreateDirectory(sciezka.FullName + "\\" + nadawca).FullName.Trim(' '), attachment.FileName)))
                    {
                        if (entity is MessagePart)
                        {
                            var part = (MessagePart)entity;

                            await part.Message.WriteToAsync(stream);
                        }
                        else
                        {
                            var part = (MimePart)entity;

                            await part.Content.DecodeToAsync(stream);
                        }
                    }
                }
            }

        }
    }


}


